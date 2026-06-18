using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using FrameAnalysis.UI.Core.Rendering;

namespace FrameAnalysis.UI
{
    /// <summary>
    /// The only file that touches a WPF <see cref="Canvas"/>. Draws a <see cref="Scene"/> via a
    /// <see cref="Viewport"/> for world-&gt;screen, and supports wheel-zoom, right-drag pan, and
    /// left-click pick (the canvas half of the hybrid selection model). Swapping to a 3D
    /// renderer means replacing this class — nothing upstream (document, mapper, scene) changes.
    /// </summary>
    public sealed class Wpf2DCanvasRenderer : IStructuralRenderer
    {
        private static readonly Brush MemberBrush = Brushes.Black;
        private static readonly Brush SelectedBrush = Brushes.OrangeRed;
        private static readonly Brush NodeBrush = Brushes.SteelBlue;
        private static readonly Brush SupportBrush = Brushes.DimGray;
        private static readonly Brush LoadBrush = Brushes.DarkRed;
        private static readonly Brush DistLoadBrush = Brushes.DarkOrange;
        private static readonly Brush ReactionBrush = Brushes.SeaGreen;
        private static readonly Brush DeflectedBrush = Brushes.MediumPurple;

        private readonly Canvas _canvas;
        private readonly Viewport _viewport = new();
        private readonly List<(int Id, Point A, Point B)> _memberSegments = new();

        private Scene _scene = Scene.Empty;
        private int? _selectedElementId;
        private bool _hasFitted;

        private Point _lastPanPoint;
        private bool _isPanning;

        /// <summary>Raised when the user clicks a member (or empty space → null).</summary>
        public event EventHandler<int?>? ElementPicked;

        public Wpf2DCanvasRenderer(Canvas canvas)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
            _canvas.Background = Brushes.White;
            _canvas.ClipToBounds = true;

            _canvas.MouseWheel += OnMouseWheel;
            _canvas.MouseLeftButtonDown += OnMouseLeftButtonDown;
            _canvas.MouseRightButtonDown += OnPanStart;
            _canvas.MouseMove += OnMouseMove;
            _canvas.MouseRightButtonUp += OnPanEnd;
            _canvas.SizeChanged += OnSizeChanged;
        }

        public void Render(Scene scene)
        {
            _scene = scene ?? Scene.Empty;

            // Auto-fit the first time we have both a scene and a sized canvas; afterwards the
            // user's zoom/pan is preserved across live edits.
            if (!_hasFitted && _canvas.ActualWidth > 0 && _canvas.ActualHeight > 0)
            {
                _viewport.FitToBounds(_scene.Bounds, _canvas.ActualWidth, _canvas.ActualHeight);
                _hasFitted = true;
            }

            Draw();
        }

        public int? HitTestElement(double screenX, double screenY)
        {
            const double threshold = 6.0;
            var p = new Point(screenX, screenY);
            int? best = null;
            double bestDistance = threshold;

            foreach ((int id, Point a, Point b) in _memberSegments)
            {
                double d = DistancePointToSegment(p, a, b);
                if (d <= bestDistance)
                {
                    bestDistance = d;
                    best = id;
                }
            }

            return best;
        }

        public void SetSelectedElement(int? elementId)
        {
            _selectedElementId = elementId;
            Draw();
        }

        public void ZoomToFit()
        {
            if (_canvas.ActualWidth <= 0 || _canvas.ActualHeight <= 0)
                return;

            _viewport.FitToBounds(_scene.Bounds, _canvas.ActualWidth, _canvas.ActualHeight);
            _hasFitted = true;
            Draw();
        }

        // --- Drawing ---

        private void Draw()
        {
            _canvas.Children.Clear();
            _memberSegments.Clear();

            DrawSupports();
            DrawDistributedLoads();
            DrawMembers(); // fills _memberSegments
            DrawDeflected();
            DrawNodes();
            DrawNodalLoads();
            DrawPointLoads();
            DrawReactions();
        }

        private Point S(double worldX, double worldY)
        {
            (double x, double y) = _viewport.WorldToScreen(worldX, worldY);
            return new Point(x, y);
        }

        private void DrawMembers()
        {
            foreach (SceneMember m in _scene.Members)
            {
                Point a = S(m.StartX, m.StartY);
                Point b = S(m.EndX, m.EndY);
                bool selected = _selectedElementId.HasValue && m.Id == _selectedElementId.Value;

                var line = new Line
                {
                    X1 = a.X, Y1 = a.Y, X2 = b.X, Y2 = b.Y,
                    Stroke = selected ? SelectedBrush : MemberBrush,
                    StrokeThickness = selected ? 3.0 : 1.8
                };
                if (m.IsTruss)
                    line.StrokeDashArray = new DoubleCollection { 5, 3 };

                _canvas.Children.Add(line);
                _memberSegments.Add((m.Id, a, b));
            }
        }

        private void DrawNodes()
        {
            foreach (SceneNode n in _scene.Nodes)
            {
                Point p = S(n.X, n.Y);
                var dot = new Ellipse { Width = 7, Height = 7, Fill = NodeBrush };
                Canvas.SetLeft(dot, p.X - 3.5);
                Canvas.SetTop(dot, p.Y - 3.5);
                _canvas.Children.Add(dot);
            }
        }

        private void DrawSupports()
        {
            foreach (SceneSupport s in _scene.Supports)
            {
                Point p = S(s.X, s.Y);

                if (s.RestrainX || s.RestrainY)
                {
                    var triangle = new Polygon
                    {
                        Stroke = SupportBrush,
                        StrokeThickness = 1.0,
                        Fill = Brushes.Gainsboro,
                        Points = new PointCollection
                        {
                            new Point(p.X, p.Y),
                            new Point(p.X - 9, p.Y + 15),
                            new Point(p.X + 9, p.Y + 15)
                        }
                    };
                    _canvas.Children.Add(triangle);
                }

                if (s.RestrainRz)
                {
                    var square = new Rectangle
                    {
                        Width = 13, Height = 13,
                        Stroke = SupportBrush, StrokeThickness = 1.3,
                        Fill = Brushes.Transparent
                    };
                    Canvas.SetLeft(square, p.X - 6.5);
                    Canvas.SetTop(square, p.Y - 6.5);
                    _canvas.Children.Add(square);
                }
            }
        }

        private void DrawNodalLoads()
        {
            foreach (SceneNodalLoad l in _scene.NodalLoads)
            {
                Point head = S(l.X, l.Y);
                (double ux, double uy) = Normalize(l.Fx, -l.Fy); // world->screen flips Y
                if (ux != 0 || uy != 0)
                    DrawArrow(new Point(head.X - ux * 48, head.Y - uy * 48), head, LoadBrush);

                if (l.Mz != 0)
                    DrawMomentMarker(head, LoadBrush);
            }
        }

        private void DrawPointLoads()
        {
            foreach (ScenePointLoad p in _scene.PointLoads)
            {
                Point head = S(p.X, p.Y);
                double sign = p.Magnitude < 0 ? -1.0 : 1.0;
                (double ux, double uy) = Normalize(p.DirX * sign, -(p.DirY * sign));
                if (ux != 0 || uy != 0)
                    DrawArrow(new Point(head.X - ux * 40, head.Y - uy * 40), head, LoadBrush);
            }
        }

        private void DrawDistributedLoads()
        {
            foreach (SceneDistributedLoad d in _scene.DistributedLoads)
            {
                double sign = d.Magnitude < 0 ? -1.0 : 1.0;
                (double ux, double uy) = Normalize(d.DirX * sign, -(d.DirY * sign));
                if (ux == 0 && uy == 0)
                    continue;

                Point a = S(d.StartX, d.StartY);
                Point b = S(d.EndX, d.EndY);

                const int count = 6;
                const double length = 26.0;
                var tails = new PointCollection();

                for (int i = 0; i < count; i++)
                {
                    double t = count == 1 ? 0.0 : (double)i / (count - 1);
                    var onMember = new Point(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
                    var tail = new Point(onMember.X - ux * length, onMember.Y - uy * length);
                    DrawArrow(tail, onMember, DistLoadBrush, 1.1);
                    tails.Add(tail);
                }

                _canvas.Children.Add(new Polyline { Stroke = DistLoadBrush, StrokeThickness = 1.0, Points = tails });
            }
        }

        private void DrawDeflected()
        {
            foreach (SceneDeflectedMember dm in _scene.DeflectedMembers)
            {
                var poly = new Polyline
                {
                    Stroke = DeflectedBrush,
                    StrokeThickness = 1.6,
                    StrokeDashArray = new DoubleCollection { 3, 2 }
                };
                foreach (ScenePoint pt in dm.Points)
                    poly.Points.Add(S(pt.X, pt.Y));
                _canvas.Children.Add(poly);
            }
        }

        private void DrawReactions()
        {
            foreach (SceneReaction r in _scene.Reactions)
            {
                Point at = S(r.X, r.Y);
                (double ux, double uy) = Normalize(r.Fx, -r.Fy);
                if (ux != 0 || uy != 0)
                    DrawArrow(new Point(at.X - ux * 40, at.Y - uy * 40), at, ReactionBrush);
            }
        }

        private void DrawArrow(Point from, Point to, Brush brush, double thickness = 1.6)
        {
            _canvas.Children.Add(new Line { X1 = from.X, Y1 = from.Y, X2 = to.X, Y2 = to.Y, Stroke = brush, StrokeThickness = thickness });

            double angle = Math.Atan2(to.Y - from.Y, to.X - from.X);
            const double size = 9.0, spread = 0.42;
            var h1 = new Point(to.X - size * Math.Cos(angle - spread), to.Y - size * Math.Sin(angle - spread));
            var h2 = new Point(to.X - size * Math.Cos(angle + spread), to.Y - size * Math.Sin(angle + spread));
            _canvas.Children.Add(new Line { X1 = to.X, Y1 = to.Y, X2 = h1.X, Y2 = h1.Y, Stroke = brush, StrokeThickness = thickness });
            _canvas.Children.Add(new Line { X1 = to.X, Y1 = to.Y, X2 = h2.X, Y2 = h2.Y, Stroke = brush, StrokeThickness = thickness });
        }

        private void DrawMomentMarker(Point center, Brush brush)
        {
            var ring = new Ellipse { Width = 16, Height = 16, Stroke = brush, StrokeThickness = 1.4 };
            Canvas.SetLeft(ring, center.X - 8);
            Canvas.SetTop(ring, center.Y - 8);
            _canvas.Children.Add(ring);
        }

        // --- Interaction ---

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double factor = e.Delta > 0 ? 1.1 : 1.0 / 1.1;
            Point p = e.GetPosition(_canvas);
            _viewport.ZoomAt(factor, p.X, p.Y);
            Draw();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(_canvas);
            ElementPicked?.Invoke(this, HitTestElement(p.X, p.Y));
        }

        private void OnPanStart(object sender, MouseButtonEventArgs e)
        {
            _isPanning = true;
            _lastPanPoint = e.GetPosition(_canvas);
            _canvas.CaptureMouse();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isPanning)
                return;

            Point p = e.GetPosition(_canvas);
            _viewport.PanBy(p.X - _lastPanPoint.X, p.Y - _lastPanPoint.Y);
            _lastPanPoint = p;
            Draw();
        }

        private void OnPanEnd(object sender, MouseButtonEventArgs e)
        {
            _isPanning = false;
            _canvas.ReleaseMouseCapture();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_hasFitted)
                ZoomToFit();
            else
                Draw();
        }

        // --- Math ---

        private static (double X, double Y) Normalize(double x, double y)
        {
            double length = Math.Sqrt(x * x + y * y);
            return length < 1e-9 ? (0.0, 0.0) : (x / length, y / length);
        }

        private static double DistancePointToSegment(Point p, Point a, Point b)
        {
            double dx = b.X - a.X, dy = b.Y - a.Y;
            double lengthSq = dx * dx + dy * dy;
            if (lengthSq < 1e-9)
                return (p - a).Length;

            double t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lengthSq;
            t = Math.Clamp(t, 0.0, 1.0);
            double cx = a.X + t * dx, cy = a.Y + t * dy;
            double ex = p.X - cx, ey = p.Y - cy;
            return Math.Sqrt(ex * ex + ey * ey);
        }
    }
}

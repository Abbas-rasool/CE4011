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
        private static readonly Brush DimensionBrush = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44));

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

                // Member length annotation (world length; constant under zoom). Skip when the
                // on-screen span is too short to label cleanly.
                double worldLen = Math.Sqrt(
                    (m.EndX - m.StartX) * (m.EndX - m.StartX) +
                    (m.EndY - m.StartY) * (m.EndY - m.StartY));
                if ((b - a).Length > 28.0 && worldLen > 0)
                    DrawDimensionLabel(a, b, FormatLength(worldLen) + " m");
            }
        }

        private static string FormatLength(double length) => length.ToString("0.###");

        /// <summary>Draws a length label at the member midpoint, aligned to the member and offset to one side.</summary>
        private void DrawDimensionLabel(Point a, Point b, string text)
        {
            var label = new TextBlock { Text = text, Foreground = DimensionBrush, FontSize = 11.0 };
            label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size sz = label.DesiredSize;

            double rad = Math.Atan2(b.Y - a.Y, b.X - a.X);
            double deg = rad * 180.0 / Math.PI;
            if (deg > 90.0) deg -= 180.0;        // keep the text upright
            else if (deg < -90.0) deg += 180.0;

            // Offset perpendicular to the member, biased toward the upper side of the screen.
            double nx = -Math.Sin(rad), ny = Math.Cos(rad);
            if (ny > 0) { nx = -nx; ny = -ny; }
            const double offset = 10.0;
            var mid = new Point((a.X + b.X) / 2.0, (a.Y + b.Y) / 2.0);
            var pos = new Point(mid.X + nx * offset, mid.Y + ny * offset);

            label.RenderTransformOrigin = new Point(0.5, 0.5);
            label.RenderTransform = new RotateTransform(deg);
            Canvas.SetLeft(label, pos.X - sz.Width / 2.0);
            Canvas.SetTop(label, pos.Y - sz.Height / 2.0);
            _canvas.Children.Add(label);
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

        // Standard civil-engineering support glyphs, derived from the restraint flags:
        //   fixed  (rotation restrained) -> hatched ground face
        //   pinned (both translations)   -> hollow triangle, apex at the node
        //   roller (one translation)     -> triangle on a row of small circles
        private const double SupHalfBase = 11.0; // half-width of the triangle base
        private const double SupHeight = 16.0;   // node -> base distance

        private void DrawSupports()
        {
            foreach (SceneSupport s in _scene.Supports)
            {
                Point p = S(s.X, s.Y);

                if (s.RestrainRz)
                {
                    // Fixed (encastré): hatched face through the node, ground below.
                    DrawFixedSupport(p);
                }
                else if (s.RestrainX && s.RestrainY)
                {
                    // Pinned: triangle pointing up into the node.
                    DrawTriangle(p, 0, 1);
                }
                else if (s.RestrainX ^ s.RestrainY)
                {
                    // Roller: rollers lie on the restrained direction's ground face.
                    // Vertical roller (restrains Y) -> ground below; horizontal
                    // roller (restrains X) -> ground to the left.
                    if (s.RestrainY)
                        DrawRoller(p, 0, 1);
                    else
                        DrawRoller(p, -1, 0);
                }
            }
        }

        /// <summary>Hollow triangle with its apex at <paramref name="apex"/>, opening along (downX, downY).</summary>
        private void DrawTriangle(Point apex, double downX, double downY)
        {
            double tx = -downY, ty = downX; // base direction (perpendicular to "down")
            var baseCenter = new Point(apex.X + downX * SupHeight, apex.Y + downY * SupHeight);
            var left = new Point(baseCenter.X - tx * SupHalfBase, baseCenter.Y - ty * SupHalfBase);
            var right = new Point(baseCenter.X + tx * SupHalfBase, baseCenter.Y + ty * SupHalfBase);

            _canvas.Children.Add(new Polygon
            {
                Stroke = SupportBrush,
                StrokeThickness = 1.6,
                Fill = Brushes.White,
                Points = new PointCollection { apex, left, right }
            });
        }

        private void DrawRoller(Point apex, double downX, double downY)
        {
            DrawTriangle(apex, downX, downY);

            double tx = -downY, ty = downX;
            const double r = 2.8;
            double along = SupHeight + r + 0.5; // sit the rollers just past the base line
            var center = new Point(apex.X + downX * along, apex.Y + downY * along);
            double spread = SupHalfBase * 0.62;

            for (int i = -1; i <= 1; i++)
            {
                var c = new Point(center.X + tx * spread * i, center.Y + ty * spread * i);
                var dot = new Ellipse
                {
                    Width = 2 * r, Height = 2 * r,
                    Stroke = SupportBrush, StrokeThickness = 1.3,
                    Fill = Brushes.White
                };
                Canvas.SetLeft(dot, c.X - r);
                Canvas.SetTop(dot, c.Y - r);
                _canvas.Children.Add(dot);
            }
        }

        private void DrawFixedSupport(Point p)
        {
            // Horizontal face through the node with 45-degree hatching below it.
            const double half = SupHalfBase + 2.0;
            var a = new Point(p.X - half, p.Y);
            var b = new Point(p.X + half, p.Y);
            _canvas.Children.Add(new Line { X1 = a.X, Y1 = a.Y, X2 = b.X, Y2 = b.Y, Stroke = SupportBrush, StrokeThickness = 1.8 });

            const double hatch = 7.0;
            const int count = 6;
            for (int i = 0; i <= count; i++)
            {
                double f = (double)i / count;
                var start = new Point(a.X + (b.X - a.X) * f, p.Y);
                var end = new Point(start.X - hatch, start.Y + hatch);
                _canvas.Children.Add(new Line { X1 = start.X, Y1 = start.Y, X2 = end.X, Y2 = end.Y, Stroke = SupportBrush, StrokeThickness = 1.0 });
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
            // Reference magnitudes so we can hide components that are essentially zero
            // (e.g. a roller's free direction, or tiny numerical residue).
            double refForce = 0, refMoment = 0;
            foreach (SceneReaction r in _scene.Reactions)
            {
                refForce = Math.Max(refForce, Math.Max(Math.Abs(r.Fx), Math.Abs(r.Fy)));
                refMoment = Math.Max(refMoment, Math.Abs(r.Mz));
            }

            const double len = 38.0;
            foreach (SceneReaction r in _scene.Reactions)
            {
                Point at = S(r.X, r.Y);

                // Horizontal component (Fx): world +x -> screen +x.
                if (refForce > 0 && Math.Abs(r.Fx) > 1e-3 * refForce)
                {
                    double dir = r.Fx > 0 ? 1 : -1;
                    DrawArrow(at, new Point(at.X + dir * len, at.Y), ReactionBrush);
                }

                // Vertical component (Fy): world +y is up -> screen -y.
                if (refForce > 0 && Math.Abs(r.Fy) > 1e-3 * refForce)
                {
                    double dir = r.Fy > 0 ? -1 : 1;
                    DrawArrow(at, new Point(at.X, at.Y + dir * len), ReactionBrush);
                }

                // Moment (Mz): curved arrow. +Mz is counter-clockwise (and, with the
                // Y-flip to screen, also reads counter-clockwise on the canvas).
                if (refMoment > 0 && Math.Abs(r.Mz) > 1e-3 * refMoment)
                    DrawMomentArc(at, 14.0, ccw: r.Mz > 0, ReactionBrush);
            }
        }

        /// <summary>A ~270-degree curved arrow centered on <paramref name="center"/> showing rotation sense.</summary>
        private void DrawMomentArc(Point center, double radius, bool ccw, Brush brush)
        {
            const int seg = 24;
            const double sweepDeg = 270.0;
            double a0 = -45.0 * Math.PI / 180.0;
            double sweep = sweepDeg * Math.PI / 180.0 * (ccw ? -1.0 : 1.0); // +theta is screen-clockwise

            var pts = new PointCollection();
            for (int i = 0; i <= seg; i++)
            {
                double a = a0 + sweep * i / seg;
                pts.Add(new Point(center.X + radius * Math.Cos(a), center.Y + radius * Math.Sin(a)));
            }
            _canvas.Children.Add(new Polyline { Stroke = brush, StrokeThickness = 1.6, Points = pts });

            // Arrowhead on the open end, aligned with the last arc segment.
            Point tip = pts[pts.Count - 1];
            Point prev = pts[pts.Count - 2];
            double ang = Math.Atan2(tip.Y - prev.Y, tip.X - prev.X);
            const double size = 8.0, spread = 0.5;
            var h1 = new Point(tip.X - size * Math.Cos(ang - spread), tip.Y - size * Math.Sin(ang - spread));
            var h2 = new Point(tip.X - size * Math.Cos(ang + spread), tip.Y - size * Math.Sin(ang + spread));
            _canvas.Children.Add(new Line { X1 = tip.X, Y1 = tip.Y, X2 = h1.X, Y2 = h1.Y, Stroke = brush, StrokeThickness = 1.6 });
            _canvas.Children.Add(new Line { X1 = tip.X, Y1 = tip.Y, X2 = h2.X, Y2 = h2.Y, Stroke = brush, StrokeThickness = 1.6 });
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

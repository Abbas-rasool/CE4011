using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using FrameAnalysisProgram.ANALYSIS_CORE;

namespace FrameAnalysis.UI
{
    /// <summary>
    /// Per-member results popup: the deflected shape on top, then the normal / shear /
    /// moment diagrams stacked under it. Reads a <see cref="MemberStationResult"/> (the
    /// station-sampled section forces + deflected shape) and draws each as a small plot.
    /// Pure presentation — all the numbers come pre-computed from the analysis.
    /// </summary>
    public partial class MemberResultWindow : Window
    {
        private static readonly Color DeflColor = Color.FromRgb(0x53, 0x4A, 0xB7);  // purple
        private static readonly Color AxialColor = Color.FromRgb(0x37, 0x8A, 0xDD); // blue
        private static readonly Color ShearColor = Color.FromRgb(0x1D, 0x9E, 0x75); // teal
        private static readonly Color MomentColor = Color.FromRgb(0xD8, 0x5A, 0x30);// coral
        private static readonly Brush BaselineBrush = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));

        private readonly MemberStationResult _stations;

        public MemberResultWindow(MemberStationResult stations, string label)
        {
            InitializeComponent();

            _stations = stations ?? throw new ArgumentNullException(nameof(stations));
            HeaderText.Text = $"{label} — results";
            SubHeaderText.Text = $"L = {_stations.Length:0.###} m · {_stations.X.Count} stations";

            DeflCaption.Text = $"Deflected shape · max {MaxAbs(_stations.DeflectionTransverse) * 1000.0:0.##} mm";
            AxialCaption.Text = $"Normal force N · max {_stations.MaxAbsAxial:0.##} kN";
            ShearCaption.Text = $"Shear V · max {_stations.MaxAbsShear:0.##} kN";
            MomentCaption.Text = $"Moment M · max {_stations.MaxAbsMoment:0.##} kN·m";

            // Canvases have no size until laid out; draw on load and keep it responsive.
            Loaded += (_, _) => DrawAll();
            SizeChanged += (_, _) => DrawAll();
        }

        private void DrawAll()
        {
            DrawDiagram(DeflCanvas, _stations.DeflectionTransverse, DeflColor);
            DrawDiagram(AxialCanvas, _stations.Axial, AxialColor);
            DrawDiagram(ShearCanvas, _stations.Shear, ShearColor);
            DrawDiagram(MomentCanvas, _stations.Moment, MomentColor);
        }

        /// <summary>
        /// Draws one diagram: a horizontal member-axis baseline through the canvas centre,
        /// the value polyline, and a translucent fill back to the axis. The vertical scale
        /// fits the largest magnitude so the shape is always visible.
        /// </summary>
        private void DrawDiagram(Canvas canvas, IReadOnlyList<double> values, Color color)
        {
            canvas.Children.Clear();

            double w = canvas.ActualWidth, h = canvas.ActualHeight;
            double L = _stations.Length;
            IReadOnlyList<double> xs = _stations.X;
            if (w <= 0 || h <= 0 || L <= 0 || xs.Count == 0)
                return;

            const double padX = 14.0, padY = 12.0;
            double left = padX, right = w - padX;
            double centerY = h / 2.0;
            double yHalf = h / 2.0 - padY;

            double amax = MaxAbs(values);
            if (amax < 1e-12) amax = 1.0;

            double X(double xi) => left + (right - left) * (xi / L);
            double Y(double v) => centerY - v / amax * yHalf;

            canvas.Children.Add(new Line
            {
                X1 = left, Y1 = centerY, X2 = right, Y2 = centerY,
                Stroke = BaselineBrush, StrokeThickness = 1.0
            });

            var fill = new PointCollection { new Point(X(xs[0]), centerY) };
            var line = new PointCollection();
            for (int k = 0; k < xs.Count; k++)
            {
                var p = new Point(X(xs[k]), Y(values[k]));
                fill.Add(p);
                line.Add(p);
            }
            fill.Add(new Point(X(xs[xs.Count - 1]), centerY));

            canvas.Children.Add(new Polygon
            {
                Points = fill,
                Fill = new SolidColorBrush(Color.FromArgb(40, color.R, color.G, color.B))
            });
            canvas.Children.Add(new Polyline
            {
                Points = line,
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 1.8
            });
        }

        private static double MaxAbs(IReadOnlyList<double> values)
        {
            double m = 0.0;
            foreach (double v in values)
            {
                double a = Math.Abs(v);
                if (a > m) m = a;
            }
            return m;
        }
    }
}

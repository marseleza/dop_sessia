using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FunctionPlotter
{
    public partial class MainWindow : Window
    {
        private MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();
            _vm = new MainViewModel();
            _vm.PropertyChanged += Vm_PropertyChanged;
            DataContext = _vm;
        }

        private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.PlotData))
                RedrawPlot();
        }

        private void PlotCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
            => RedrawPlot();

        private void RedrawPlot()
        {
            PlotCanvas.Children.Clear();

            var data = _vm.PlotData;
            if (data == null || data.Points.Count == 0) return;

            double w = PlotCanvas.ActualWidth;
            double h = PlotCanvas.ActualHeight;
            if (w <= 1 || h <= 1) return;

            // Оси
            var axisBrush = Brushes.LightGray;
            PlotCanvas.Children.Add(new Line { X1 = 0, Y1 = h, X2 = w, Y2 = h, Stroke = axisBrush });
            PlotCanvas.Children.Add(new Line { X1 = 0, Y1 = 0, X2 = 0, Y2 = h, Stroke = axisBrush });

            // Кривая
            var poly = new Polyline
            {
                Stroke = Brushes.SteelBlue,
                StrokeThickness = 2,
                StrokeLineJoin = PenLineJoin.Round
            };

            foreach (var p in data.Points)
            {
                double px = p.X * w;
                double py = h - p.Y * h;
                poly.Points.Add(new Point(px, py));
            }
            PlotCanvas.Children.Add(poly);

            // Подписи Y
            var labelBrush = Brushes.DimGray;
            var t1 = new TextBlock { Text = data.YMax.ToString("0.###"), Foreground = labelBrush };
            Canvas.SetLeft(t1, -35);
            Canvas.SetTop(t1, -5);
            PlotCanvas.Children.Add(t1);

            var t2 = new TextBlock { Text = data.YMin.ToString("0.###"), Foreground = labelBrush };
            Canvas.SetLeft(t2, -35);
            Canvas.SetTop(t2, h - 10);
            PlotCanvas.Children.Add(t2);
        }
    }
}
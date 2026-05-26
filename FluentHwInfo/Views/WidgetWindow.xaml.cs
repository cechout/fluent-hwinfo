using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using LiveChartsCore.Kernel.Sketches;

namespace FluentHwInfo.Views
{
    public sealed partial class WidgetWindow : Window
    {
        private AppWindow _appWindow;


        // the raw data list that will be plotted by LiveCharts
        public ObservableCollection<double?> CpuPowerData { get; set; }
        private const int MaxDataPoints = 50;

        // LiveCharts Fields
        public ISeries[] Series { get; set; }
        public ICartesianAxis[] XAxes { get; set; } = { new Axis { IsVisible = false } };
        public ICartesianAxis[] YAxes { get; set; } = { new Axis { IsVisible = false } };
        public LiveChartsCore.Measure.Margin ChartMargin { get; set; } = new LiveChartsCore.Measure.Margin(0);

        public WidgetWindow()
        {
            CpuPowerData = new ObservableCollection<double?>(new double?[MaxDataPoints]);

            // test
            CpuPowerData[34] = 29.2;
            CpuPowerData[35] = 35.2;
            CpuPowerData[36] = 29.2;
            CpuPowerData[37] = 27.2;
            CpuPowerData[38] = 25.2;
            CpuPowerData[39] = 45.2;
            CpuPowerData[40] = 50.1;
            CpuPowerData[41] = 62.5;
            CpuPowerData[42] = 48.0;
            CpuPowerData[43] = 55.3;
            CpuPowerData[44] = 55.3;
            CpuPowerData[45] = 25.3;
            CpuPowerData[46] = 25.3;
            CpuPowerData[47] = 35.3;
            CpuPowerData[48] = 45.3;
            CpuPowerData[49] = 35.3;

            // custom gradient fill
            var gradientFill = new LinearGradientPaint(
                new[] { SKColors.DodgerBlue.WithAlpha(100), SKColors.DodgerBlue.WithAlpha(0) },
                new SKPoint(0.5f, 0), // start: top middle
                new SKPoint(0.5f, 1)  // end: bottom middle
            );

            // the LiveCharts ISeries definition
            Series = new ISeries[]
            {
                new LineSeries<double?>
                {
                    Values = CpuPowerData,
                    Fill = gradientFill, 
                    GeometrySize = 1, // 0: no graph points, >=1: size of graph points
                    LineSmoothness = 0.8, 
                    Stroke = new SolidColorPaint(SKColors.DodgerBlue) { StrokeThickness = 2 },
                    DataPadding = new LiveChartsCore.Drawing.LvcPoint(0, 0) // graph padding
                }
            };

            this.InitializeComponent();

            // custom window settings
            _appWindow = this.AppWindow;
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar); 
            _appWindow.SetPresenter(AppWindowPresenterKind.CompactOverlay);
            PositionWidgetTopRight(); // custom method to position window at top-right of the screen
        }

        private void PositionWidgetTopRight()
        {
            // get the size of the primary screen
            var displayArea = DisplayArea.Primary;
            int screenWidth = displayArea.WorkArea.Width;

            int widgetWidth = 1000;
            int widgetHeight = 600;

            // move the window to the right edge (with 10px margin)
            _appWindow.MoveAndResize(new Windows.Graphics.RectInt32(
                screenWidth - widgetWidth - 10,
                10,
                widgetWidth,
                widgetHeight));
        }

        private void BackToDashboard_Click(object sender, RoutedEventArgs e)
        {
            // Here you would call/show your MainWindow again
            // App.MainWindow.Activate();
            this.Close();
        }
    }
}
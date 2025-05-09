using System;
using System.ComponentModel;
using System.Windows.Media;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using FuturesEquityCurve.Models;
using FuturesEquityCurve.Services;

namespace FuturesEquityCurve.ViewModels
{
    public class EquityCurveViewModel : INotifyPropertyChanged
    {
        private PlotModel _plotModel;
        private LineSeries _equitySeries;
        private ScatterSeries _openTradeSeries;
        private ScatterSeries _closeTradeSeries;
        private MockCtpService _ctpService;

        public PlotModel PlotModel
        {
            get { return _plotModel; }
            set
            {
                _plotModel = value;
                OnPropertyChanged("PlotModel");
            }
        }

        public EquityCurveViewModel()
        {
            InitializePlotModel();
            _ctpService = new MockCtpService();
            _ctpService.OnEquityUpdated += OnEquityUpdated;
            _ctpService.OnTradeExecuted += OnTradeExecuted;
        }

        private void InitializePlotModel()
        {
            PlotModel = new PlotModel { Title = "期货实时资金曲线" };

            // 添加时间轴
            PlotModel.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "HH:mm:ss",
                Title = "时间",
                MajorGridlineStyle = LineStyle.Dot,
                MinorGridlineStyle = LineStyle.None
            });

            // 添加资金轴
            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "资金(元)",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            });

            // 资金曲线
            _equitySeries = new LineSeries
            {
                Title = "账户资金",
                Color = OxyColors.Green,
                StrokeThickness = 2,
                MarkerType = MarkerType.None
            };

            // 开仓点
            _openTradeSeries = new ScatterSeries
            {
                Title = "开仓点",
                MarkerType = MarkerType.Diamond,
                MarkerSize = 6,
                MarkerFill = OxyColors.Blue
            };

            // 平仓点
            _closeTradeSeries = new ScatterSeries
            {
                Title = "平仓点",
                MarkerType = MarkerType.Circle,
                MarkerSize = 6,
                MarkerFill = OxyColors.Red
            };

            PlotModel.Series.Add(_equitySeries);
            PlotModel.Series.Add(_openTradeSeries);
            PlotModel.Series.Add(_closeTradeSeries);
        }

        // 处理资金更新事件
        private void OnEquityUpdated(object sender, EquityPoint e)
        {
            // 添加资金点
            _equitySeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(e.Time), e.Equity));

            // 自动调整坐标轴
            AutoAdjustAxes();

            // 刷新图表
            PlotModel.InvalidatePlot(true);
        }

        // 处理交易事件
        private void OnTradeExecuted(object sender, TradePoint e)
        {
            // 添加交易点
            if (e.IsClose)
            {
                _closeTradeSeries.Points.Add(new ScatterPoint(
                    DateTimeAxis.ToDouble(e.Time),
                    e.Equity,
                    6,
                    0
                ));

                // 添加盈亏标记
                var annotation = new OxyPlot.Annotations.TextAnnotation
                {
                    Text = e.PnL.ToString("F0"),
                    TextPosition = new DataPoint(DateTimeAxis.ToDouble(e.Time), e.Equity),
                    TextColor = e.PnL >= 0 ? OxyColors.Green : OxyColors.Red,
                    FontWeight = 500
                };
                PlotModel.Annotations.Add(annotation);
            }
            else
            {
                _openTradeSeries.Points.Add(new ScatterPoint(
                    DateTimeAxis.ToDouble(e.Time),
                    e.Equity,
                    6,
                    0
                ));
            }

            // 自动调整坐标轴
            AutoAdjustAxes();

            // 刷新图表
            PlotModel.InvalidatePlot(true);
        }

        // 自动调整坐标轴范围
        private void AutoAdjustAxes()
        {
            if (_equitySeries.Points.Count <= 1)
                return;

            var dateAxis = PlotModel.Axes[0] as DateTimeAxis;

            // 显示最新的100个点
            int visiblePoints = 100;
            if (_equitySeries.Points.Count > visiblePoints)
            {
                double minX = _equitySeries.Points[_equitySeries.Points.Count - visiblePoints].X;
                dateAxis.Minimum = minX;
                dateAxis.Maximum = DateTimeAxis.ToDouble(DateTime.Now.AddMinutes(1));
            }

            // 自动调整Y轴
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            // 只考虑可见区域的点
            int startIdx = Math.Max(0, _equitySeries.Points.Count - visiblePoints);
            for (int i = startIdx; i < _equitySeries.Points.Count; i++)
            {
                minY = Math.Min(minY, _equitySeries.Points[i].Y);
                maxY = Math.Max(maxY, _equitySeries.Points[i].Y);
            }

            // 添加边距
            double margin = (maxY - minY) * 0.1;
            if (margin < 10) margin = 10; // 最小边距

            var valueAxis = PlotModel.Axes[1] as LinearAxis;
            valueAxis.Minimum = minY - margin;
            valueAxis.Maximum = maxY + margin;
        }

        // 启动模拟服务
        public void StartSimulation()
        {
            _ctpService.Start();
        }

        // 停止模拟服务
        public void StopSimulation()
        {
            _ctpService.Stop();
        }

        // INotifyPropertyChanged实现
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
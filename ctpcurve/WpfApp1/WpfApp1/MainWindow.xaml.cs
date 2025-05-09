using System;
using System.Windows;
using FuturesEquityCurve.ViewModels;

namespace FuturesEquityCurve
{
    public partial class MainWindow : Window
    {
        private EquityCurveViewModel ViewModel => Resources["ViewModel"] as EquityCurveViewModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 自动开始模拟
            ViewModel?.StartSimulation();
            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 关闭窗口时停止模拟
            ViewModel?.StopSimulation();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.StartSimulation();
            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.StopSimulation();
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
        }
    }
}
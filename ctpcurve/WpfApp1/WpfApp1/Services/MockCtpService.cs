using System;
using System.Collections.Generic;
using System.Timers;
using FuturesEquityCurve.Models;

namespace FuturesEquityCurve.Services
{
    public class MockCtpService
    {
        private Timer _marketDataTimer;
        private Timer _tradeTimer;
        private double _lastPrice = 3500.0;
        private Random _random = new Random();
        private double _equity = 100000.0;  // 初始资金
        private double _floatingPnL = 0.0;
        private int _position = 0;          // 持仓量(正为多，负为空)
        private double _avgOpenPrice = 0.0;  // 开仓均价
        private List<string> _symbols = new List<string> { "IF2401", "IC2401", "IH2401" };
        private string _currentSymbol;

        // 事件定义
        public event EventHandler<EquityPoint> OnEquityUpdated;
        public event EventHandler<TradePoint> OnTradeExecuted;

        public MockCtpService()
        {
            _currentSymbol = _symbols[0];

            // 设置行情更新定时器(每秒)
            _marketDataTimer = new Timer(1000);
            _marketDataTimer.Elapsed += OnMarketDataTimerElapsed;

            // 设置随机交易定时器(约每10秒)
            _tradeTimer = new Timer(10000);
            _tradeTimer.Elapsed += OnTradeTimerElapsed;
        }

        public void Start()
        {
            _marketDataTimer.Start();
            _tradeTimer.Start();
        }

        public void Stop()
        {
            _marketDataTimer.Stop();
            _tradeTimer.Stop();
        }

        // 模拟行情更新
        private void OnMarketDataTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // 随机生成价格波动(-5到5之间)
            double priceChange = (_random.NextDouble() - 0.5) * 10;
            _lastPrice += priceChange;

            // 计算浮动盈亏
            if (_position != 0)
            {
                // 合约乘数假设为10
                int multiplier = 10;
                _floatingPnL = (_lastPrice - _avgOpenPrice) * _position * multiplier;
            }
            else
            {
                _floatingPnL = 0;
            }

            // 计算当前权益
            double currentEquity = _equity + _floatingPnL;

            // 触发事件
            OnEquityUpdated?.Invoke(this, new EquityPoint(DateTime.Now, currentEquity, _floatingPnL));
        }

        // 模拟随机交易
        private void OnTradeTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // 如果已有持仓，有70%概率平仓
            bool isClose = _position != 0 && _random.NextDouble() < 0.7;

            string direction;
            int volume = _random.Next(1, 5);  // 随机1-4手
            double pnl = 0;

            if (isClose)
            {
                // 平仓方向与持仓相反
                direction = _position > 0 ? "Sell" : "Buy";

                // 平仓数量不能超过持仓量
                volume = Math.Min(volume, Math.Abs(_position));

                // 计算平仓盈亏
                int multiplier = 10;
                pnl = (_lastPrice - _avgOpenPrice) * (direction == "Buy" ? -1 : 1) * volume * multiplier;

                // 更新资金
                _equity += pnl;

                // 更新持仓
                if (direction == "Buy")
                    _position += volume;
                else
                    _position -= volume;

                // 如果完全平仓，重置均价
                if (_position == 0)
                    _avgOpenPrice = 0;
            }
            else
            {
                // 开仓，随机方向
                direction = _random.NextDouble() > 0.5 ? "Buy" : "Sell";

                // 更新持仓和均价
                if (_position == 0)
                {
                    _avgOpenPrice = _lastPrice;
                    _position = direction == "Buy" ? volume : -volume;
                }
                else if ((_position > 0 && direction == "Buy") ||
                         (_position < 0 && direction == "Sell"))
                {
                    // 同向加仓，更新均价
                    int oldPos = Math.Abs(_position);
                    double oldValue = _avgOpenPrice * oldPos;
                    double newValue = _lastPrice * volume;
                    _avgOpenPrice = (oldValue + newValue) / (oldPos + volume);

                    // 更新持仓
                    _position = direction == "Buy" ?
                        _position + volume : _position - volume;
                }
                else
                {
                    // 反向开仓(如果超过现有持仓就是反向开仓)
                    if (volume > Math.Abs(_position))
                    {
                        int closeVolume = Math.Abs(_position);
                        int openVolume = volume - closeVolume;

                        // 部分平仓
                        int multiplier = 10;
                        pnl = (_lastPrice - _avgOpenPrice) * (direction == "Buy" ? -1 : 1) * closeVolume * multiplier;
                        _equity += pnl;

                        // 反向开仓
                        _position = direction == "Buy" ? openVolume : -openVolume;
                        _avgOpenPrice = _lastPrice;
                    }
                    else
                    {
                        // 部分平仓
                        int multiplier = 10;
                        pnl = (_lastPrice - _avgOpenPrice) * (direction == "Buy" ? -1 : 1) * volume * multiplier;
                        _equity += pnl;

                        // 更新持仓
                        _position = direction == "Buy" ?
                            _position + volume : _position - volume;
                    }
                }
            }

            // 当前总权益
            double currentEquity = _equity + _floatingPnL;

            // 触发交易事件
            OnTradeExecuted?.Invoke(this, new TradePoint(
                DateTime.Now, currentEquity, _lastPrice, volume,
                direction, isClose, pnl
            ));
        }
    }
}
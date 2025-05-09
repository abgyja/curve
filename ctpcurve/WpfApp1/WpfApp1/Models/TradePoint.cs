using System;

namespace FuturesEquityCurve.Models
{
    public class TradePoint
    {
        public DateTime Time { get; set; }
        public double Equity { get; set; }
        public double Price { get; set; }
        public int Volume { get; set; }
        public string Direction { get; set; }  // "Buy" or "Sell"
        public bool IsClose { get; set; }
        public double PnL { get; set; }        // 平仓盈亏(开仓为0)

        public TradePoint(DateTime time, double equity, double price, int volume,
                         string direction, bool isClose, double pnl)
        {
            Time = time;
            Equity = equity;
            Price = price;
            Volume = volume;
            Direction = direction;
            IsClose = isClose;
            PnL = pnl;
        }
    }
}
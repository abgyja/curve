using System;

namespace FuturesEquityCurve.Models
{
    public class EquityPoint
    {
        public DateTime Time { get; set; }
        public double Equity { get; set; }
        public double FloatingPnL { get; set; }

        public EquityPoint(DateTime time, double equity, double floatingPnL)
        {
            Time = time;
            Equity = equity;
            FloatingPnL = floatingPnL;
        }
    }
}
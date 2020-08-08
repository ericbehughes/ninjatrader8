#region Using declarations
using NinjaTrader.Cbi;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
#endregion


//________________________________________________________________________________________
namespace NinjaTrader.NinjaScript.Indicators
{
    [Description("")]
    public class FModel : Indicator
    {


        #region Variables
        private int vol = 1;
        private int per = 14;
        private double mult = 2;


        double FilterBuySignal, FilterBuySignal_1;
        double FilterSellSignal, FilterSellSignal_1;
        double upward, upward_1;
        double downward, downward_1;
        bool longCond, longCond_1;
        bool shortCond, shortCond_1;


        #endregion

        //________________________________________________________________________________________		
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "FModel";
                DrawOnPricePanel = true; //false;
                AddPlot(new Stroke(Brushes.Transparent, 3), PlotStyle.Line, "signal");

                IsOverlay = true;
                BarsRequiredToPlot = 5;

            }
            else if (State == State.Configure) { }
        }
        //________________________________________________________________________________________
        protected override void OnBarUpdate()
        {
            if (CurrentBar < 4) return;

            FilterBuySignal_1 = FilterBuySignal;
            FilterSellSignal_1 = FilterSellSignal;
            upward_1 = upward;
            downward_1 = downward;
            longCond_1 = longCond;
            shortCond_1 = shortCond;
            signal[0] = 0;

            double AverageRangeBuySignal = ATR(per * 50)[0] * mult;
            double AverageRangeSellSignal = ATR(per * 50)[0] * mult;

            FilterBuySignal = RangeFilter(Close[0], Close[1], AverageRangeBuySignal);
            FilterSellSignal = RangeFilter(Close[0], Close[1], AverageRangeSellSignal);


            upward = FilterBuySignal > FilterBuySignal_1 ? nz(upward_1) + 0.1 : FilterBuySignal < FilterBuySignal_1 ? 0 : nz(upward_1);
            downward = FilterSellSignal < FilterSellSignal_1 ? nz(downward_1) + 0.1 : FilterSellSignal > FilterSellSignal_1 ? 0 : nz(downward_1);


            longCond = upward > 0;
            shortCond = downward > 0;

            bool shortCondition = longCond && !longCond_1;    // abovebar   red
            bool longCondition = shortCond && !shortCond_1;   // belowbar   green

            //Print(Time[0]+"   "+shortCondition+"  "+longCondition);

            if (longCondition) Draw.Dot(this, "Up" + CurrentBar, true, 0, Low[0] - 5 * TickSize, Brushes.LightGreen);
            if (shortCondition) Draw.Dot(this, "Dn" + CurrentBar, false, 0, High[0] + 5 * TickSize, Brushes.Red);

        }
        //___________________________________________________________________________________________________________________________
        double RangeFilter(double x, double x1, double r)
        {
            //someVar = x;
            double somevar = x > nz(x1) ? x - r < nz(x1) ? nz(x1) : x - r : x + r > nz(x1) ? nz(x1) : x + r;
            return (somevar);
        }
        //___________________________________________________________________________________________________________________________
        double nz(double x) { return (x); }
        //___________________________________________________________________________________________________________________________
        #region Properties

        [Browsable(false)] [XmlIgnore()] public Series<double> signal { get { return Values[0]; } }


        [NinjaScriptProperty] [Display(GroupName = "A. ", Order = 1)] public int Period { get { return per; } set { per = value; } }
        [NinjaScriptProperty] [Display(GroupName = "A. ", Order = 3)] public double Multiplier { get { return mult; } set { mult = value; } }


        #endregion
        //___________________________________________________________________________________________________________________________	
    }
}

//_____________________________________________________________________________________________________________________________________________


/*

//@version=4
study(title="model2", overlay=true)
closingPrice = close
var closingPrice2 = close

//Sampling Period
per = input(defval=14, minval=1, title="")
//Range Multiplier
mult = input(defval=2, minval=0.1, title="")

//---------------------------
//Definitions
//---------------------------

//functions in pinescript will return the last expression value
//    wper      = (t * 2) - 1 is another setting for wper
Average_Range(x, t, m)=>
    wper      = (t * 50)
    avrng     = ema(abs(x - x[1]), t)
    Average_Range = ema(avrng, wper)*m
    
// compute buy and sell signals with function    
AverageRangeBuySignal = Average_Range(closingPrice, per, mult)
AverageRangeSellSignal = Average_Range(closingPrice, per, mult)

//Range Filter
RangeFilter(x, r) =>
    someVar = x
    someVar := x > nz(someVar[1]) ? x - r < nz(someVar[1]) ? nz(someVar[1]) : x - r : x + r > nz(someVar[1]) ? nz(someVar[1]) : x + r
    //return 
    someVar
    
FilterBuySignal = RangeFilter(closingPrice, AverageRangeBuySignal)
FilterSellSignal = RangeFilter(closingPrice, AverageRangeSellSignal)

//Filter Direction
upward = 0.0
upward := FilterBuySignal > FilterBuySignal[1] ? nz(upward[1]) + 0.1 : FilterBuySignal < FilterBuySignal[1] ? 0 : nz(upward[1])
downward = 0.0
downward := FilterSellSignal < FilterSellSignal[1] ? nz(downward[1]) + 0.1 : FilterSellSignal > FilterSellSignal[1] ? 0 : nz(downward[1])

longCond = false
shortCond = false
longCond := upward > 0
shortCond := downward > 0

// Getting inputs
fast_length = input(title="Fast Length", type=input.integer, defval=12)
slow_length = input(title="Slow Length", type=input.integer, defval=26)
src = input(title="Source", type=input.source, defval=close)
signal_length = input(title="Signal Smoothing", type=input.integer, minval = 1, maxval = 50, defval = 9)
sma_source = input(title="Simple MA(Oscillator)", type=input.bool, defval=false)
sma_signal = input(title="Simple MA(Signal Line)", type=input.bool, defval=false)


shortCondition = longCond  and not longCond[1] 
longCondition = shortCond  and not shortCond[1] 

//Alerts
plotshape(shortCondition , title="short", text="", location=location.abovebar, color=color.red, textcolor=color.white, style=shape.circle, size=size.tiny, transp=50, offset=2)
plotshape(longCondition, title="short", text="", location=location.belowbar, color=color.green, textcolor=color.white, style=shape.circle, size=size.tiny, transp=50, offset=2)

*/

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private FModel[] cacheFModel;
		public FModel FModel(int period, double multiplier)
		{
			return FModel(Input, period, multiplier);
		}

		public FModel FModel(ISeries<double> input, int period, double multiplier)
		{
			if (cacheFModel != null)
				for (int idx = 0; idx < cacheFModel.Length; idx++)
					if (cacheFModel[idx] != null && cacheFModel[idx].Period == period && cacheFModel[idx].Multiplier == multiplier && cacheFModel[idx].EqualsInput(input))
						return cacheFModel[idx];
			return CacheIndicator<FModel>(new FModel(){ Period = period, Multiplier = multiplier }, input, ref cacheFModel);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.FModel FModel(int period, double multiplier)
		{
			return indicator.FModel(Input, period, multiplier);
		}

		public Indicators.FModel FModel(ISeries<double> input , int period, double multiplier)
		{
			return indicator.FModel(input, period, multiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.FModel FModel(int period, double multiplier)
		{
			return indicator.FModel(Input, period, multiplier);
		}

		public Indicators.FModel FModel(ISeries<double> input , int period, double multiplier)
		{
			return indicator.FModel(input, period, multiplier);
		}
	}
}

#endregion

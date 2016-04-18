/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Linq;
using QuantConnect;
using QuantConnect.Algorithm;
using QuantConnect.Data.Market;
using QuantConnect.Indicators;
using QuantConnect.Orders;

namespace ClassLibrary_QuantConnect.Algorithms
{
    /// <summary>
    /// In this example we look at the canonical 15/30 day moving average cross. This algorithm
    /// will go long when the 15 crosses above the 30 and will liquidate when the 15 crosses
    /// back below the 30.
    /// </summary>
    public class ThreeDucksAlgorithm : QCAlgorithm
    {
        private OrderTicket _orderTicket;

        private DateTime previous;
        private ExponentialMovingAverage fast;
        private ExponentialMovingAverage slow;
        private SimpleMovingAverage[] ribbon;

        private new const string Symbol = "EURUSD";
        private const decimal TenPips = 0.0010m;
        private readonly Symbol _symbol = new Symbol(SecurityIdentifier.GenerateForex(Symbol, Market.FXCM),  Symbol);

        private SimpleMovingAverage _sma60;
        private SimpleMovingAverage _sma720;
        private SimpleMovingAverage _sma2880;
        private bool _isWaitingFor60SmaToShort;
        private bool _isWaitingFor60SmaToLong;

        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        /// </summary>
        public override void Initialize()
        {
            // set up our analysis span
            SetStartDate(2014, 05, 01);
            SetEndDate(2014, 05, 15);

            // request SPY data with minute resolution
            AddSecurity(SecurityType.Forex, Symbol, Resolution.Minute);

            //// create a 15 day exponential moving average
            //fast = EMA(Symbol, 15, Resolution.Daily);

            //// create a 30 day exponential moving average
            //slow = EMA(Symbol, 30, Resolution.Daily);

            _sma60 = SMA(Symbol, 60, Resolution.Minute);
            _sma720 = SMA(Symbol, 720, Resolution.Minute);
            _sma2880 = SMA(Symbol, 2880, Resolution.Minute);

            //int ribbonCount = 8;
            //int ribbonInterval = 15;
            //ribbon = Enumerable.Range(0, ribbonCount).Select(x => SMA(Symbol, (x + 1)*ribbonInterval, Resolution.Daily)).ToArray();
        }

        
        /// <summary>
        /// OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        /// </summary>
        /// <param name="data">TradeBars IDictionary object with your stock data</param>
        public void OnData(TradeBars data)
        {
            // a couple things to notice in this method:
            //  1. We never need to 'update' our indicators with the data, the engine takes care of this for us
            //  2. We can use indicators directly in math expressions
            //  3. We can easily plot many indicators at the same time

            // wait for our slow ema to fully initialize
            //if (!slow.IsReady) return;
            if (!_sma2880.IsReady) return;

            // only once per day
            //if (previous.Date == data.Time.Date) return;

            // define a small tolerance on our checks to avoid bouncing
            //const decimal tolerance = 0.00015m;
            var holdings = Portfolio[Symbol].Quantity;
            var currentPrice = Portfolio[Symbol].Price;

            // we only want to go long if we're currently short or flat
            if (holdings <= 0)
            {
                //// if the fast is greater than the slow, we'll go long
                //if (fast > slow * (1 + tolerance))
                //{
                //    Log("BUY  >> " + Securities[Symbol].Price);
                //    SetHoldings(Symbol, 1.0);
                //}

                // look to go short
                if (currentPrice < _sma2880 && currentPrice < _sma720)
                {
                    if (currentPrice > _sma60)
                    {
                        // wait for current price to drop below 60 SMA
                        _isWaitingFor60SmaToShort = true;
                    }

                    if (_isWaitingFor60SmaToShort)
                    {
                        if (currentPrice <= _sma60)
                        {
                            //open order
                            //SetHoldings(Symbol, 1);
                            //MarketOrder(_symbol, 1);
                            _orderTicket = LimitOrder(_symbol, 1, currentPrice + TenPips);
                        }
                    }
                }

                // look to go long
                if (currentPrice > _sma2880 && currentPrice > _sma720)
                {
                    if (currentPrice < _sma60)
                    {
                        // wait for current price to drop below 60 SMA
                        _isWaitingFor60SmaToLong = true;
                    }

                    if (_isWaitingFor60SmaToLong)
                    {
                        if (currentPrice >= _sma60)
                        {
                            //open order
                            //SetHoldings("EURUSD", 1);
                            _orderTicket = LimitOrder(_symbol, 1, currentPrice - TenPips);

                        }
                        
                    }
                }
            }

            //// we only want to liquidate if we're currently long
            //// if the fast is less than the slow we'll liquidate our long
            //if (holdings > 0 && fast < slow)
            //{
            //    Log("SELL >> " + Securities[Symbol].Price);
            //    Liquidate(Symbol);    
            //}

            //Plot(Symbol, "Price", data[Symbol].Price);
            
            //// easily plot indicators, the series name will be the name of the indicator
            //Plot(Symbol, fast, slow);
            //Plot("Ribbon", ribbon);

            //previous = data.Time;
        }
    }
}
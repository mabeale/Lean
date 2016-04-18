using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using QuantConnect.Configuration;
using QuantConnect.Lean.Engine;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Packets;
using QuantConnect.Util;
using WPF_QuantConnect.Helpers;
using Timer = System.Timers.Timer;

namespace WPF_QuantConnect.ViewModels
{
    public class MainWindowViewModel : NotificationObject
    {
        private StringBuilder _algorithmOutput;
        private readonly Engine _engine;
        //Form Controls:
        private RichTextBox _console;

        //Form Business Logic:
        private Timer _polling;
        private IResultHandler _resultsHandler;
        private bool _isComplete = false;
        private static Thread _leanEngineThread;

        public MainWindowViewModel()
        {
            string algorithm = "ThreeDucksAlgorithm";

            // Setup the configuration, since the UX is not in the 
            // lean directory we write a new config in the UX output directory.
            // TODO > Most of this should be configured through a helper form in the UX.
            Config.Set("algorithm-type-name", algorithm);
            Config.Set("live-mode", "false");
            Config.Set("messaging-handler", "QuantConnect.Messaging.Messaging");
            Config.Set("job-queue-handler", "QuantConnect.Queues.JobQueue");
            Config.Set("api-handler", "QuantConnect.Api.Api");
            Config.Set("result-handler", "QuantConnect.Lean.Engine.Results.DesktopResultHandler");
            Config.Set("environment", "desktop");

            //Start default backtest.
            var engine = LaunchLean();

            _resultsHandler = engine.AlgorithmHandlers.Results;

            //var threeDucksAlgorithm = new ThreeDucksAlgorithm();

            //threeDucksAlgorithm.Initialize();
            
            //Setup Polling Events:
            _polling = new Timer();
            _polling.Interval = 1000;
            _polling.Elapsed += PollingOnTick;
            _polling.Start();

            
        }


        /// <summary>
        /// Launch the LEAN Engine in a separate thread.
        /// </summary>
        private static Engine LaunchLean()
        {
            //Launch the Lean Engine in another thread: this will run the algorithm specified above.
            // TODO > This should only be launched when clicking a backtest/trade live button provided in the UX.

            var systemHandlers = LeanEngineSystemHandlers.FromConfiguration(Composer.Instance);
            var algorithmHandlers = LeanEngineAlgorithmHandlers.FromConfiguration(Composer.Instance);
            var engine = new Engine(systemHandlers, algorithmHandlers, false);
            _leanEngineThread = new Thread(() =>
            {
                string algorithmPath;
                var job = systemHandlers.JobQueue.NextJob(out algorithmPath);
                engine.Run(job, algorithmPath);
                systemHandlers.JobQueue.AcknowledgeJob(job);
            });
            _leanEngineThread.Start();

            return engine;
        }


        public StringBuilder AlgorithmOutput
        {
            get { return _algorithmOutput; }
            set
            {
                _algorithmOutput = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// Primary polling thread for the logging and chart display.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void PollingOnTick(object sender, EventArgs eventArgs)
        {
            Packet message;
            if (_resultsHandler == null) return;
            while (_resultsHandler.Messages.TryDequeue(out message))
            {
                //Process the packet request:
                switch (message.Type)
                {
                    case PacketType.BacktestResult:
                        //Draw chart
                        break;

                    case PacketType.LiveResult:
                        //Draw streaming chart
                        break;

                    case PacketType.AlgorithmStatus:
                        //Algorithm status update
                        break;

                    case PacketType.RuntimeError:
                        var runError = message as RuntimeErrorPacket;
                        if (runError != null) AppendConsole(runError.Message, Colors.Red);
                        break;

                    case PacketType.HandledError:
                        var handledError = message as HandledErrorPacket;
                        if (handledError != null) AppendConsole(handledError.Message, Colors.Red);
                        break;

                    case PacketType.Log:
                        var log = message as LogPacket;
                        if (log != null) AppendConsole(log.Message);
                        break;

                    case PacketType.Debug:
                        var debug = message as DebugPacket;
                        if (debug != null) AppendConsole(debug.Message);
                        break;

                    case PacketType.OrderEvent:
                        //New order event.
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Write to the console in specific font color.
        /// </summary>
        /// <param name="message">String to append</param>
        /// <param name="color">Defaults to black</param>
        private void AppendConsole(string message, Color color = default(Color))
        {
            message = DateTime.Now.ToString("u") + " " + message + Environment.NewLine;
            //Add to console:
            AlgorithmOutput.AppendLine(message);
            //_console.Refresh();
        }
    }
}

using Jacobi.Vst.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Pressor.Calculations;

namespace Pressor.Logic
{
    public enum ECompState
    {
        Bypass,
        Attack, 
        Release,
        Compressing
    }
    internal sealed class Pressor
    {

        #region DEBUG_LISTS_DECLARATION
#if DEBUG
        private List<double> _inputs = new List<double>();
        private List<double> _dbEnvs = new List<double>();
        private List<double> _envs = new List<double>();
        private List<double> _grs = new List<double>();
        private List<double> _outputs = new List<double>();
        private List<double> _thresholds = new List<double>();
        private List<double> _tfs = new List<double>();
#endif
        #endregion

        /// <summary>
        /// Current channel index
        /// </summary>
        private int _currentChannel;

        /// <summary>
        /// Gets or sets the sample rate.
        /// </summary>
        public double SampleRate {
            get => PP.SampleRate;
            set {
                PP.SampleRate = value;
            } 
        }

        /// <summary>
        /// Pressor parameters
        /// </summary>
        public PressorParameters PP { get; }

        /// <summary>
        /// State machine class
        /// </summary>
        private StateCoefCounter _stateCoefCounter { get; }

        /// <summary>
        /// Proxy to get single pressor state, that matches current channel indexer
        /// </summary>
        private PressorState PS => _pressorStates[_currentChannel];

        private PressorState[] _pressorStates;

        /// <summary>
        /// Constructs pressor and initializes it's params depending on incomping channels number
        /// </summary>
        /// <param name="inputCount"></param>
        public Pressor(int inputCount)
        {
            PP = new PressorParameters();
            _pressorStates = new PressorState[inputCount];

            int i = 0;
            while(i < inputCount)
            {
                _pressorStates[i] = new PressorState();
                i++;
            }

            _stateCoefCounter = new StateCoefCounter(PP, PS);
        }

        /// <summary>
        /// Core logic launching method. Does all compressor stuff and triggers state changing in state machine
        /// </summary>
        /// <param name="inBuffer">Incoming samples buffer</param>
        /// <param name="outBuffer">Outgoing samples buffer</param>
        /// <param name="currentChannel">current channel indexer to adress pressor state to correct data</param>
        public void ProcessChannel(VstAudioBuffer inBuffer, VstAudioBuffer outBuffer, int currentChannel)
        {
            ClearDebugLists();

            _currentChannel = currentChannel;

            if (PS.AvgEnv == 0)
                PS.AvgEnv = inBuffer.AvgEnv();

            if (PS.LastY == 0)
                PS.LastY = PS.AvgEnv;

            for (var i = 0; i < inBuffer.SampleCount; i++)
            {
                PS.X = inBuffer[i];

                // Count average envelope lvl
                PS.AvgEnv = PressorMath.EnvFunc(PS.AvgEnv, Math.Abs(PS.X));

                // Convert absolute average envelope lvl to db scale
                PS.DbEnv = DBFSConvert.LinToDb(PS.AvgEnv);

                // Count Gaing reduction in dbs 
                PS.GRDb = Math.Abs(PressorMath.GR(PS.DbEnv, PP.T, PP.R, PP.W));

                // Count tau function argument from current state using state machine
                PS.Tf = _stateCoefCounter.Count();

                // Count gain reduction multiplying gain reduction in dbs to tau function argument
                PS.GR = DBFSConvert.DbToLin(-(PS.GRDb * PS.Tf));

                // Count final sample through smoothing filter with respect of last final sample value
                PS.Y = (float)(PressorMath.OPFilter(0.63, PS.X * PS.GR, PS.LastY) / DBFSConvert.DbToLin(-PP.M));
                PS.LastY = PS.Y;

                outBuffer[i] = (float)PS.Y;

                WriteDebugListsInfo();
            }
        }
        private string Stringify4Log(params (string, object)[] args)
        {
            StringBuilder log = new StringBuilder();
            foreach(var (name, obj) in args)
            {
                log.AppendLine($"{name}='{obj}',{Environment.NewLine}");
            }
            return log.ToString().TrimEnd(',');
        }

        #region DEBUG_METHODS
        public void ClearDebugLists()
        {

#if DEBUG
            _inputs.Clear();
            _dbEnvs.Clear();
            _envs.Clear();
            _grs.Clear();
            _outputs.Clear();
            _thresholds.Clear();
            _tfs.Clear();
#endif
        }
        public void WriteDebugListsInfo()
        {

#if DEBUG
            _thresholds.Add(PP.T); 
            _inputs.Add(PS.X);
            _envs.Add(PS.AvgEnv);
            _dbEnvs.Add(PS.DbEnv);
            _grs.Add(PS.GRDb);
            _tfs.Add(PS.Tf);
            _outputs.Add(PS.Y);


            if (double.IsNaN(PS.Y))
                Debug.WriteLine($"Final sample is NaN, values were:{Environment.NewLine}" +
                    $"{Stringify4Log((nameof(PS.X), PS.X), (nameof(PS.GRDb), PS.GRDb), (nameof(PS.AvgEnv), PS.AvgEnv), (nameof(PS.Tf), PS.Tf), (nameof(PS.LastY), PS.LastY))}");
#endif
        }
        #endregion
    }    
}
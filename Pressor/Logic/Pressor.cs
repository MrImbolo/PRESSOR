﻿using Jacobi.Vst.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Pressor.Calculations;
using System.Linq;

namespace Pressor.Logic
{
    public enum ECompState
    {
        Bypass,
        Attack, 
        Compressing,
        Release,
    }
    internal sealed class Pressor
    {

        #region DEBUG_LISTS_DECLARATION
#if DEBUG
        private List<double> _envs = new List<double>();
        private List<double> _inputs = new List<double>();
        private List<double> _outputs = new List<double>();
        private List<double> _tfs = new List<double>();
        private List<double> _states = new List<double>();


        private List<double> _thresholds = new List<double>();
        private List<double> _dbEnvs = new List<double>();
        private List<double> _grs = new List<double>();

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
        private StateHandler SH => _stateHandlers[_currentChannel]; 

        private StateHandler[] _stateHandlers;

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
            _stateHandlers = new StateHandler[inputCount];

            int i = 0;
            while(i < inputCount)
            {
                _pressorStates[i] = new PressorState();
                _stateHandlers[i] = new StateHandler(PP, _pressorStates[i]);
                i++;
            }
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

            if (PS.Env == 0)
                PS.Env = inBuffer.AvgEnv();

            if (PS.LastY == 0)
                PS.LastY = PS.Env;

            for (var i = 0; i < inBuffer.SampleCount; i++)
            {
                PS.X = inBuffer[i];

                // Count average envelope lvl
                PS.Env = PressorCalc.EnvFunc(Math.Abs(PS.X), PS.Env);

                // Convert absolute average envelope lvl to db scale
                PS.EnvDb = DBFSConvert.LinToDb(PS.Env);

                // Count momentary gain reduction in dbs 
                PS.IsExceeded = PressorCalc.DetectEnvExceed(PS.EnvDb, PP.T, PP.W);
                
                // Count tau function argument from current state using state machine
                SH.CountTf();

                // Count GRDb according to stage
                SH.CountGRDb();

                // Count final sample through smoothing filter with respect of last final sample value
                PS.Y = (float)(PressorCalc.OPFilter(0.5, PS.X * PS.GR, PS.LastY) / DBFSConvert.DbToLin(-PP.M));
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
            _envs.Clear();
            _inputs.Clear();
            _outputs.Clear();
            _tfs.Clear();
            _states.Clear();

            _thresholds.Clear();
            _dbEnvs.Clear();
            _grs.Clear();
#endif
        }
        public void WriteDebugListsInfo()
        {

#if DEBUG
            _envs.Add(PS.Env);
            _inputs.Add(PS.X);
            _outputs.Add(PS.Y);
            _tfs.Add(PS.Tf);
            _states.Add((double)SH.State);

            _thresholds.Add(PP.T); 
            _dbEnvs.Add(PS.EnvDb);
            _grs.Add(PS.GRDb);


            if (double.IsNaN(PS.Y))
                Debug.WriteLine($"Final sample is NaN, values were:{Environment.NewLine}" +
                    $"{Stringify4Log((nameof(PS.X), PS.X), (nameof(PS.GRDb), PS.GRDb), (nameof(PS.Env), PS.Env), (nameof(PS.Tf), PS.Tf), (nameof(PS.LastY), PS.LastY))}");
#endif
        }
        #endregion
    }    
}
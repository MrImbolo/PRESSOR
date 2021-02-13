using Jacobi.Vst.Core;
using Pressor.Calculations;
using System;
using System.Collections.Generic;

namespace Pressor.Logic
{
    internal sealed class Pressor
    {

        #region DEBUG_LISTS_DECLARATION
#if DEBUG
        private readonly List<double> _xs = new List<double>();
        private readonly List<double> _ys = new List<double>();
        private readonly List<double> _cs = new List<double>();


        private readonly List<double> _xgs = new List<double>();
        private readonly List<double> _ygs = new List<double>();
        private readonly List<double> _cdbs = new List<double>();


        private readonly List<double> _thresholds = new List<double>();
        private readonly List<double> _xls = new List<double>();
        private readonly List<double> _yls = new List<double>();

#endif
        #endregion

        /// <summary>
        /// Current channel index
        /// </summary>
        private int _currentChannel;

        /// <summary>
        /// Proxy for sample rate
        /// </summary>
        public double SampleRate
        {
            get => PP.SampleRate;
            set {
                PP.SampleRate = value;
            } 
        }

        /// <summary>
        /// Pressor parameters instance
        /// </summary>
        public PressorParameters PP { get; }


        /// <summary>
        /// Proxy to get single pressor state, that matches current channel indexer
        /// </summary>
        private PressorState PS => _pressorStates[_currentChannel];

        private readonly PressorState[] _pressorStates;


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
        }

        /// <summary>
        /// Compressor core logic method. 
        /// </summary>
        /// <param name="inBuffer">Incoming samples buffer</param>
        /// <param name="outBuffer">Outgoing samples buffer</param>
        /// <param name="currentChannel">Current channel state container indexer</param>
        public void ProcessChannel(VstAudioBuffer inBuffer, VstAudioBuffer outBuffer, int currentChannel)
        {
            ClearDebugLists();
            _currentChannel = currentChannel;

            for (var i = 0; i < inBuffer.SampleCount; i++)
            {
                PS.X = inBuffer[i];

                // Add offset for the proper conversion
                // Если семпл равен нулю, добавляем офсет во избежание ошибок конвертирования
                PS.X = (PS.X == 0) ? 0.0000000001 : PS.X;

                // Convert value from linear into log domain
                // Конвертируем значение из линейного домена в логарифмический
                PS.XG = DomainConverter.LinToLog(Math.Abs(PS.X));

                #region GainProcessor

                // Calculating control voltage 
                // Высчитываем управляющее напряжение
                PS.YG = Maths.YG(PS.XG, PP.T, PP.R, PP.W);

                #endregion

                // Calculating level for detector
                // Считаем уровень 
                PS.XL = PS.XG - PS.YG;

                #region LevelDetector
                // Detecting levels
                // Детектируем фазу
                if (PS.XL > PS.LastYL)
                    // Waveform is going up => attack
                    PS.YL = PP.AlphaA * PS.LastYL + (1 - PP.AlphaA) * PS.XL;
                else
                    // Waveform is going down => release
                    PS.YL = PP.AlphaR * PS.LastYL + (1 - PP.AlphaR) * PS.XL;
                #endregion


                // Get reduction and detection level into log domain control voltage
                // Результирующая уровней и есть контролирующее 
                PS.CDb = -PS.YL;

                // Subtract makeup gain from control voltage and convert result into linear domain
                // Linear domain control voltage is ready
                PS.C = DomainConverter.LogToLin(PS.CDb + PP.M);

                #region Attenuator
                // Calculate final sample value
                PS.Y = PS.X * PS.C;

                // Buffering result level for next operation
                PS.LastYL = PS.YL;
                #endregion

                // Saving final sample into out buffer
                outBuffer[i] = (float)PS.Y;

                WriteDebugListsInfo();
            }
        }

        /// <summary>
        /// Sets all PS.LastYL to 0
        /// </summary>
        internal void EmptyBuffer()
        {
            foreach (var state in _pressorStates)
            {
                state.LastYL = 0;
            }
        }

        #region DEBUG_METHODS
        public void ClearDebugLists()
        {

#if DEBUG
            _ys.Clear(); 
            _xs.Clear();
            _cs.Clear();
            _xgs.Clear();
            _ygs.Clear();
            _cdbs.Clear();
            _xls.Clear();
            _yls.Clear();
            _thresholds.Clear();
#endif
        }
        public void WriteDebugListsInfo()
        {

#if DEBUG
            _xs.Add(PS.X);
            _ys.Add(PS.Y);
            _cs.Add(PS.C);
            _xgs.Add(PS.XG);
            _ygs.Add(PS.YG);
            _cdbs.Add(PS.CDb);
            _xls.Add(PS.XL);
            _yls.Add(PS.YL);
            _thresholds.Add(PP.T);
#endif
        }
        #endregion
    }    
}
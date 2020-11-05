using Jacobi.Vst.Plugin.Framework;
using Microsoft.VisualBasic;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Xml.Serialization;

namespace TestPlugin
{
    public class PressorParams
    {
        private int _sampleCount = 0;
        private double _gainReduction;

        //private int _attackCounter;
        //private int _releaseCounter;
        public int SampleCount { get => _sampleCount; private set => _sampleCount = (value < int.MaxValue) ? value : 0; }

        private readonly VstParameterManager _thresholdMgr;
        private readonly VstParameterManager _ratioMgr;
        private readonly VstParameterManager _attackMgr;
        private readonly VstParameterManager _releaseMgr;
        private readonly VstParameterManager _kneeMgr;
        private readonly VstParameterManager _makeupMgr;


        public PressorParams(VstParameterInfoCollection parameters) 
        {
            if (parameters == null || !parameters.Any())
                throw new ArgumentNullException(
                    $"{nameof(VstParameterInfoCollection)} type variable value is {parameters?.Count.ToString() ?? "null"}");

            _thresholdMgr = parameters.ElementAt(0).Normalize().ToManager();
            _ratioMgr = parameters.ElementAt(1).Normalize().ToManager();
            _attackMgr = parameters.ElementAt(2).Normalize().ToManager();
            _releaseMgr = parameters.ElementAt(3).Normalize().ToManager();
            _kneeMgr = parameters.ElementAt(4).Normalize().ToManager();
            _makeupMgr = parameters.ElementAt(5).Normalize().ToManager();

            SetUpInitialValues();

            SetUpPropertyChangedEvents();
        }

        /// <summary>
        /// Put all intial plugin managers values into fields
        /// </summary>
        private void SetUpInitialValues()
        {
            T = -_thresholdMgr.CurrentValue;
            R = _ratioMgr.CurrentValue;
            Ta = Math.Round(_attackMgr.CurrentValue, 0) / 1000 * SampleRate;
            Tr = Math.Round(_releaseMgr.CurrentValue, 0) / 1000 * SampleRate;
            W = _kneeMgr.CurrentValue;
            M = _makeupMgr.CurrentValue;
        }

        /// <summary>
        /// Create event handlers for all manager's params changes
        /// </summary>
        private void SetUpPropertyChangedEvents()
        {
            _thresholdMgr.PropertyChanged += new PropertyChangedEventHandler(ThresholdManager_PropertyChanged);
            _ratioMgr.PropertyChanged += new PropertyChangedEventHandler(RatioManager_PropertyChanged);
            _attackMgr.PropertyChanged += new PropertyChangedEventHandler(AttackManager_PropertyChanged);
            _releaseMgr.PropertyChanged += new PropertyChangedEventHandler(ReleaseManager_PropertyChanged);
            _kneeMgr.PropertyChanged += new PropertyChangedEventHandler(KneeManager_PropertyChanged);
            _makeupMgr.PropertyChanged += new PropertyChangedEventHandler(MakeupManager_PropertyChanged);
        }

        #region PropertyChangedEventHandlers
        private void ThresholdManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VstParameterManager.CurrentValue))
            {
                var paramMgr = (VstParameterManager)sender;
                T = -paramMgr.CurrentValue;
            }
        }
        private void RatioManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VstParameterManager.CurrentValue))
            {
                var paramMgr = (VstParameterManager)sender;
                R = paramMgr.CurrentValue;
            }
        }
        private void AttackManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VstParameterManager.CurrentValue))
            {
                var paramMgr = (VstParameterManager)sender;
                Ta = Math.Round(paramMgr.CurrentValue, 0) / 1000 * SampleRate;
            }
        }
        private void ReleaseManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VstParameterManager.CurrentValue))
            {
                var paramMgr = (VstParameterManager)sender;
                Tr = Math.Round(paramMgr.CurrentValue, 0) / 1000 * SampleRate;
            }
        }
        private void KneeManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VstParameterManager.CurrentValue))
            {
                var paramMgr = (VstParameterManager)sender;
                W = paramMgr.CurrentValue;
            }
        }
        private void MakeupManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VstParameterManager.CurrentValue))
            {
                var paramMgr = (VstParameterManager)sender;
                M = paramMgr.CurrentValue;
            }
        }
        #endregion

        /// <summary>
        /// Lin value of db based Threshold scale
        /// </summary>
        public double T { get; private set; }

        /// <summary>
        /// Dbs to Db units Ratio scale
        /// </summary>
        public double R { get; private set; }


        /// <summary>
        /// Reduction level that must not be 0 and is a ratio between (input - threshold) / (output - threshold) difference 
        /// </summary>
        public double GainReduction {
            get => _gainReduction;
            //set => _gainReduction = value > Knee && Knee > 0 ? Math.Abs((value + Knee) / 2) : Math.Abs(value);
            set => _gainReduction = Math.Abs(value);
        }


        /// <summary>
        /// Attack in sample units
        /// </summary>
        public double Ta { get; private set; }

        /// <summary>
        /// Release in sample units
        /// </summary>
        public double Tr { get; private set; }
        ///// <summary>
        ///// Attack in sample units
        ///// </summary>
        //public double AttackRatio => Math.Exp(Math.Log(0.01) / (Ta * _sampleRate * 0.001));

        ///// <summary>
        ///// Release in sample units
        ///// </summary>
        //public double ReleaseRatio => Math.Pow(0.01, 1.0 / Tr * _sampleRate * 0.001);

        /// <summary>
        /// Cimpressor curve knee in dBs
        /// </summary>
        public double W { get; private set; }
        public double M { get; private set; }
        public double Env { get; internal set; }
        //public Point LastSample { get; internal set; }
        public double SampleRate { get; set; }

        //public void SetSampleRate(double sR) => _sampleRate = sR;
    }
}

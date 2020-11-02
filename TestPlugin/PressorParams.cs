using Jacobi.Vst.Plugin.Framework;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;

namespace TestPlugin
{
    public class PressorParams
    {
        private int _sampleCount = 0;
        private double _sampleRate;
        private double _gainReduction;
        public int SampleCount { get => _sampleCount; private set => _sampleCount = (value < int.MaxValue) ? value : 0; }

        private readonly VstParameterManager _thresholdMgr;
        private readonly VstParameterManager _ratioMgr;
        private readonly VstParameterManager _attackMgr;
        private readonly VstParameterManager _releaseMgr;
        private readonly VstParameterManager _kneeMgr;


        public PressorParams(double sampleRate, VstParameterInfo trshInfo, VstParameterInfo ratInfo,
                            VstParameterInfo attInfo, VstParameterInfo relInfo, VstParameterInfo kneeInfo) 
        {
            _sampleRate = sampleRate;
            _thresholdMgr = trshInfo.Normalize().ToManager();
            _ratioMgr = ratInfo.Normalize().ToManager();
            _attackMgr = attInfo.Normalize().ToManager();
            _releaseMgr = relInfo.Normalize().ToManager();
            _kneeMgr = kneeInfo.Normalize().ToManager();

            SetUpInitialValues();

            SetUpPropertyChangedEvents();
        }

        /// <summary>
        /// Put all intial plugin managers values into fields
        /// </summary>
        private void SetUpInitialValues()
        {
            Threshold = -_thresholdMgr.CurrentValue;
            Ratio = _ratioMgr.CurrentValue;
            Attack = Math.Abs(_attackMgr.CurrentValue) / 1000 * _sampleRate;
            Release = Math.Abs(_releaseMgr.CurrentValue) / 1000 * _sampleRate;
            Knee = _kneeMgr.CurrentValue;
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
        }

        #region PropertyChangedEventHandlers
        private void ThresholdManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VstParameterManager.CurrentValue))
            {
                var paramMgr = (VstParameterManager)sender;
                Threshold = -paramMgr.CurrentValue;
            }
        }
        private void RatioManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VstParameterManager.CurrentValue))
            {
                var paramMgr = (VstParameterManager)sender;
                Ratio = paramMgr.CurrentValue;
            }
        }
        private void AttackManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VstParameterManager.CurrentValue))
            {
                var paramMgr = (VstParameterManager)sender;
                Attack = Math.Abs(paramMgr.CurrentValue) / 1000 * _sampleRate;
            }
        }
        private void ReleaseManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VstParameterManager.CurrentValue))
            {
                var paramMgr = (VstParameterManager)sender;
                Release = Math.Abs(paramMgr.CurrentValue) / 1000 * _sampleRate;
            }
        }
        private void KneeManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VstParameterManager.CurrentValue))
            {
                var paramMgr = (VstParameterManager)sender;
                Knee = paramMgr.CurrentValue;
            }
        }
        #endregion

        /// <summary>
        /// Lin value of db based Threshold scale
        /// </summary>
        public double Threshold { get; private set; }

        /// <summary>
        /// Dbs to Db units Ratio scale
        /// </summary>
        public double Ratio { get; private set; }


        /// <summary>
        /// Reduction level that must not be 0 and is a ratio between (input - threshold) / (output - threshold) difference 
        /// </summary>
        public double GainReduction {
            get => Math.Abs(_gainReduction);
            set => _gainReduction = value > Knee && Knee > 0 ? Math.Abs((value + Knee) / 2) : Math.Abs(value);
        }


        /// <summary>
        /// Attack in sample units
        /// </summary>
        public double Attack { get; private set; }

        /// <summary>
        /// Release in sample units
        /// </summary>
        public double Release { get; private set; }

        /// <summary>
        /// Cimpressor curve knee in dBs
        /// </summary>
        public double Knee { get; private set; }



        /// <summary>
        /// Current compressor curve state
        /// </summary>
        public ECurveState CurveState { get; set; } = ECurveState.None;


        public void SetSampleRate(double sR) => _sampleRate = sR;
    }
}

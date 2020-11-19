using Jacobi.Vst.Plugin.Framework;
using System;
using System.ComponentModel;
using System.Linq;

namespace Pressor.Logic
{
    public class PressorParameters
    {
        private readonly VstParameterManager _thresholdMgr;
        private readonly VstParameterManager _ratioMgr;
        private readonly VstParameterManager _attackMgr;
        private readonly VstParameterManager _releaseMgr;
        private readonly VstParameterManager _kneeMgr;
        private readonly VstParameterManager _makeupMgr;

        private VstParameterInfoCollection _parameters;
        public PressorParameters() 
        {
            _parameters = new VstParameterInfoCollection();

            #region params

            var threshInfo = new VstParameterInfo
            {
                CanBeAutomated = true,
                Name = "Thrshld",
                Label = "Threshold",
                ShortLabel = "lin2dbs",
                MinInteger = 0,
                MaxInteger = 60,
                SmallStepFloat = 0.1f,
                StepFloat = 1f,
                LargeStepFloat = 3f,
                DefaultValue = -6,
            };
            _parameters.Add(threshInfo);


            var ratInfo = new VstParameterInfo
            {
                CanBeAutomated = true,
                Name = "Ratio",
                Label = "Ratio",
                ShortLabel = ":1",
                MinInteger = 1,
                MaxInteger = 60,
                StepInteger = 1,
                LargeStepInteger = 3,
                DefaultValue = 4f,
            };
            _parameters.Add(ratInfo);


            var attInfo = new VstParameterInfo
            {
                CanBeAutomated = true,
                Name = "Attack",
                Label = "Attack",
                ShortLabel = "ms",
                MinInteger = 1,
                MaxInteger = 1000,
                StepInteger = 1,
                LargeStepInteger = 10,
                DefaultValue = 50f,
            };
            _parameters.Add(attInfo);

            var relInfo = new VstParameterInfo
            {
                CanBeAutomated = true,
                CanRamp = true,
                Name = "Release",
                Label = "Release",
                ShortLabel = "ms",
                MinInteger = 1,
                MaxInteger = 1000,
                StepInteger = 1,
                LargeStepInteger = 10,
                DefaultValue = 50f,
            };

            _parameters.Add(relInfo);


            var kneeInfo = new VstParameterInfo
            {
                CanBeAutomated = true,
                CanRamp = true,
                Name = "Knee",
                Label = "Knee",
                ShortLabel = "db to db",
                MinInteger = 0,
                MaxInteger = 10,
                StepInteger = 1,
                LargeStepInteger = 1,
                DefaultValue = 1,
            };
            _parameters.Add(kneeInfo);

            var mGainInfo = new VstParameterInfo
            {
                CanBeAutomated = true,
                CanRamp = true,
                Name = "MkGain",
                Label = "MakeUpGain",
                ShortLabel = "dbs",
                MinInteger = 0,
                MaxInteger = 60,
                StepInteger = 1,
                LargeStepInteger = 1,
                DefaultValue = 0,
            };
            _parameters.Add(mGainInfo);

            #endregion

            _thresholdMgr = _parameters.ElementAt(0).Normalize().ToManager();
            _ratioMgr = _parameters.ElementAt(1).Normalize().ToManager();
            _attackMgr = _parameters.ElementAt(2).Normalize().ToManager();
            _releaseMgr = _parameters.ElementAt(3).Normalize().ToManager();
            _kneeMgr = _parameters.ElementAt(4).Normalize().ToManager();
            _makeupMgr = _parameters.ElementAt(5).Normalize().ToManager();

            SetUpInitialValues();

            SetUpPropertyChangedEvents();
        }

        /// <summary>
        /// Put all intial plugin manager's values into fields
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
                Ta = Math.Round(paramMgr.CurrentValue, 0);
            }
        }
        private void ReleaseManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VstParameterManager.CurrentValue))
            {
                var paramMgr = (VstParameterManager)sender;
                Tr = Math.Round(paramMgr.CurrentValue, 0);
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
        public double T;

        /// <summary>
        /// Dbs to Db units Ratio scale
        /// </summary>
        public double R;

        /// <summary>
        /// Attack in sample units
        /// </summary>
        public double Ta;

        /// <summary>
        /// Release in sample units
        /// </summary>
        public double Tr;

        /// <summary>
        /// Cimpressor curve knee in dBs
        /// </summary>
        public double W;

        /// <summary>
        /// Pressor MakeUp Gain in dbs
        /// </summary>
        public double M;

        /// <summary>
        /// Project's sample rate
        /// </summary>
        public double SampleRate;

        public double AlphaA => Math.Exp(-1 / (0.001 * Ta * SampleRate));
        public double AlphaR => Math.Exp(-1 / (0.001 * Tr * SampleRate));

        public VstParameterInfoCollection Parameters { get => _parameters; set => _parameters = value; }
    }
}

using Jacobi.Vst.Core;
using Jacobi.Vst.Plugin.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;

namespace Pressor
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
        /// Pressor parameters
        /// </summary>
        private PressorParameters PP { get; }

        /// <summary>
        /// Gets or sets the sample rate.
        /// </summary>
        public double SampleRate {
            get => PP.SampleRate;
            set {
                PP.SampleRate = value;
            } 
        }


        public PressorState PS { get; }

        public Pressor(PressorParameters pp)
        {
            PP = pp;
            PS = new PressorState();
        }

        public void ProcessChannel(VstAudioBuffer inBuffer, VstAudioBuffer outBuffer)
        {
            ClearDebugLists();

            if (PS.AvgEnv == 0)
                PS.AvgEnv = inBuffer.AvgEnv();

            if (PS.LastY == 0)
                PS.LastY = PS.AvgEnv;

            for (var i = 0; i < inBuffer.SampleCount; i++)
            {
                PS.X = inBuffer[i];

                PS.AvgEnv = PressorMath.EnvFunc(PS.AvgEnv, Math.Abs(PS.X));
                PS.DbEnv = DBFSConvert.LinToDb(PS.AvgEnv);
                PS.GRDb = Math.Abs(PressorMath.GR(PS.DbEnv, PP.T, PP.R, PP.W));

                HandlePressorState(PP, PS, SampleRate);

                PS.GR = DBFSConvert.DbToLin(Math.CopySign(PS.GRDb * PS.Tf, -1));
                PS.Y = (float)(PressorMath.OPFilter(0.63, PS.GR, PS.LastY) / DBFSConvert.DbToLin(-PP.M));
                PS.LastY = PS.Y;

                outBuffer[i] = (float)PS.Y;

                WriteDebugListsInfo();
            }
        }
        /// <summary>
        /// Incapsulated logic for handling attack and release stages of the pressor
        /// </summary>
        /// <param name="pp">PressorParams instance</param>
        /// <param name="ps">PressorState instance</param>
        /// <param name="sampleRate">Sample rate</param>
        public static void HandlePressorState(PressorParameters pp, PressorState ps, double sampleRate)
        {
            if (ps.GRDb != 0)
            {
                if (ps.Da == pp.Ta && ps.Dr == pp.Tr)
                {
                    // attack end => release
                    ps.Da = 0;
                    ps.Dr--;
                    if (ps.Dr != 0 && pp.Tr != 0)
                        ps.Tf = Math.Exp(-1 / (ps.Dr / pp.Tr * sampleRate * 0.001));
                }
                else if (ps.Dr > 0 && ps.Dr < pp.Tr)
                {
                    // release on gain end => swap with attack
                    ps.Da = Math.Ceiling(ps.Dr / pp.Tr * pp.Ta);
                    ps.Dr = pp.Tr;

                    if (ps.Dr != 0 && pp.Tr != 0)
                        ps.Tf = Math.Exp(-1 / (ps.Da / pp.Ta * sampleRate * 0.001));
                }
                else if (ps.Da >= 0 && ps.Da < pp.Ta)
                {
                    // release end || attack
                    ps.Da++;
                    ps.Dr = pp.Tr;
                    if (ps.Da != 0 && pp.Ta != 0)
                        ps.Tf = Math.Exp(-1 / (ps.Da / pp.Ta * sampleRate * 0.001));
                }
            }
            else
            {
                if (ps.Da > 0)
                {
                    // Momentary attack to release state change
                    ps.Dr = Math.Ceiling(ps.Da / pp.Ta * pp.Tr);
                    ps.Da = 0;
                    if (ps.Dr != 0 && pp.Tr != 0)
                        ps.Tf = Math.Exp(-1 / (ps.Dr / pp.Tr * sampleRate * 0.001));
                }
                else if (ps.Dr > 0 && ps.Dr < pp.Tr)
                {
                    ps.Dr--;
                    if (ps.Dr != 0 && pp.Tr != 0)
                        ps.Tf = Math.Exp(-1 / (ps.Dr / pp.Tr * sampleRate * 0.001));
                }
                else
                    ps.Dr = pp.Tr;
            }
        }
        public string Stringify4Log(params (string, object)[] args)
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
    public class StateCoefCouner
    {
        private readonly PressorParameters _pp;
        private readonly PressorState _ps;
        private readonly double _sampleRate;
        public ECompState State = ECompState.Bypass;

        public StateCoefCouner(PressorParameters pp, PressorState ps, double sampleRate)
        {
            _pp = pp;
            _ps = ps;
            _sampleRate = sampleRate;
        }
    
        public double Count()
        {
            RecountState();

            return State switch
            {
                ECompState.Attack => DeltaAttackTauFunc(),
                ECompState.Release => DeltaReleaseTauFunc(),
                ECompState.Compressing => 1.0,
                _ => 0.0,
            };
        }
        public double DeltaAttackTauFunc()
        {
            if (_ps.Da != 0 && _pp.Ta != 0)
                return Math.Exp(-1 / (_ps.Da / _pp.Ta * _sampleRate * 0.001));

            return _ps.Tf;
        }
        public double DeltaReleaseTauFunc()
        {
            if (_ps.Dr != 0 && _pp.Tr != 0)
                return Math.Exp(-1 / (_ps.Dr / _pp.Tr * _sampleRate * 0.001));

            return _ps.Tf;
        }
        private void RecountState()
        {
            if (State == ECompState.Attack)
            {
                if (_ps.GRDb != 0)
                {
                    if (_ps.Da < _pp.Ta)
                    {
                        _ps.Da++;
                    }
                    else
                    {
                        SetCompressingState();
                    }
                }
                else
                {
                    SetMomentaryReleaseState();
                }
            }
            else if (State == ECompState.Compressing)
            {
                if (_ps.GRDb != 0)
                    return;
                
                SetReleaseState();
            }
            else if (State == ECompState.Release)
            {
                if (_ps.GRDb != 0)
                {
                    SetMomentaryAttackState();
                }
                else
                {
                    if (_ps.Dr > 0)
                    {
                        _ps.Dr--;
                    }
                    else
                    {
                        SetBypassState();
                    }
                }
            }
            // consider bypass
            else if (State == ECompState.Bypass)
            {
                if (_ps.GRDb == 0)
                    return;
                SetAttackState();
            }
            else 
                throw new NotImplementedException();
        }
        public void SetBypassState()
        {
            _ps.Da = 0;
            _ps.Dr = _pp.Tr;
            State = ECompState.Bypass;
        }
        public void SetMomentaryAttackState()
        {
            _ps.Da = Math.Ceiling(_ps.Dr / _pp.Tr * _pp.Ta);
            _ps.Dr = _pp.Tr;
            State = ECompState.Attack;
        }
        public void SetReleaseState()
        {
            _ps.Dr--;
            _ps.Da = 0;
            State = ECompState.Release;
        }
        public void SetCompressingState()
        {
            _ps.Da = 0;
            _ps.Dr = _pp.Tr;
            State = ECompState.Compressing;
        }
        
        public void SetMomentaryReleaseState()
        {
            _ps.Dr = Math.Ceiling(_ps.Da / _pp.Ta * _pp.Tr);
            _ps.Da = 0;
            State = ECompState.Release;
        }

        private void SetAttackState()
        {
            _ps.Da++;
            _ps.Dr = _pp.Tr;
            State = ECompState.Attack;
        }
    }
    
}
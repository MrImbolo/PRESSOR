using Pressor.Calculations;
using System;

namespace Pressor.Logic
{
    public class StateHandler
    {
        private readonly PressorParameters _pp;
        private readonly PressorState _ps;
        private double SampleRate => _pp.SampleRate;
        public ECompState State = ECompState.Bypass;

        public double StateAlpha => State switch
        {
            ECompState.Attack => 0.63,
            ECompState.Release => 0.37,
            _ => 0.5
        };

        public StateHandler(PressorParameters pp, PressorState ps)
        {
            _pp = pp;
            _ps = ps;
        }
    
        public void CountTf()
        {
            RecountState();

            _ps.Tf =  State switch
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
                return Math.Exp(-1 / (_ps.Da / _pp.Ta * SampleRate * 0.001));

            return _ps.Tf;
        }
        public double DeltaReleaseTauFunc()
        {
            if (_ps.Dr != 0 && _pp.Tr != 0)
                return Math.Exp(-1 / (_ps.Dr / _pp.Tr * SampleRate * 0.001));

            return _ps.Tf;
        }
        private void RecountState()
        {
            if (State == ECompState.Attack)
            {
                if (_ps.Da < _pp.Ta)
                {
                    _ps.Da++;
                }
                else
                {
                    SetReleaseState();
                }
                //if (_ps.IsExceeded)
                //{
                //}
                //else
                //{
                //    SetMomentaryReleaseState();
                //}
            }
            //else if (State == ECompState.Compressing)
            //{
            //    if (_ps.IsExceeded)
            //        return;
                
            //    SetReleaseState();
            //}
            else if (State == ECompState.Release)
            {
                //if (_ps.IsExceeded)
                //{
                //    SetMomentaryAttackState();
                //}
                //else
                //{
                //}
                if (_ps.Dr > 0)
                {
                    _ps.Dr--;
                }
                else
                {
                    SetBypassState();
                }
            }
            // consider bypass
            else if (State == ECompState.Bypass)
            {
                if (!_ps.IsExceeded)
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
            _ps.Da = 0; //Math.Ceiling(_ps.Dr / _pp.Tr * _pp.Ta);
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
            _ps.Da = 1;
            _ps.Dr = _pp.Tr;
            State = ECompState.Attack;
        }

        /// <summary>
        /// Counts state-depending and GR
        /// </summary>
        internal void CountGRDb()
        {
            _ps.TempGRDb = (_ps.IsExceeded) ? PressorCalc.GR(_ps.EnvDb, _pp.T, _pp.R, _pp.W) : _ps.GRDb;

            _ps.GRDb = PressorCalc.OPFilter(StateAlpha, _ps.TempGRDb * _ps.Tf, _ps.GRDb);
                
            _ps.GR = DBFSConvert.DbToLin(-_ps.GRDb);
            
        }
    }
    
}
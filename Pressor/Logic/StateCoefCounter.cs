using System;

namespace Pressor.Logic
{
    public class StateCoefCounter
    {
        private readonly PressorParameters _pp;
        private readonly PressorState _ps;
        private double SampleRate => _pp.SampleRate;
        public ECompState State = ECompState.Bypass;

        public StateCoefCounter(PressorParameters pp, PressorState ps)
        {
            _pp = pp;
            _ps = ps;
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
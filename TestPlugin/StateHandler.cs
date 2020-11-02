using System;

namespace TestPlugin
{
    public interface IStateHandler
    {
        double AttackRatio { get; }
        double GainReductionFixed { get; }
        double ReleaseRatio { get; }

        void SampleHandled();
        void SetAttackState();
        void SetBypassState();
        void SetReleaseState();
    }

    public class StateHandler : IStateHandler
    {
        private int _attackCounter;
        private int _releaseCounter;
        private readonly PressorParams _pressorParams;

        public StateHandler(PressorParams @params)
        {
            _pressorParams = @params;
        }


        /// <summary>
        /// _attackSamplesPassed to Attack smooth ratio
        /// </summary>
        public double AttackRatio => PressorMath.StraightQuadFunc(_attackCounter, _pressorParams.Attack);

        /// <summary>
        /// _releaseSamplesHandled to Release smooth ratio
        /// </summary>
        public double ReleaseRatio => PressorMath.LogReverseFunc(_releaseCounter, _pressorParams.Release);

        public double GainReductionFixed => _pressorParams.State switch
        {
            ECompState.Attack => _pressorParams.GainReduction * AttackRatio,
            ECompState.Release => _pressorParams.GainReduction * ReleaseRatio,
            _ => _pressorParams.GainReduction
        };
        /// <summary>
        /// Set attack state to compressor, e.g. set attack counter to 1 or release proportion to attack, 
        /// depending on the current state, nullify release counter and set State to Attack
        /// </summary>
        public void SetAttackState()
        {
            if (_pressorParams.State == ECompState.Bypass)
                _attackCounter = 1;
            else if (_pressorParams.State == ECompState.Release)
                _attackCounter = (int)Math.Round(ReleaseRatio * _pressorParams.Attack, 0);

            _releaseCounter = 0;
            _pressorParams.State = ECompState.Attack;
        }

        /// <summary>
        /// Nullify attack counter, set release counter to 1 and set Release state
        /// </summary>
        public void SetReleaseState()
        {
            _attackCounter = 0;
            _releaseCounter = 1;

            _pressorParams.State = ECompState.Release;
        }

        /// <summary>
        /// Nullify both attack and release counters and set Bypass state
        /// </summary>
        public void SetBypassState()
        {
            _pressorParams.GainReduction = 0;
            _releaseCounter = 0;
            _attackCounter = 0;
            _pressorParams.State = ECompState.Bypass;
        }

        public void SampleHandled()
        {
            if (_pressorParams.State == ECompState.Attack)
                _attackCounter++;

            if (_pressorParams.State == ECompState.Release)
                _releaseCounter++;

            if (_attackCounter >= _pressorParams.Attack)
                SetReleaseState();

            if (_releaseCounter >= _pressorParams.Release)
                SetBypassState();
        }
    }
}

using System;

namespace TestPlugin
{
    public interface IStateHandler
    {
        /// <summary>
        /// Gain Reduction Attack Multiplyer depending on how much attack state samples has passed
        /// </summary>
        double AttackRatio { get; }

        /// <summary>
        /// GainReduction according to current state (attack, release)
        /// </summary>
        double GainReductionFixed { get; }
        /// <summary>
        /// Gain Reduction Release Multiplyer depending on how much release state samples has passed
        /// </summary>
        double ReleaseRatio { get; }
        /// <summary>
        /// Current state of the pressor state machine
        /// </summary>
        ECompState State { get; }

        /// <summary>
        /// Notify IStateHandler current sample is fully handled and counters must increment
        /// </summary>
        void SampleHandled();
        /// <summary>
        /// Set state machine into the increasing gain reduction attack state
        /// </summary>
        void SetAttackState();
        /// <summary>
        /// Set state machine into the decreasing gain reduction release state
        /// </summary>
        void SetReleaseState();
        /// <summary>
        /// Set state machine into passive non-reducting bypass state
        /// </summary>
        void SetBypassState();
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

        public double GainReductionFixed => State switch
        {
            ECompState.Attack => _pressorParams.GainReduction * AttackRatio,
            ECompState.Release => _pressorParams.GainReduction * ReleaseRatio,
            _ => _pressorParams.GainReduction
        };

        /// <summary>
        /// Current phase of compressor
        /// </summary>
        public ECompState State { get; set; } = ECompState.Bypass;

        /// <summary>
        /// Set attack state to compressor, e.g. set attack counter to 1 or release proportion to attack, 
        /// depending on the current state, nullify release counter and set State to Attack
        /// </summary>
        public void SetAttackState()
        {
            if (State == ECompState.Bypass)
            {
                _attackCounter = 1;
                State = ECompState.Attack;
            }
            else if (State == ECompState.Release)
            {
                _attackCounter = (int)Math.Round(ReleaseRatio * _pressorParams.Attack, 0);
                State = ECompState.Attack;
            }
            _releaseCounter = 0;
        }

        /// <summary>
        /// Nullify attack counter, set release counter to 1 and set Release state
        /// </summary>
        public void SetReleaseState()
        {
            _attackCounter = 0;
            _releaseCounter = 1;

            State = ECompState.Release;
        }

        /// <summary>
        /// Nullify both attack and release counters and set Bypass state
        /// </summary>
        public void SetBypassState()
        {
            _pressorParams.GainReduction = 0;
            _releaseCounter = 0;
            _attackCounter = 0;
            State = ECompState.Bypass;
        }
        /// <summary>
        /// Set state machine into passive non-reducting bypass state
        /// </summary>
        public void SampleHandled()
        {
            if (State == ECompState.Attack)
                _attackCounter++;

            if (State == ECompState.Release)
                _releaseCounter++;

            if (_attackCounter >= _pressorParams.Attack)
                SetReleaseState();

            if (_releaseCounter >= _pressorParams.Release)
                SetBypassState();
        }
    }
}

namespace TestPlugin
{
    public interface IGainProcessor
    {
        Sample Process(Sample sample);
    }
    internal class GainProcessor : IGainProcessor
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Library code")]
        private readonly PressorParams _pressorParams;
        private readonly IStateHandler _stateHandler;

        public GainProcessor(PressorParams @params, IStateHandler stateHandler)
        {
            _pressorParams = @params;
            _stateHandler = stateHandler;
        }

        public Sample Process(Sample sample)
        {
            sample.Abs -= _stateHandler.GainReductionFixed;

            _stateHandler.SampleHandled();
            return sample;
        }
    }
}

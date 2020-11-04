namespace TestPlugin
{
    public interface IGainProcessor
    {
        void Process(Point sample);
        void OPFilter(Point sample, Point lastSample);
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

        public void OPFilter(Point sample, Point lastSample)
        {
            sample.Dbs = 0.63 * lastSample.Dbs + (1 - 0.63) * sample.Dbs;
        }

        public void Process(Point sample)
        {
            sample.Dbs -= _stateHandler.GainReductionFixed;
            _stateHandler.SampleHandled();
        }

    }
}

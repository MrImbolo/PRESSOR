namespace TestPlugin
{
    public interface IGainProcessor
    {
        Sample Process(Sample sample);
    }
    internal class GainProcessor : IGainProcessor
    {
        private readonly PressorParams _pressorParams;

        public GainProcessor(PressorParams @params)
        {
            _pressorParams = @params;
        }

        public Sample Process(Sample sample)
        {
            sample.Abs -= _pressorParams.GainReductionFixed;
            _pressorParams.SampleHandled();
            return sample;
        }
    }
}

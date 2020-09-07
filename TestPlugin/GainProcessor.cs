namespace TestPlugin
{
    public interface IGainProcessor
    {
        Sample Process(Sample sample);
    }
    internal class GainProcessor : IGainProcessor
    {
        private readonly PressorParams _params;

        public GainProcessor(PressorParams @params)
        {
            _params = @params;
        }

        public Sample Process(Sample sample)
        {
            if (_params.State == ECompState.Attack)
            {
                if (sample.IsAbove(_params.Threshold) && _params.RatioFixed > 1)
                {
                    var tempSample = Count(sample.Abs);
                    _params.GainReduction = sample.Abs - tempSample;
                    sample.Abs = tempSample;
                }
                _params.SampleHandled();
            }
            else if (_params.State == ECompState.Release)
            {
                if (sample.IsAbove(_params.Threshold) && _params.RatioFixed > 1)
                {
                    var tempSample = Count(sample.Abs);
                    _params.GainReduction = sample.Abs - tempSample;
                    sample.Abs = tempSample;
                }
                else
                {


                    _params.SampleHandled();
                }
            }


            return sample;
        }

        private float Count(float val) => _params.Threshold + (val - _params.Threshold) / _params.RatioFixed;
    }
}

using System;

namespace TestPlugin
{
    public class EnvelopeHandler
    {
        private readonly PressorParams _pressorParams;

        public EnvelopeHandler(PressorParams @params)
        {
            _pressorParams = @params;
        }
    }

}

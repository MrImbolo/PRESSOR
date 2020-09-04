using System;

namespace TestPlugin
{
    public struct Sample
    {
        public Sample(float sample)
        {
            var value = (sample > 1)
                ? 1
                : (sample < -1)
                    ? -1
                    : sample;

            Abs = Math.Abs(value);
            Sign = (value < 0) ? -1 : 1;
        }

        public float Value => Abs * Sign;
        public float Sign { get; set; }
        public float Abs { get; set; }
        public bool IsZero => Value == 0;

        public bool IsAbove(float threshold) => Abs > threshold;
    }
}

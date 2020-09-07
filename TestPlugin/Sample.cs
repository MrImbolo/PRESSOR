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

        public override string ToString() => Value.ToString();

        public override bool Equals(object obj) => 
            (obj is Sample sample) 
                ? sample.Value == Value
            : (obj is float fSample) 
                ? Value == fSample
            : (obj is double dSample)
                ? Value == (float)dSample
            : obj.GetHashCode() == GetHashCode();

        public override int GetHashCode() => HashCode.Combine(Value, Sign, Abs, IsZero);

        public static bool operator ==(Sample a, Sample b) => a.Value == b.Value;
        public static bool operator !=(Sample a, Sample b) => a.Value != b.Value;
    }
}

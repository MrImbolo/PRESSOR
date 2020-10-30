using System;

namespace TestPlugin
{
    public struct Sample
    {
        public Sample(double sample)
        {
            var value = (sample > 1)
                ? 1
                : (sample < -1)
                    ? -1
                    : sample;

            Abs = Math.Abs(value);
            Sign = (value < 0) ? -1 : 1;
        }

        public double Value => Abs * Sign;
        public double Sign { get; set; }
        public double Abs { get; set; }
        public bool IsZero => Value == 0;

        public bool IsAbove(double threshold) => Abs > threshold;

        public override string ToString() => Value.ToString();

        public override bool Equals(object obj) => 
            (obj is Sample sample) 
                ? sample.Value == Value
            : (obj is double fSample) 
                ? Value == fSample
            : (obj is double dSample)
                ? Value == (double)dSample
            : obj.GetHashCode() == GetHashCode();

        public override int GetHashCode() => HashCode.Combine(Value, Sign, Abs, IsZero);

        public static bool operator ==(Sample a, Sample b) => a.Value == b.Value;
        public static bool operator !=(Sample a, Sample b) => a.Value != b.Value;
        public static bool operator >(Sample a, Sample b) => a.Value > b.Value;
        public static bool operator <(Sample a, Sample b) => a.Value < b.Value;
        public static bool operator >=(Sample a, Sample b) => a.Value >= b.Value;
        public static bool operator <=(Sample a, Sample b) => a.Value <= b.Value;
    }
}

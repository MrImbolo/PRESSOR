using System;

namespace TestPlugin
{
    public struct Point
    {
        public Point(double sample)
        {
            Initial = sample;
            var value = (sample > 1)
                ? 1
                : (sample < -1)
                    ? -1
                    : sample;

            Abs = Math.Abs(value);
            Dbs = DBFSConvert.LinToDb(Abs);
            Sign = Math.Sign(value);
        }

        public readonly double Initial;
        public double Value => Math.CopySign(DBFSConvert.DbToLin(Dbs), Sign);
        public double Sign { get; set; }
        public double Dbs { get; set; }
        public bool IsZero => Value == 0;

        public double Abs { get; internal set; }

        public bool IsAbove(double threshold) => Dbs > threshold;

        public override string ToString() => Value.ToString();

        public override bool Equals(object obj) => 
            (obj is Point sample) 
                ? sample.Value == Value
            : (obj is double fSample) 
                ? Value == fSample
            : (obj is double dSample)
                ? Value == (double)dSample
            : obj.GetHashCode() == GetHashCode();

        public override int GetHashCode() => HashCode.Combine(Value, Sign, Dbs, IsZero);

        public static bool operator ==(Point a, Point b) => a.Value == b.Value;
        public static bool operator !=(Point a, Point b) => a.Value != b.Value;
        public static bool operator >(Point a, Point b) => a.Value > b.Value;
        public static bool operator <(Point a, Point b) => a.Value < b.Value;
        public static bool operator >=(Point a, Point b) => a.Value >= b.Value;
        public static bool operator <=(Point a, Point b) => a.Value <= b.Value;
    }
}

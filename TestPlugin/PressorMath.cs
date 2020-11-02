using System;

namespace TestPlugin
{
    public static class PressorMath
    {
        private const double _cQuadraticExp = 2;
        public static double LogReverseFunc(double a, double b) => Math.Log(b - a + 1) / Math.Log(b + 1);
        public static double StraightQuadFunc(double a, double b) => Math.Pow(a, _cQuadraticExp) / Math.Pow(b, _cQuadraticExp);
    }
}

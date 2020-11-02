using System;

namespace TestPlugin
{
    public static class PressorMath
    {
        private const double CQuadraticExp = 3;
        public static double LogReverseFunc(double a, double b) => Math.Log(b - a + 1) / Math.Log(b + 1);
        public static double StraightQuadFunc(double a, double b) => Math.Pow(a, CQuadraticExp) / Math.Pow(b, CQuadraticExp);
    }
}

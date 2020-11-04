using System;
using System.Reflection.Metadata.Ecma335;

namespace TestPlugin
{
    public static class PressorMath
    {
        public const double CQuadraticExp = 2;
        public const double CWEnv = 0.35;
        public static double LogReverseFunc(double a, double b) => Math.Log(b - a + 1) / Math.Log(b + 1);
        public static double StraightQuadFunc(double a, double b) => Math.Pow(a, CQuadraticExp) / Math.Pow(b, CQuadraticExp);

        /// <summary>
        /// Count envelope in absolute values
        /// </summary>
        /// <param name="env"></param>
        /// <param name="sample"></param>
        /// <returns></returns>
        public static double EnvFunc(double env, double sample) => (CWEnv * sample + (1 - CWEnv) * env);

        public static double OPFilter(double alpha, double a, double b) => alpha * a + (1 - alpha) * b;


        /// <summary>
        /// Count gain reduction. All in dBs
        /// </summary>
        /// <param name="env"></param>
        /// <param name="t"></param>
        /// <param name="r"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        public static double GR(double env, double t, double r, double w) 
            => 2 * (env - t) < -w
                ? 0
                : (2 * Math.Abs(env - t) <= w)
                    ? (1 / r - 1) * Math.Pow(env - t + w / 2, 2) / (2 * w)
                    : env - (t + (env - t) / r);

    }
}

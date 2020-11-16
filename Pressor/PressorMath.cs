using System;
using System.Reflection.Metadata.Ecma335;

namespace Pressor
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
        public static double EnvFunc(double env, double sample) => OPFilter(CWEnv, sample, env);

        /// <summary>
        /// Smooth filter for curves
        /// </summary>
        /// <param name="alpha">Smooth coefficient </param>
        /// <param name="a">Dynamic parameter</param>
        /// <param name="b">Buffer parameter</param>
        /// <returns></returns>
        public static double OPFilter(double alpha, double a, double b) => alpha * a + (1 - alpha) * b;

        /// <summary>
        /// tf argument counting function. Can return NaN if delta, time or smplRate is 0
        /// </summary>
        /// <param name="tf">Ref tf var</param>
        /// <param name="delta">Time delta e.g. attack delta or release delta</param>
        /// <param name="time">Time variable like attack time or release time</param>
        /// <param name="smplRate">Project sample rate, usually is set up by host</param>
        public static double Tf(double delta, double time, double smplRate)
            => Math.Exp(-1 / delta / time * smplRate * 0.001);

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
                : (2 * Math.Abs(env - t) <= w && w > 0)
                    ? (1 / r - 1) * Math.Pow(env - t + w / 2, 2) / (2 * w)
                    : env - (t + (env - t) / r);

    }
}

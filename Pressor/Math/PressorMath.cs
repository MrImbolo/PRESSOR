using System;
using System.Net.NetworkInformation;
using System.Reflection.Metadata.Ecma335;

namespace Pressor.Calculations
{
    public static class PressorCalc
    {
        public const double CQuadraticExp = 2;
        public const double CWEnv = 0.37;

        /// <summary>
        /// Count envelope in absolute values
        /// </summary>
        /// <param name="env"></param>
        /// <param name="sample"></param>
        /// <returns></returns>
        public static double EnvFunc(double sample, double env) => OPFilter(CWEnv, sample, env);

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
        {
            if (2 * (env - t) < -w)
            {
                return 0;
            }
            else if (2 * Math.Abs(env - t) <= w && w > 0)
            {
                return (1 / r - 1) * Math.Pow(env - t + w / 2, 2) / (2 * w);
            }
            else
            {
                return env - (t + (env - t) / r);
            }
        }

        /// <summary>
        /// Finds out whether env is exceeding threshold or not depending on T and W
        /// </summary>
        /// <param name="env">Envelope level in Dbs</param>
        /// <param name="t">Threshold level in Dbs</param>
        /// <param name="w">Knee width level in Dbs</param>
        /// <returns>Is the env above the threshold or inside the knee or not</returns>
        public static bool DetectEnvExceed(double env, double t, double w) => !(2 * (env - t) < -w);
    }
}

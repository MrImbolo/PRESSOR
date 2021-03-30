using System;
using System.Net.NetworkInformation;
using System.Reflection.Metadata.Ecma335;

namespace Pressor.Calculations
{
    public static class Maths
    {
        /// <summary>
        /// Smoothing filter function
        /// <para><paramref name="alpha"/> * <paramref name="a"/> + (1 - <paramref name="alpha"/>) * <paramref name="b"/></para>
        /// </summary>
        /// <param name="alpha">Smoothing coefficient</param>
        /// <param name="a">Dynamic parameter</param>
        /// <param name="b">Buffer parameter</param>
        /// <returns><paramref name="alpha"/> * <paramref name="a"/> + (1 - <paramref name="alpha"/>) * <paramref name="b"/></returns>
        public static double OnePoleFilter(double alpha, double a, double b) => alpha * a + (1 - alpha) * b;

        /// <summary>
        /// Counts YG in log domain
        /// </summary>
        /// <param name="xg">Gain of X (-120db...0db)</param>
        /// <param name="t">Threshold  (-120db...0db)</param>
        /// <param name="r">Ratio  (0...60)</param>
        /// <param name="w">Knee Width (0...10)</param>
        /// <returns>Gain of Y  (-120db...0db)</returns>
        public static double YG(double xg, double t, double r, double w)
            => (2 * (xg - t) < -w)
                    ? xg
                    : (2 * Math.Abs(xg - t) <= w && w > 0)
                        ? (xg + (1 / r - 1) * Math.Pow(xg - t + w / 2, 2) / (2 * w))
                        : (t + ((xg - t) / r));
    }
}

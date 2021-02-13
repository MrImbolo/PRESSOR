using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pressor.Logic
{
    public class PressorState
    {
        /// <summary>
        /// Initial sample or offset if zero
        /// <para>Linear Domain: -1...-0.001 ; 0.001...1</para>
        /// </summary>
        public double X;

        /// <summary>
        /// Final sample
        /// <para>Linear Domain: -1...-0.001 ; 0.001...1</para>
        /// </summary>
        public double Y;

        /// <summary>
        /// Control voltage aka Gain Reduction adjusted with MakeupGain and linearized
        /// <para>Linear Domain: -1...1</para>
        /// </summary>
        public double C;

        /// <summary>
        /// Gain of X
        /// <para>Log Domain: -120db ... 0db</para>
        /// </summary>
        public double XG;

        /// <summary>
        /// Gain of Y, found by gain processor
        /// <para>Log Domain: -120db ... 0db</para>
        /// </summary>
        public double YG;

        /// <summary>
        /// Control voltage aka Gain Reduction
        /// <para>Log Domain: -120db ... 0db</para>
        /// </summary>
        public double CDb;


        /// <summary>
        /// Level of X found by gain processor
        /// <para>Log Domain: 0+db</para>
        /// </summary>
        public double XL;

        /// <summary>
        /// Level of Y found by detector
        /// <para>Log Domain: 0+db</para>
        /// </summary>
        public double YL;

        /// <summary>
        /// Beffered YL [n-1]
        /// <para>Log Domain: 0+db</para>
        /// </summary>
        public double LastYL;
    }
}

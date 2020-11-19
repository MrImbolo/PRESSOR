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
        /// Current sample intial absolute value
        /// </summary>
        public double X { get; internal set; } = 0;
        
        /// <summary>
        /// Current sample final absolute value
        /// </summary>
        public double Y { get; internal set; }
        

        /// <summary>
        /// Current envelope in dbs
        /// </summary>
        public double EnvDb { get; internal set; }

        /// <summary>
        /// Delta of the attack
        /// </summary>
        public double Da { get; set; }

        /// <summary>
        /// Delta of the release
        /// </summary>
        public double Dr { get; set; }

        /// <summary>
        /// Last output value
        /// </summary>
        public double LastYL { get; set; }

        /// <summary>
        /// Average envelope - always positive
        /// </summary>
        public double XG { get; set; }

        /// <summary>
        /// Gain reduction absolute
        /// </summary>
        public double CDb { get; set; } = 1.0;

        /// <summary>
        /// Inbetween Y value in db
        /// </summary>
        public double TempYDb { get; set; }

        /// <summary>
        /// Y in dBs
        /// </summary>
        public double YG { get; set; }

        /// <summary>
        /// Tau-coefficient for current compressor state
        /// </summary>
        public double Tf { get; set; }

        /// <summary>
        /// Envelope exceedence flag
        /// </summary>
        public bool IsExceeded { get; set; }
        public double  XL { get; internal set; }
        public double YL { get; internal set; }
        public double C { get; internal set; }
    }
}

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
        public double DbEnv { get; internal set; }

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
        public double LastY { get; set; }

        /// <summary>
        /// Average envelope level till now
        /// </summary>
        public double AvgEnv { get; set; }

        /// <summary>
        /// Gain reduction absolute
        /// </summary>
        public double GR { get; set; } = 1.0;

        /// <summary>
        /// Momentary GainReduction for the state detecting
        /// </summary>
        public double TempGRDb { get; set; }

        /// <summary>
        /// Gain Reduction in dBs
        /// </summary>
        public double GRDb { get; set; }

        /// <summary>
        /// Tau-coefficient for current compressor state
        /// </summary>
        public double Tf { get; set; }
    }
}

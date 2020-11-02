using System;

namespace TestPlugin
{
    internal static class DBFSConvert
    {
        /// <summary>
        /// Minimal signal constant
        /// </summary>
        private const double _epsilon = 0.001f;


        /// <summary>
        /// Constant coefficient for dbfs convertion
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Just in case")]
        private static readonly double _dbfsCoef = 20 / Math.Log(10);

        /// <summary>
        /// From linear scale to expanential one
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static double ToExpanent(double val)
        {
            var volume = (double) Math.Pow(_epsilon, 1 - val);
            return volume > _epsilon ? volume : 0;
        }

        /// <summary>
        /// From expanential scale to linear one
        /// </summary>
        /// <param name="volume"></param>
        /// <returns></returns>
        public static double FromExpanent(double volume) 
            => 1 - (double) Math.Log(Math.Max(volume, _epsilon)) / (double)Math.Log(_epsilon);

        /// <summary>
        /// Exponential scale convertion to Dbs
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static double LinToDb(double val) => (double) (20 * Math.Log10(val));

        /// <summary>
        /// From Dbs to exponential scale
        /// </summary>
        /// <param name="dbfs"></param>
        /// <returns></returns>
        public static double DbToLin(double dbfs) => (double) Math.Pow(10, 0.05 * dbfs);

        /// <summary>
        /// Calculates scale position from dbs spl (-60 == 0)
        /// </summary>
        /// <param name="db">Dbs from 0 to -60</param>
        /// <returns>Scale position from 0 to 1</returns>
        public static double DbSplToScalePartition(double db) => FromExpanent(DbToLin(db));

        /// <summary>
        /// Converts scale position (from 0 to 1) to -Dbs from maximum loudness
        /// </summary>
        /// <param name="pos">Position (from 0 to 1)</param>
        /// <returns>-Dbs from maximum loudness</returns>
        public static double FromScaleToDbSpl(double pos) => LinToDb(ToExpanent(pos));
    }
}

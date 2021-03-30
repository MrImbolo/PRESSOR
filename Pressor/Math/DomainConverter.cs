using System;

namespace Pressor.Calculations
{
    internal static class DomainConverter
    {
        ///// <summary>
        ///// Minimal signal constant
        ///// </summary>
        //private const double _epsilon = 0.001f;


        ///// <summary>
        ///// Constant coefficient for dbfs convertion
        ///// </summary>
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Just in case")]
        //private static readonly double _dbfsCoef = 20 / Math.Log(10);

        ///// <summary>
        ///// From linear scale to expanential one
        ///// </summary>
        ///// <param name="val"></param>
        ///// <returns></returns>
        //public static double ToExpanent(double val)
        //{
        //    var volume = (double)Math.Pow(_epsilon, 1 - val);
        //    return volume > _epsilon ? volume : 0;
        //}

        ///// <summary>
        ///// From expanential scale to linear one
        ///// </summary>
        ///// <param name="volume"></param>
        ///// <returns></returns>
        //public static double FromExpanent(double volume)
        //    => 1 - (double)Math.Log(Math.Max(volume, _epsilon)) / (double)Math.Log(_epsilon);

        /// <summary>
        /// LINEAR => LOG domain value convertion
        /// <para>-1...1 => -120db...0db</para>
        /// </summary>
        /// <param name="val">Linear domain value: -1...1 </param>
        /// <returns>Log domain value: -120db...0db</returns>
        public static double LinToLog(double val) => 20 * Math.Log10(val);

        /// <summary>
        /// LOG => LINEAR domain convertion
        /// <para>-1...1 => -120db...0db</para>
        /// </summary>
        /// <param name="db">Log domain value: -120db...0db</param>
        /// <returns>Linear domain value: -1...1 </returns>
        public static double LogToLin(double db) => Math.Pow(10, 0.05 * db);

        ///// <summary>
        ///// Calculates scale position from dbs spl (-60 == 0)
        ///// </summary>
        ///// <param name="db">Dbs from 0 to -60</param>
        ///// <returns>Scale position from 0 to 1</returns>
        //public static double DbSplToScalePartition(double db) => FromExpanent(DbToLin(db));

        ///// <summary>
        ///// Converts scale position (from 0 to 1) to -Dbs from maximum loudness
        ///// </summary>
        ///// <param name="pos">Position (from 0 to 1)</param>
        ///// <returns>-Dbs from maximum loudness</returns>
        //public static double FromScaleToDbSpl(double pos) => LinToDb(ToExpanent(pos));
    }
}

using System;

namespace TestPlugin
{
    internal static class DBFSConvert
    {
        /// <summary>
        /// Minimal signal constant
        /// </summary>
        private const float _epsilon = 0.001f;
        
        /// <summary>
        /// Constant coefficient for dbfs convertion
        /// </summary>
        private static readonly float _dbfsCoef = (float) (20 / Math.Log(10));

        /// <summary>
        /// From linear scale to expanential one
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static float ToExpanent(float val)
        {
            var volume = (float) Math.Pow(_epsilon, 1 - val);
            return volume > _epsilon ? volume : 0;
        }

        /// <summary>
        /// From expanential scale to linear one
        /// </summary>
        /// <param name="volume"></param>
        /// <returns></returns>
        public static float FromExpanent(float volume) 
            => 1 - (float) Math.Log(Math.Max(volume, _epsilon)) / (float)Math.Log(_epsilon);

        /// <summary>
        /// Exponential scale convertion to Dbs
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static float LinToDb(float val) => (float) (20 * Math.Log10(val));

        /// <summary>
        /// From Dbs to exponential scale
        /// </summary>
        /// <param name="dbfs"></param>
        /// <returns></returns>
        public static float DbToLin(float dbfs) => (float) Math.Pow(10, 0.05 * dbfs);

        /// <summary>
        /// Calculates scale position from dbs spl (-60 == 0)
        /// </summary>
        /// <param name="db">Dbs from 0 to -60</param>
        /// <returns>Scale position from 0 to 1</returns>
        public static float DbSplToScalePartition(float db) => FromExpanent(DbToLin(db));

        /// <summary>
        /// Converts scale position (from 0 to 1) to -Dbs from maximum loudness
        /// </summary>
        /// <param name="pos">Position (from 0 to 1)</param>
        /// <returns>-Dbs from maximum loudness</returns>
        public static float FromScaleToDbSpl(float pos) => LinToDb(ToExpanent(pos));
    }
}

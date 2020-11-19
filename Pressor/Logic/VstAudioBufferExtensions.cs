﻿using Jacobi.Vst.Core;
using System;
using System.Linq;

namespace Pressor.Logic
{
    public static class VstAudioBufferExtensions
    {
        public static bool IsEmpty(this VstAudioBuffer[] channels) => channels.All(x => x[0] == 0 && x[x.SampleCount - 1] == 0);
        public static double AvgEnv(this VstAudioBuffer buffer)
        {
            double[] lvls = new double[buffer.SampleCount];
            for (int i = 0; i < buffer.SampleCount; i++)
                lvls[i] = Math.Abs(buffer[i]);

            return lvls.Average(x => Math.Abs(x));
        }
    }
}

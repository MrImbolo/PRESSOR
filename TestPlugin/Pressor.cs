using Jacobi.Vst.Core;
using Jacobi.Vst.Plugin.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;

namespace TestPlugin
{
    public enum ECompState
    {
        Bypass,
        Attack, 
        Release,
        Compressing
    }
    internal sealed class Pressor
    {
        //private Point _sample;
        //private float _sampleRate;
        private int _sampleCount = 0;
        private double _da = 0;
        private double _dr = 0;
        private double _lx;
        private double _avgEnv;

        private List<double> _inputs = new List<double>();
        private List<double> _dbEnvs = new List<double>();
        private List<double> _envs = new List<double>();
        private List<double> _grs = new List<double>();
        private List<double> _outputs = new List<double>();
        private List<double> _thresholds = new List<double>();
        private List<double> _tfs = new List<double>();
        public int SampleCount { get => _sampleCount; private set => _sampleCount = (value < int.MaxValue) ? value : 0; }
        private PressorParams PP { get; }

        /// <summary>
        /// Gets or sets the sample rate.
        /// </summary>
        public double SampleRate {
            get => PP.SampleRate;
            set {
                PP.SampleRate = value;
            } 
        }

        public Pressor(PressorParams pp)
        {
            PP = pp;
        }

        public void ProcessChannel(VstAudioBuffer inBuffer, VstAudioBuffer outBuffer)
        {
            _inputs.Clear();
            _dbEnvs.Clear();
            _envs.Clear();
            _grs.Clear();
            _outputs.Clear();
            _thresholds.Clear();
            _tfs.Clear();

            var t = PP.T;
            var r = PP.R;
            var w = PP.W;
            
            if (_avgEnv == 0)
                _avgEnv = inBuffer.AvgEnv();

            if (_lx == 0)
                _lx = _avgEnv;

            for (var i = 0; i < inBuffer.SampleCount; i++)
            {
                double xi = inBuffer[i];
                double yi = 0.0;
                double tf = 0.0;
                double gri = 1.0;

                _thresholds.Add(t);
                _inputs.Add(xi);

                _avgEnv = PressorMath.EnvFunc(_avgEnv, Math.Abs(xi));
                _envs.Add(_avgEnv);
                var env = DBFSConvert.LinToDb(_avgEnv);

                _dbEnvs.Add(env);

                // testing
                var gr = Math.Abs(PressorMath.GR(env, t, r, w));
                _grs.Add(gr);

                if (gr == 0)
                {
                    if (_da > 0)
                    {
                        // Momentary attack to release state change
                        _dr = Math.Ceiling(_da / PP.Ta * PP.Tr);
                        _da = 0;
                        tf = Math.Exp(-1 / (_dr / PP.Tr * SampleRate * 0.001));
                    }
                    else if (_dr > 0 && _dr < PP.Tr)
                    {
                        _dr--;
                        tf = Math.Exp(-1 / (_dr / PP.Tr * SampleRate * 0.001));
                    }
                    else
                        _dr = PP.Tr;
                }
                else
                {
                    if (_da == PP.Ta && _dr == PP.Tr)
                    {
                        // attack end => release
                        _da = 0;
                        _dr--;
                        tf = Math.Exp(-1 / (_dr / PP.Tr * SampleRate * 0.001));
                    }
                    else if (_dr > 0 && _dr < PP.Tr)
                    {
                        // release
                        _dr--;
                        tf = Math.Exp(-1 / (_dr / PP.Tr * SampleRate * 0.001));
                    }
                    else if (_da >= 0 && _da < PP.Ta)
                    {
                        // release end || attack
                        _da++;
                        _dr = PP.Tr;
                        tf = Math.Exp(-1 / (_da / PP.Ta * SampleRate * 0.001));
                    }
                }

                _tfs.Add(tf);

                gri = DBFSConvert.DbToLin(Math.CopySign(gr * tf, -1));

                yi = PressorMath.OPFilter(0.63, xi * gri, _lx);

                if (double.IsNaN(yi))
                    Debug.WriteLine($"Final sample is NaN, values were:{Environment.NewLine}" +
                        $"{Stringify4Log((nameof(xi), xi), (nameof(gri), gri), (nameof(env), env), (nameof(tf), tf), (nameof(_lx), _lx))}");

                outBuffer[i] = (float)(yi / DBFSConvert.DbToLin(-PP.M));
                _outputs.Add(yi);
                _lx = yi;
            }
        }
        public string Stringify4Log(params (string, object)[] args)
        {
            StringBuilder log = new StringBuilder();
            foreach(var (name, obj) in args)
            {
                log.AppendLine($"{name}='{obj}',{Environment.NewLine}");
            }
            return log.ToString().TrimEnd(',');
        }
    }
}
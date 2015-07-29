using System;
using System.Collections.Generic;

namespace DataClasses
{
    #region Force feedback
    
    public class EffectTypeData
    {
        public Guid effectGuid { get; set; }

        public int gain { get; set; }
        public int duration { get; set; }
        public int startDelay { get; set; }
        public int trigRepInterval { get; set; }
        public int trigButton { get; set; }

        private List<int> directions;

        public void AddDirection(int dir)
        {
            if (directions == null)
                directions = new List<int>();

            directions.Add(dir);
        }

        public int[] GetDirections()
        {
            int[] rawDirs = new int[directions.Count];
            int i = 0;
            foreach (int dir in directions)
                rawDirs[i++] = dir;

            return rawDirs;
        }
    }

    public class ConstantEffectTypeData : EffectTypeData
    {
        public int magnitude { get; set; }
    }

    public class RampEffectTypeData : EffectTypeData
    {
        public int start { get; set; }
        public int end { get; set; }
    }

    public class PeriodicEffectTypeData : EffectTypeData
    {
        public int magnitude { get; set; }
        public int offset { get; set; }
        public int period { get; set; }
        public int phase { get; set; }
    }

    public class ConditionEffectTypeData : EffectTypeData
    {
        public int deadBand { get; set; }
        public int negCoeff { get; set; }
        public int negSat { get; set; }
        public int offset { get; set; }
        public int posCoeff { get; set; }
        public int posSat { get; set; }
    }

    public class EnvelopeData
    {
        public int attackLevel { get; set; }
        public int attackTime { get; set; }
        public int fadeLevel { get; set; }
        public int fadeTime { get; set; }
    }

    public class FFBEffectData
    {
        public EffectTypeData effect;
        public EnvelopeData envelope;
        public int slot;

        public FFBEffectData(EffectTypeData efd, EnvelopeData evd, int s)
        {
            effect = efd;
            envelope = evd;
            slot = s;
        }
    }

    #endregion
    #region Device info
    public class AxesInput
    {
        public int xAxis { get; set; }
        public int yAxis { get; set; }
        public int zAxis { get; set; }

        public int rxAxis { get; set; }
        public int ryAxis { get; set; }
        public int rzAxis { get; set; }

        public int fX { get; set; }
        public int fY { get; set; }
        public int fZ { get; set; }
    }

    public class AxesRange
    {
        public int minimum { get; set; }
        public int maximum { get; set; }
    }

    public class HardwareInfo
    {
        public int axesCount { get; set; }
        public int buttonCount { get; set; }
        public int povCount { get; set; }
        public int driverVersion { get; set; }
        public int fwVersion { get; set; }
        public int hwRevision { get; set; }
        public int ffbMinTime { get; set; }
        public int ffbSamplePer { get; set; }
    };
#endregion
}
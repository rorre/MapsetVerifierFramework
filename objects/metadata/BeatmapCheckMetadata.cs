using MapsetParser.objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapsetVerifierFramework.objects.metadata
{
    public class BeatmapCheckMetadata : CheckMetadata
    {
        /// <summary> The mode(s) this check applies to, by default all. </summary>
        public Beatmap.Mode[] Modes { get; set; } = new Beatmap.Mode[]
        {
            Beatmap.Mode.Standard,
            Beatmap.Mode.Taiko,
            Beatmap.Mode.Catch,
            Beatmap.Mode.Mania
        };

        /// <summary> The difficulties this check applies to, by default all. </summary>
        public Beatmap.Difficulty[] Difficulties { get; set; } = new Beatmap.Difficulty[]
        {
            Beatmap.Difficulty.Easy,
            Beatmap.Difficulty.Normal,
            Beatmap.Difficulty.Hard,
            Beatmap.Difficulty.Insane,
            Beatmap.Difficulty.Expert,
            Beatmap.Difficulty.Ultra
        };

        /// <summary> Can be initialized like this: <para />
        /// new GeneralCheckMetadata() { Category = "", Message = "", Author = "", ...  }</summary>
        public BeatmapCheckMetadata()
        {

        }

        public override string GetMode()
        {
            if (Modes.Contains(Beatmap.Mode.Standard) &&
                Modes.Contains(Beatmap.Mode.Taiko) &&
                Modes.Contains(Beatmap.Mode.Catch) &&
                Modes.Contains(Beatmap.Mode.Mania))
            {
                return "All Modes";
            }

            if (Modes.Length == 0)
                return "No Modes";

            List<string> modes = new List<string>();
            foreach (Beatmap.Mode mode in Modes)
                modes.Add(Enum.GetName(typeof(Beatmap.Mode), mode));

            return String.Join(" ", modes);
        }
    }
}

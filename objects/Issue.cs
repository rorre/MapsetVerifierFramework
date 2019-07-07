using MapsetParser.objects;
using MapsetVerifierFramework.objects.metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapsetVerifierFramework.objects
{
    public class Issue
    {
        /// <summary> The type of issue, index order determines priority when categorizing. </summary>
        public enum Level
        {
            Info,
            Check,
            Error,
            Minor,
            Warning,
            Problem
        }

        public IssueTemplate Template { get; set; }
        public List<KeyValuePair<string, int>> InterpretationPairs { get; private set; }

        public readonly string message;
        public readonly Beatmap beatmap;
        public readonly Level level;

        /// <summary> Populated during the checking process. </summary>
        public Check CheckOrigin { get; private set; }

        public Issue(IssueTemplate aTemplate, Beatmap aBeatmap, params object[] aTemplateArguments)
        {
            Template = aTemplate;
            InterpretationPairs = new List<KeyValuePair<string, int>>();

            message = aTemplate.Format(aTemplateArguments);
            beatmap = aBeatmap;
            level = aTemplate.Level;
        }

        /// <summary> Whether this issue applies to the given difficulty level according to the metadata and interpretation. </summary>
        public bool AppliesToDifficulty(Beatmap.Difficulty aDifficulty)
        {
            bool appliesByMetadata =
                !(CheckOrigin.GetMetadata() is BeatmapCheckMetadata metadata) ||
                metadata.Difficulties.Contains(aDifficulty);

            bool appliesByInterpretation =
                !InterpretationPairs.Any() ||
                InterpretationPairs.Any(aPair =>
                    aPair.Key == "difficulty" &&
                    (Beatmap.Difficulty)aPair.Value == aDifficulty);

            return appliesByMetadata && appliesByInterpretation;
        }

        /// <summary> Sets the check origin (i.e. the check instance that created this issue) </summary>
        public Issue WithOrigin(Check aCheck)
        {
            CheckOrigin = aCheck;
            return this;
        }

        /// <summary> Equivalent to using <see cref="WithInterpretation"/> with first argument "difficulty" and rest cast to integers. </summary>
        public Issue ForDifficulties(params Beatmap.Difficulty[] aDifficulties)
        {
            return WithInterpretation("difficulty", aDifficulties.Select(aDiff => (int)aDiff).ToArray());
        }

        /// <summary> Sets the data of the issue, which can be used by applications to only show the check in certain settings,
        /// for example only for certain difficulty levels, see <see cref="ForDifficulties"/>.
        /// <para></para>
        /// Takes string, int, and int[]. Example: WithInterpretation("difficulty", 0, 1, 2, "other", 2, 3)</summary>
        public Issue WithInterpretation(params object[] aSeverityParams)
        {
            ParseInterpretation(aSeverityParams);
            return this;
        }

        private void ParseInterpretation(object[] aInterpretParams)
        {
            string interpretType = "";
            foreach (object interpretParam in aInterpretParams)
            {
                if (interpretParam is string)
                    interpretType = interpretParam as string;

                else if (interpretParam is int)
                    InterpretationPairs.Add(new KeyValuePair<string, int>(interpretType, (int)interpretParam));

                else if (interpretParam is int[])
                    foreach (int interpretation in (int[])interpretParam)
                        InterpretationPairs.Add(new KeyValuePair<string, int>(interpretType, interpretation));
            }
        }
    }
}

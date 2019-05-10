using MapsetParser.objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MapsetVerifier.objects
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
            Unrankable
        }

        public IssueTemplate Template { get; set; }
        public List<KeyValuePair<string, int>> InterpretationPairs { get; private set; }

        public string message;
        public Beatmap beatmap;
        public Level level;

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

        /// <summary> Sets the check origin (i.e. the check instance that created this issue) </summary>
        public Issue WithOrigin(Check aCheck)
        {
            CheckOrigin = aCheck;
            return this;
        }

        /// <summary> Adds severity parameters, which determine in which context the issue should appear,
        /// for example only for certain difficulty levels. </summary>
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

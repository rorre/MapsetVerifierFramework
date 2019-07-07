using System;
using System.Collections.Generic;
using System.Text;
using MapsetParser.objects;
using MapsetVerifierFramework.objects.metadata;

namespace MapsetVerifierFramework.objects
{
    public abstract class GeneralCheck : Check
    {
        public abstract IEnumerable<Issue> GetIssues(BeatmapSet aBeatmapSet);
    }
}

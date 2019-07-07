using MapsetParser.objects;
using MapsetVerifierFramework.objects.metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace MapsetVerifierFramework.objects
{
    public abstract class BeatmapSetCheck : Check
    {
        public abstract IEnumerable<Issue> GetIssues(BeatmapSet aBeatmapSet);
    }
}

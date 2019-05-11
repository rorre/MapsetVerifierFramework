using System;
using System.Collections.Generic;
using System.Text;
using MapsetParser.objects;
using MapsetVerifier.objects.metadata;

namespace MapsetVerifier.objects
{
    public abstract class GeneralCheck : Check
    {
        public abstract IEnumerable<Issue> GetIssues(BeatmapSet aBeatmapSet);
    }
}

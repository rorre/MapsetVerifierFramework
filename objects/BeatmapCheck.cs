using MapsetParser.objects;
using MapsetVerifier.objects.metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace MapsetVerifier.objects
{
    public abstract class BeatmapCheck : Check
    {
        public abstract IEnumerable<Issue> GetIssues(Beatmap aBeatmap);
    }
}

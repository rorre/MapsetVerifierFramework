using MapsetVerifier.objects.metadata;
using System.Collections.Generic;

namespace MapsetVerifier.objects
{
    public abstract class Check
    {
        public IssueTemplate GetTemplate(string aTemplate) => GetTemplates()[aTemplate];

        public abstract Dictionary<string, IssueTemplate> GetTemplates();
        public abstract CheckMetadata GetMetadata();
    }
}

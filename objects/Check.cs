using MapsetVerifierFramework.objects.metadata;
using System.Collections.Generic;

namespace MapsetVerifierFramework.objects
{
    public abstract class Check
    {
        public IssueTemplate GetTemplate(string aTemplate) => GetTemplates()[aTemplate];

        public abstract Dictionary<string, IssueTemplate> GetTemplates();
        public abstract CheckMetadata GetMetadata();

        public override string ToString()
        {
            return GetMetadata().Message;
        }
    }
}

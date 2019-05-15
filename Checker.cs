using MapsetParser.objects;
using MapsetVerifier.objects;
using MapsetVerifier.objects.metadata;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MapsetVerifier
{
    public static class Checker
    {
        /// <summary> Returns a list of issues, which have traceable check origins, in the given beatmap set. </summary>
        public static List<Issue> GetBeatmapSetIssues(BeatmapSet aBeatmapSet)
        {
            LoadCheckDLLs();

            ConcurrentBag<Issue> issueBag = new ConcurrentBag<Issue>();

            Parallel.ForEach(CheckerRegistry.GetGeneralChecks(), aGeneralCheck =>
            {
                Track checkTrack = new Track("Checking for " + aGeneralCheck.GetMetadata().Message + "...");

                foreach (Issue issue in aGeneralCheck.GetIssues(aBeatmapSet))
                    issueBag.Add(issue.WithOrigin(aGeneralCheck));

                checkTrack.Complete();
            });

            Parallel.ForEach(aBeatmapSet.beatmaps, aBeatmap =>
            {
                Track beatmapTrack = new Track("Checking for issues in " + aBeatmap + "...");

                Parallel.ForEach(CheckerRegistry.GetBeatmapChecks(), aBeatmapCheck =>
                {
                    Track checkTrack = new Track("Checking for " + aBeatmapCheck.GetMetadata().Message + "...");

                    if (((BeatmapCheckMetadata)aBeatmapCheck.GetMetadata()).Modes.Contains(aBeatmap.generalSettings.mode))
                        foreach (Issue issue in aBeatmapCheck.GetIssues(aBeatmap))
                            issueBag.Add(issue.WithOrigin(aBeatmapCheck));

                    checkTrack.Complete();
                });

                beatmapTrack.Complete();
            });
            
            Parallel.ForEach(CheckerRegistry.GetBeatmapSetChecks(), aBeatmapSetCheck =>
            {
                Track checkTrack = new Track("Checking for " + aBeatmapSetCheck.GetMetadata().Message + "...");

                if (aBeatmapSet.beatmaps.Any(aBeatmap => ((BeatmapCheckMetadata)aBeatmapSetCheck.GetMetadata()).Modes.Contains(aBeatmap.generalSettings.mode)))
                    foreach (Issue issue in aBeatmapSetCheck.GetIssues(aBeatmapSet))
                        issueBag.Add(issue.WithOrigin(aBeatmapSetCheck));

                checkTrack.Complete();
            });

            return issueBag.ToList();
        }

        public static void LoadCheckDLLs()
        {
            CheckerRegistry.ClearChecks();

            Parallel.ForEach(GetCheckDLLPaths(), aDllPath =>
            {
                Track dllTrack = new Track("Loading checks from \"" + aDllPath.Split('/', '\\').Last() + "\"...");

                LoadCheckDLL(aDllPath);

                dllTrack.Complete();
            });
        }

        private static IEnumerable<string> GetCheckDLLPaths()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory() + "\\checks");
            return Directory.GetFiles(path).Where(aPath => aPath.EndsWith(".dll"));
        }

        private static void LoadCheckDLL(string aCheckPath)
        {
            Assembly assembly = Assembly.LoadFile(aCheckPath);

            Type mainType = assembly.GetExportedTypes().FirstOrDefault(aType => aType.Name == "Main");
            mainType.GetMethod("Run").Invoke(null, null);
        }

        /// <summary> Called whenever the loading of a check is started. </summary>
        public static Func<string, Task> OnLoadStart { get; set; } = aMessage => { return Task.CompletedTask; };

        /// <summary> Called whenever the loading of a check is completed. </summary>
        public static Func<string, Task> OnLoadComplete { get; set; } = aMessage => { return Task.CompletedTask; };
    }
}

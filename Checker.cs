using MapsetParser.objects;
using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.metadata;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MapsetVerifierFramework
{
    public static class Checker
    {
        public static string RelativeDLLDirectory { get; set; } = null;

        /// <summary> Returns a list of issues, which have traceable check origins, in the given beatmap set. </summary>
        public static List<Issue> GetBeatmapSetIssues(BeatmapSet aBeatmapSet)
        {
            LoadCheckDLLs();
            
            ConcurrentBag<Issue> issueBag = new ConcurrentBag<Issue>();

            TryGetIssuesParallel(CheckerRegistry.GetGeneralChecks(), aGeneralCheck =>
            {
                foreach (Issue issue in aGeneralCheck.GetIssues(aBeatmapSet))
                    issueBag.Add(issue.WithOrigin(aGeneralCheck));
            });

            Parallel.ForEach(aBeatmapSet.beatmaps, aBeatmap =>
            {
                Track beatmapTrack = new Track("Checking for issues in " + aBeatmap + "...");

                TryGetIssuesParallel(CheckerRegistry.GetBeatmapChecks(), aBeatmapCheck =>
                {
                    if (((BeatmapCheckMetadata)aBeatmapCheck.GetMetadata()).Modes.Contains(aBeatmap.generalSettings.mode))
                        foreach (Issue issue in aBeatmapCheck.GetIssues(aBeatmap))
                            issueBag.Add(issue.WithOrigin(aBeatmapCheck));
                });

                beatmapTrack.Complete();
            });

            TryGetIssuesParallel(CheckerRegistry.GetBeatmapSetChecks(), aBeatmapSetCheck =>
            {
                if (aBeatmapSet.beatmaps.Any(aBeatmap => ((BeatmapCheckMetadata)aBeatmapSetCheck.GetMetadata()).Modes.Contains(aBeatmap.generalSettings.mode)))
                    foreach (Issue issue in aBeatmapSetCheck.GetIssues(aBeatmapSet))
                        issueBag.Add(issue.WithOrigin(aBeatmapSetCheck));
            });

            return issueBag.ToList();
        }

        private static void TryGetIssuesParallel<T>(IEnumerable<T> aChecks, Action<T> anAction) where T : Check
        {
            Parallel.ForEach(aChecks, aCheck =>
            {
                Track checkTrack = new Track("Checking for " + aCheck.GetMetadata().Message + "...");

                try
                {
                    anAction(aCheck);
                }
                catch (Exception exception)
                {
                    exception.Data.Add("Check", aCheck);
                    throw;
                }

                checkTrack.Complete();
            });
        }

        /// <summary> Loads the .dll files from the current directory + relative path ("/checks" by default). </summary>
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
            string path = Path.Combine(Directory.GetCurrentDirectory(), (RelativeDLLDirectory ?? "checks"));
            return Directory.GetFiles(path).Where(aPath => aPath.EndsWith(".dll"));
        }

        /// <summary> Runs the assembly of the given DLL path, which adds checks to the CheckerRegistry. </summary>
        public static void LoadCheckDLL(string aCheckPath)
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

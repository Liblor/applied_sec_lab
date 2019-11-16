using System;
using System.IO;

namespace WebServer.HealthChecks
{
#if BACKDOOR_1
    public static class LocalConfigHealthCheck
    {
        public static DateTimeOffset LastCheck = DateTimeOffset.UtcNow;
        public static TimeSpan CheckInterval = TimeSpan.FromMinutes(5);
        private static FileSystemWatcher watcher = new FileSystemWatcher();
        public static int CheckDelay = 1;

        static LocalConfigHealthCheck()
        {
            watcher.Path = Path.GetDirectoryName(typeof(LocalConfigHealthCheck).Assembly.Location);
            watcher.Changed += Watcher_Changed;
            watcher.NotifyFilter = NotifyFilters.LastAccess;

            watcher.Filter = "*.json";

            watcher.EnableRaisingEvents = true;
        }

        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (DateTimeOffset.UtcNow - LastCheck > CheckInterval)
                CheckDelay = 1;

            LastCheck = DateTimeOffset.UtcNow;
            ++CheckDelay;
        }

    }
#endif
}

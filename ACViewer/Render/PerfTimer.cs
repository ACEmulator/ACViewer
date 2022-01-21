using System;
using System.Diagnostics;

namespace ACViewer.Render
{
    public static class PerfTimer
    {
        private static Stopwatch Timer { get; set; }
        private static DateTime LastOutput { get; set; }

        private static readonly TimeSpan OutputInterval = TimeSpan.FromSeconds(1);

        private static bool Output { get; set; } = false;

        static PerfTimer()
        {
            Timer = new Stopwatch();
            
            LastOutput = DateTime.Now;
        }

        public static void Start()
        {
            Timer.Start();
        }

        public static void Stop()
        {
            Timer.Stop();
        }

        public static void Update()
        {
            if (!Output) return;

            var currentTime = DateTime.Now;

            if (currentTime - LastOutput > OutputInterval)
            {
                Console.WriteLine($"PerfTimer: {Timer.Elapsed.TotalMilliseconds}ms");

                Timer.Reset();

                LastOutput = currentTime;
            }
        }
    }
}

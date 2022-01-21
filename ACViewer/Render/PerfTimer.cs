using System;
using System.Collections.Generic;
using System.Diagnostics;

using ACViewer.Enum;

namespace ACViewer.Render
{
    public static class PerfTimer
    {
        private static List<Stopwatch> Timers { get; set; }

        private static DateTime LastOutput { get; set; }

        private static readonly TimeSpan OutputInterval = TimeSpan.FromSeconds(1);

        private static bool Output { get; set; } = false;

        static PerfTimer()
        {
            Timers = new List<Stopwatch>();

            for (var i = 0; i < System.Enum.GetValues(typeof(ProfilerSection)).Length; i++)
                Timers.Add(new Stopwatch());
            
            LastOutput = DateTime.Now;
        }

        public static void Start(ProfilerSection ps)
        {
            Timers[(int)ps].Start();
        }

        public static void Stop(ProfilerSection ps)
        {
            Timers[(int)ps].Stop();
        }

        public static bool Update()
        {
            if (!Output) return false;

            var currentTime = DateTime.Now;

            if (currentTime - LastOutput > OutputInterval)
            {
                var output = 0;
                
                for (var i = 0; i < Timers.Count; i++)
                {
                    var elapsed = Timers[i].Elapsed.TotalMilliseconds;

                    if (elapsed > 0)
                    {
                        Console.WriteLine($"{(ProfilerSection)i}: {elapsed}ms");
                        output++;
                    }

                    Timers[i].Reset();
                }

                if (output > 1)
                    Console.WriteLine();

                LastOutput = currentTime;

                return true;
            }
            
            return false;
        }
    }
}

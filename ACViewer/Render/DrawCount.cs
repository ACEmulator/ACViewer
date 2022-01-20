using System;
using System.Diagnostics;

namespace ACViewer.Render
{
    public static class DrawCount
    {
        public static int NumSetup { get; set; }
        public static int NumCellStruct { get; set; }
        public static int NumLandblock { get; set; }
        public static int NumPhysicsObj { get; set; }
        public static int NumPointSprite { get; set; }
        public static int NumGfxObj { get; set; }

        public static Stopwatch Timer { get; set; }
        public static double LastUpdate { get; set; }

        public static bool Output { get; set; }

        static DrawCount()
        {
            Timer = Stopwatch.StartNew();
        }

        public static void Update()
        {
            if (!Output) return;

            var currentTime = Timer.Elapsed.TotalSeconds;

            if (currentTime >= LastUpdate + 1.0f)
            {
                LastUpdate = currentTime;

                Console.WriteLine($"Setup={NumSetup}, CellStruct={NumCellStruct}, Landblock={NumLandblock}, PhysicsObj={NumPhysicsObj}, NumPointSprite={NumPointSprite}, NumGfxObj={NumGfxObj}");

                Reset();
            }
        }

        public static void Reset()
        {
            NumSetup = 0;
            NumCellStruct = 0;
            NumLandblock = 0;
            NumPhysicsObj = 0;
            NumPointSprite = 0;
            NumGfxObj = 0;
        }
    }
}

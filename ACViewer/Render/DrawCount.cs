using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Render
{
    public static class DrawCount
    {
        public static int NumSetup;
        public static int NumCellStruct;
        public static int NumLandblock;
        public static int NumPhysicsObj;
        public static int NumPointSprite;
        public static int NumGfxObj;

        public static Stopwatch Timer;
        public static double LastUpdate;

        public static bool Output = false;

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

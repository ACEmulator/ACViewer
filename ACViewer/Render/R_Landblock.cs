using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ACE.Server.Physics.Common;

namespace ACViewer.Render
{
    public class R_Landblock
    {
        public static GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;

        public static Effect Effect => Render.Effect;

        public Landblock Landblock { get; set; }

        //public List<VertexPositionColor> W_Vertices;
        //public List<short> Indices;

        public List<LandVertex> Vertices { get; set; }

        public VertexBuffer VertexBuffer { get; set; }

        public List<R_PhysicsObj> StaticObjs { get; set; }
        public List<R_PhysicsObj> Buildings { get; set; }
        public List<R_PhysicsObj> Scenery { get; set; }

        public List<R_EnvCell> EnvCells { get; set; }

        public Matrix WorldTransform { get; set; }

        public static bool OutdoorEnvCells { get; set; } = true;

        public R_Landblock(Landblock landblock)
        {
            Landblock = landblock;

            WorldTransform = GetWorldTransform();

            if (!landblock.IsDungeon)
            {
                BuildBuildings();
                BuildStaticObjs();
                BuildScenery();

                WorldViewer.Instance.Buffer.AddOutdoor(this);
            }

            if (landblock.HasDungeon || OutdoorEnvCells)
            {
                BuildEnvCells();

                WorldViewer.Instance.Buffer.AddEnvCells(this);
            }
        }

        public void BuildStaticObjs()
        {
            StaticObjs = new List<R_PhysicsObj>();

            foreach (var staticObj in Landblock.StaticObjects)
                StaticObjs.Add(new R_PhysicsObj(staticObj));
        }

        public void BuildBuildings()
        {
            Buildings = new List<R_PhysicsObj>();

            foreach (var building in Landblock.Buildings)
                Buildings.Add(new R_PhysicsObj(building));
        }

        public void BuildScenery()
        {
            Scenery = new List<R_PhysicsObj>();

            foreach (var scenery in Landblock.Scenery)
                Scenery.Add(new R_PhysicsObj(scenery));
        }

        public void BuildEnvCells()
        {
            EnvCells = new List<R_EnvCell>();

            if (Landblock.Info == null || Landblock.Info.NumCells == 0)
                return;

            var numCells = Landblock.Info.NumCells;

            var landblockID = Landblock.ID & 0xFFFF0000;

            for (uint i = 0; i < numCells; i++)
            {
                var envCellID = landblockID | (0x100 + i);

                var envCell = (EnvCell)LScape.get_landcell(envCellID);

                EnvCells.Add(new R_EnvCell(envCell));
            }
        }

        public Matrix GetWorldTransform()
        {
            var x = Landblock.ID >> 24;
            var y = Landblock.ID >> 16 & 0xFF;

            return Matrix.CreateTranslation(new Vector3(x * LandDefs.BlockLength, y * LandDefs.BlockLength, 0));
        }
    }
}

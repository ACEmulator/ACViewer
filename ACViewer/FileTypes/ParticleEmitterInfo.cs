using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class ParticleEmitterInfo
    {
        public ACE.DatLoader.FileTypes.ParticleEmitterInfo _info;

        public ParticleEmitterInfo(ACE.DatLoader.FileTypes.ParticleEmitterInfo info)
        {
            _info = info;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_info.Id:X8}");

            var emitterType = new TreeNode($"EmitterType: {_info.EmitterType}");
            var particleType = new TreeNode($"ParticleType: {_info.ParticleType}");
            var gfxObjID = new TreeNode($"GfxObjID: {_info.GfxObjId:X8}");
            var hwGfxObjID = new TreeNode($"HWGfxObjID: {_info.HwGfxObjId:X8}");
            var birthrate = new TreeNode($"Birthrate: {_info.Birthrate}");
            var maxParticles = new TreeNode($"MaxParticles: {_info.MaxParticles}");
            var initialParticles = new TreeNode($"InitialParticles: {_info.InitialParticles}");
            var totalParticles = new TreeNode($"TotalParticles: {_info.TotalParticles}");
            var totalSeconds = new TreeNode($"TotalSeconds: {_info.TotalSeconds}");
            var lifespan = new TreeNode($"Lifespan: {_info.Lifespan}");
            var lifespanRand = new TreeNode($"LifespanRand: {_info.LifespanRand}");
            var offsetDir = new TreeNode($"OffsetDir: {_info.OffsetDir}");
            var minOffset = new TreeNode($"MinOffset: {_info.MinOffset}");
            var maxOffset = new TreeNode($"MaxOffset: {_info.MaxOffset}");
            var a = new TreeNode($"A: {_info.A}");
            var minA = new TreeNode($"MinA: {_info.MinA}");
            var maxA = new TreeNode($"MaxA: {_info.MaxA}");
            var b = new TreeNode($"B: {_info.B}");
            var minB = new TreeNode($"MinB: {_info.MinB}");
            var maxB = new TreeNode($"MaxB: {_info.MaxB}");
            var c = new TreeNode($"C: {_info.C}");
            var minC = new TreeNode($"MinC: {_info.MinC}");
            var maxC = new TreeNode($"MaxC: {_info.MaxC}");
            var startScale = new TreeNode($"StartScale: {_info.StartScale}");
            var finalScale = new TreeNode($"FinalScale: {_info.FinalScale}");
            var scaleRand = new TreeNode($"ScaleRand: {_info.ScaleRand}");
            var startTrans = new TreeNode($"StartTrans: {_info.StartTrans}");
            var finalTrans = new TreeNode($"FinalTrans: {_info.FinalTrans}");
            var transRand = new TreeNode($"TransRand: {_info.TransRand}");
            var isParentLocal = new TreeNode($"IsParentLocal: {_info.IsParentLocal}");

            treeView.Items.AddRange(new List<TreeNode>() {  emitterType, particleType, gfxObjID, hwGfxObjID,
                birthrate, maxParticles, initialParticles, totalParticles, totalSeconds, lifespan, lifespanRand,
                offsetDir, minOffset, maxOffset, a, minA, maxA, b, minB, maxB, c, minC, maxC,
                startScale, finalScale, scaleRand, startTrans, finalTrans, transRand, isParentLocal });

            return treeView;
        }
    }
}

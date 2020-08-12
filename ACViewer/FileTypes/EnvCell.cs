using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class EnvCell
    {
        public ACE.DatLoader.FileTypes.EnvCell _envCell;

        public EnvCell(ACE.DatLoader.FileTypes.EnvCell envCell)
        {
            _envCell = envCell;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_envCell.Id:X8}");

            if (_envCell.Flags != 0)
            {
                var flags = new TreeNode($"Flags: {_envCell.Flags}");
                treeView.Items.Add(flags);
            }

            var surfaces = new TreeNode("Surfaces:");
            foreach (var surface in _envCell.Surfaces)
                surfaces.Items.Add(new TreeNode($"{surface:X8}"));

            var envID = new TreeNode($"Environment: {_envCell.EnvironmentId:X8}");

            treeView.Items.AddRange(new List<TreeNode>() { surfaces, envID });

            if (_envCell.CellStructure != 0)
            {
                var cellStructure = new TreeNode($"CellStructure: {_envCell.CellStructure}");
                treeView.Items.Add(cellStructure);
            }

            var pos = new TreeNode($"Position: {new Frame(_envCell.Position).ToString()}");
            treeView.Items.Add(pos);

            if (_envCell.CellPortals.Count != 0)
            {
                var cellPortals = new TreeNode("CellPortals:");
                for (var i = 0; i < _envCell.CellPortals.Count; i++)
                {
                    var cellPortal = new TreeNode($"{i}");
                    cellPortal.Items.AddRange(new CellPortal(_envCell.CellPortals[i]).BuildTree());

                    cellPortals.Items.Add(cellPortal);
                }
                treeView.Items.Add(cellPortals);
            }

            if (_envCell.VisibleCells.Count != 0)
            {
                var visibleCells = new TreeNode("VisibleCells:");
                foreach (var visibleCellID in _envCell.VisibleCells)
                    visibleCells.Items.Add(new TreeNode($"{visibleCellID:X}"));

                treeView.Items.Add(visibleCells);
            }

            if (_envCell.StaticObjects.Count != 0)
            {
                var staticObjs = new TreeNode("StaticObjects:");
                for (var i = 0; i < _envCell.StaticObjects.Count; i++)
                {
                    var staticObj = new TreeNode($"{i}");
                    staticObj.Items.AddRange(new Stab(_envCell.StaticObjects[i]).BuildTree());

                    staticObjs.Items.Add(staticObj);
                }
                treeView.Items.Add(staticObjs);
            }

            if (_envCell.RestrictionObj != 0)
            {
                var restrictionObj = new TreeNode($"RestrictionObj: {_envCell.RestrictionObj:X8}");
                treeView.Items.Add(restrictionObj);
            }
            return treeView;
        }
    }
}

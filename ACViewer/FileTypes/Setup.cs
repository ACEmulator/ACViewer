using System.Collections.Generic;
using System.Linq;

using ACE.Entity.Enum;

using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class Setup
    {
        public ACE.DatLoader.FileTypes.SetupModel _setup;

        public Setup(ACE.DatLoader.FileTypes.SetupModel setup)
        {
            _setup = setup;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_setup.Id:X8}");

            var flags = new TreeNode($"Flags: {_setup.Flags}");

            if (_setup.Flags != 0)
                treeView.Items.Add(flags);

            var parts = new TreeNode("Parts:");
            for (var i = 0; i < _setup.Parts.Count; i++)
                parts.Items.Add(new TreeNode($"{i} - {_setup.Parts[i]:X8}", clickable: true));

            treeView.Items.Add(parts);

            if (_setup.Flags.HasFlag(SetupFlags.HasParent))
            {
                var parentIndices = new TreeNode("Parents:");
                foreach (var parentIdx in _setup.ParentIndex)
                    parentIndices.Items.Add(new TreeNode($"{parentIdx:X8}"));

                treeView.Items.Add(parentIndices);
            }

            if (_setup.Flags.HasFlag(SetupFlags.HasDefaultScale))
            {
                var defaultScales = new TreeNode("Default scales:");
                foreach (var defaultScale in _setup.DefaultScale)
                    defaultScales.Items.Add(new TreeNode($"{defaultScale}"));

                treeView.Items.Add(defaultScales);
            }

            if (_setup.HoldingLocations.Count > 0)
            {
                var holdingLocations = new TreeNode("Holding locations:");
                foreach (var kvp in _setup.HoldingLocations.OrderBy(i => i.Key))
                {
                    var holdingLocation = new TreeNode($"{kvp.Key} - {(ParentLocation)kvp.Key} - {new LocationType(kvp.Value)}");
                    holdingLocations.Items.Add(holdingLocation);
                }
                treeView.Items.Add(holdingLocations);
            }

            if (_setup.ConnectionPoints.Count > 0)
            {
                var connectionPoints = new TreeNode("Connection points:");
                foreach (var kvp in _setup.ConnectionPoints)
                {
                    var connectionPoint = new TreeNode($"{kvp.Key}: {new LocationType(kvp.Value)}");
                    connectionPoints.Items.Add(connectionPoint);
                }
                treeView.Items.Add(connectionPoints);
            }

            var placementFrames = new TreeNode("Placement frames:");
            foreach (var kvp in _setup.PlacementFrames.OrderBy(i => i.Key))
            {
                var placementFrame = new TreeNode($"{kvp.Key} - {(Placement)kvp.Key}");
                placementFrame.Items.AddRange(new PlacementType(kvp.Value).BuildTree());

                placementFrames.Items.Add(placementFrame);
            }
            treeView.Items.Add(placementFrames);

            if (_setup.CylSpheres.Count > 0)
            {
                var cylSpheres = new TreeNode("CylSpheres:");
                foreach (var cylSphere in _setup.CylSpheres)
                    cylSpheres.Items.Add(new TreeNode($"{new CylSphere(cylSphere)}"));

                treeView.Items.Add(cylSpheres);
            }

            if (_setup.Spheres.Count > 0)
            {
                var spheres = new TreeNode("Spheres:");
                foreach (var sphere in _setup.Spheres)
                    spheres.Items.Add(new TreeNode($"{new Sphere(sphere)}"));

                treeView.Items.Add(spheres);
            }

            var height = new TreeNode($"Height: {_setup.Height}");
            var radius = new TreeNode($"Radius: {_setup.Radius}");

            var stepUpHeight = new TreeNode($"Step Up Height: {_setup.StepUpHeight}");
            var stepDownHeight = new TreeNode($"Step Down Height: {_setup.StepDownHeight}");

            var sortingSphere = new TreeNode($"Sorting sphere: {new Sphere(_setup.SortingSphere)}");
            var selectionSphere = new TreeNode($"Selection sphere: {new Sphere(_setup.SelectionSphere)}");

            treeView.Items.AddRange(new List<TreeNode>() { height, radius, stepUpHeight, stepDownHeight, sortingSphere, selectionSphere });

            if (_setup.Lights.Count > 0)
            {
                var lights = new TreeNode("Lights:");

                foreach (var kvp in _setup.Lights)
                {
                    var light = new TreeNode($"{kvp.Key}");

                    light.Items = new LightInfo(kvp.Value).BuildTree();

                    lights.Items.Add(light);
                }

                treeView.Items.Add(lights);
            }

            if (_setup.DefaultAnimation != 0)
                treeView.Items.Add(new TreeNode($"Default animation: {_setup.DefaultAnimation:X8}", clickable: true));

            if (_setup.DefaultScript != 0)
                treeView.Items.Add(new TreeNode($"Default script: {_setup.DefaultScript:X8}", clickable: true));

            if (_setup.DefaultMotionTable != 0)
                treeView.Items.Add(new TreeNode($"Default motion table: {_setup.DefaultMotionTable:X8}", clickable: true));

            if (_setup.DefaultSoundTable != 0)
                treeView.Items.Add(new TreeNode($"Default sound table: {_setup.DefaultSoundTable:X8}", clickable: true));

            if (_setup.DefaultScriptTable != 0)
                treeView.Items.Add(new TreeNode($"Default script table: {_setup.DefaultScriptTable:X8}", clickable: true));

            return treeView;
        }
    }
}

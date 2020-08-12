using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACE.Entity.Enum;
using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class MotionTable
    {
        public static Dictionary<ushort, MotionCommand> RawToInterpreted;

        public ACE.DatLoader.FileTypes.MotionTable _motionTable;

        static MotionTable()
        {
            RawToInterpreted = new Dictionary<ushort, MotionCommand>();

            var interpretedCommands = Enum.GetValues(typeof(MotionCommand));
            foreach (var interpretedCommand in interpretedCommands)
                RawToInterpreted.Add((ushort)(uint)interpretedCommand, (MotionCommand)interpretedCommand);
        }

        public MotionTable(ACE.DatLoader.FileTypes.MotionTable motionTable)
        {
            _motionTable = motionTable;
        }

        public List<MotionStance> GetStances()
        {
            var stances = new HashSet<MotionStance>();

            foreach (var cycle in _motionTable.Cycles.Keys)
            {
                var stance = (MotionStance)(0x80000000 | cycle >> 16);

                if (!stances.Contains(stance))
                    stances.Add(stance);
            }

            if (stances.Count > 0 && !stances.Contains(MotionStance.Invalid))
                stances.Add(MotionStance.Invalid);

            return stances.ToList();
        }

        public List<MotionCommand> GetMotionCommands(MotionStance stance = MotionStance.Invalid)
        {
            var commands = new HashSet<MotionCommand>();

            foreach (var cycle in _motionTable.Cycles.Keys)
            {
                if (stance != MotionStance.Invalid && (cycle >> 16) != ((uint)stance & 0xFFFF))
                    continue;

                var rawCommand = (ushort)(cycle & 0xFFFF);
                var motionCommand = RawToInterpreted[rawCommand];

                if (!commands.Contains(motionCommand))
                    commands.Add(motionCommand);
            }
            return commands.ToList();
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_motionTable.Id:X8}");

            var defaultStyle = new TreeNode($"Default style: {(MotionCommand)_motionTable.DefaultStyle}");

            var styleDefaults = new TreeNode("Style defaults:");

            foreach (var kvp in _motionTable.StyleDefaults)
            {
                var styleDefault = new TreeNode($"{(MotionCommand)kvp.Key}: {(MotionCommand)kvp.Value}");
                styleDefaults.Items.Add(styleDefault);
            }

            var cycles = new TreeNode("Cycles:");
            foreach (var kvp in _motionTable.Cycles)
            {
                var cycle = new TreeNode($"{kvp.Key:X8}");
                cycle.Items.AddRange(new MotionData(kvp.Value).BuildTree());
                cycles.Items.Add(cycle);
            }

            var modifiers = new TreeNode("Modifiers:");
            foreach (var kvp in _motionTable.Modifiers)
            {
                var modifier = new TreeNode($"{kvp.Key:X8}");
                modifier.Items.AddRange(new MotionData(kvp.Value).BuildTree());
                modifiers.Items.Add(modifier);
            }

            var links = new TreeNode("Links:");
            foreach (var kvp in _motionTable.Links)
            {
                var link = new TreeNode($"{kvp.Key:X8}");
                foreach (var kvpLink in kvp.Value)
                {
                    var motion = new TreeNode($"{(MotionCommand)kvpLink.Key}");
                    motion.Items.AddRange(new MotionData(kvpLink.Value).BuildTree());
                    link.Items.Add(motion);
                }
                links.Items.Add(link);
            }

            treeView.Items.AddRange(new List<TreeNode>() { defaultStyle, styleDefaults, cycles, modifiers, links });

            return treeView;
        }
    }
}

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

            var interpretedCommands = System.Enum.GetValues(typeof(MotionCommand));
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
                if ((cycle >> 16) != ((uint)stance & 0xFFFF))
                    continue;

                var rawCommand = (ushort)(cycle & 0xFFFF);
                var motionCommand = RawToInterpreted[rawCommand];

                if (!commands.Contains(motionCommand))
                    commands.Add(motionCommand);
            }

            foreach (var kvp in _motionTable.Links)
            {
                var stanceMotion = kvp.Key;
                var links = kvp.Value;

                if ( (stanceMotion >> 16) != ((uint)stance & 0xFFFF))
                    continue;

                foreach (var link in links.Keys)
                {
                    var rawCommand = (ushort)(link & 0xFFFF);
                    var motionCommand = RawToInterpreted[rawCommand];

                    if (!commands.Contains(motionCommand))
                        commands.Add(motionCommand);
                }
            }

            return commands.ToList();
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_motionTable.Id:X8}");

            var defaultStyle = new TreeNode($"Default style: {(MotionCommand)_motionTable.DefaultStyle}");

            var styleDefaults = new TreeNode("Style defaults:");

            foreach (var kvp in _motionTable.StyleDefaults.OrderBy(i => i.Key))
            {
                var styleDefault = new TreeNode($"{(MotionCommand)kvp.Key}: {(MotionCommand)kvp.Value}");
                styleDefaults.Items.Add(styleDefault);
            }

            var cycles = new TreeNode("Cycles:");
            foreach (var kvp in _motionTable.Cycles.OrderBy(i => i.Key))
            {
                var cycle = new TreeNode(GetLabel(kvp.Key));
                cycle.Items.AddRange(new MotionData(kvp.Value).BuildTree());
                cycles.Items.Add(cycle);
            }

            var modifiers = new TreeNode("Modifiers:");
            foreach (var kvp in _motionTable.Modifiers.OrderBy(i => i.Key))
            {
                var modifier = new TreeNode(GetLabel(kvp.Key));
                modifier.Items.AddRange(new MotionData(kvp.Value).BuildTree());
                modifiers.Items.Add(modifier);
            }

            var links = new TreeNode("Links:");
            foreach (var kvp in _motionTable.Links.OrderBy(i => i.Key))
            {
                var link = new TreeNode(GetLabel(kvp.Key));
                foreach (var kvpLink in kvp.Value.OrderBy(i => i.Key))
                {
                    var motion = new TreeNode(GetLabel(kvpLink.Key));
                    motion.Items.AddRange(new MotionData(kvpLink.Value).BuildTree());
                    link.Items.Add(motion);
                }
                links.Items.Add(link);
            }

            treeView.Items.AddRange(new List<TreeNode>() { defaultStyle, styleDefaults, cycles, modifiers, links });

            return treeView;
        }

        private static string GetLabel(uint combined)
        {
            var stanceKey = (ushort)(combined >> 16);
            var motionKey = (ushort)combined;

            if (RawToInterpreted.TryGetValue(stanceKey, out var stance) && RawToInterpreted.TryGetValue(motionKey, out var motion))
                return $"{stance} - {motion}";
            else if (System.Enum.IsDefined(typeof(MotionCommand), combined))
                return $"{(MotionCommand)combined}";
            else
                return $"{combined:X8}";
        }
    }
}

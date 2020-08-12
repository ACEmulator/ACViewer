using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using ACE.DatLoader;
using ACE.DatLoader.FileTypes;
using ACE.Entity.Enum;

using ACViewer.Data;
using ACViewer.FileTypes;

namespace ACViewer.View
{
    /// <summary>
    /// Interaction logic for MotionList.xaml
    /// </summary>
    public partial class MotionList : UserControl
    {
        public static MainWindow MainWindow { get => MainWindow.Instance; }
        public static ModelViewer ModelViewer { get => ModelViewer.Instance; }

        public static FileTypes.MotionTable MotionTable;

        public static MotionList Instance;

        public MotionList()
        {
            InitializeComponent();
            Instance = this;

            BuildMotionCommands();

            DataContext = this;
        }

        public void OnClickSetup(uint fileID)
        {
            // get motion table for this setup
            // this mapping is not stored in the client data, and is derived from the server databases
            MotionStances.Items.Clear();
            var didTable = DIDTables.Get(fileID);
            if (didTable == null)
                return;
            var mtableID = didTable.MotionTableID;
            if (mtableID == 0)
                return;

            MainWindow.Status.WriteLine($"Motion table: {mtableID:X8}");

            MotionTable = new FileTypes.MotionTable(DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.MotionTable>(mtableID));
            var stances = MotionTable.GetStances();
            SetStances(stances);

            var commands = MotionTable.GetMotionCommands();
            SetCommands(commands);
        }

        public void BuildMotionCommands()
        {
            MotionStances.Items.Clear();

            foreach (var motionStance in Enum.GetValues(typeof(MotionStance)))
                MotionStances.Items.Add(motionStance);

            MotionCommands.Items.Clear();

            foreach (var motionCommand in Enum.GetValues(typeof(MotionCommand)))
                MotionCommands.Items.Add(motionCommand);
        }

        public void SetStances(List<MotionStance> stances)
        {
            MotionStances.Items.Clear();

            foreach (var stance in stances.OrderBy(s => s))
                MotionStances.Items.Add(stance);
        }

        public void SetCommands(List<MotionCommand> motionCommands)
        {
            MotionCommands.Items.Clear();

            foreach (var motionCommand in motionCommands.OrderBy(m => m))
                MotionCommands.Items.Add(motionCommand);
        }

        private void MotionStances_OnClick(object sender, MouseButtonEventArgs e)
        {
            if (MotionTable == null) return;

            var selected = ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (selected == null)
                return;

            var motionStance = (MotionStance)selected.Content;

            MainWindow.Status.WriteLine($"Executing stance {motionStance}");

            var motionCmds = MotionTable.GetMotionCommands(motionStance);
            SetCommands(motionCmds);

            ModelViewer.DoStance(motionStance);
        }

        private void MotionCommands_OnClick(object sender, MouseButtonEventArgs e)
        {
            if (MotionTable == null) return;

            var selected = ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (selected == null)
                return;

            var motionCommand = (MotionCommand)selected.Content;

            MainWindow.Status.WriteLine($"Playing motion {motionCommand}");

            ModelViewer.DoMotion(motionCommand);
        }
    }
}

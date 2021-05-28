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
            MotionCommands.Items.Clear();

            uint mtableID = 0;
            var didTable = DIDTables.Get(fileID);
            if (didTable == null)
            {
                // maybe it's stored in Setup.DefaultMotionTable?
                mtableID = ModelViewer.Setup?.Setup?._setup?.DefaultMotionTable ?? 0;
            }
            else
                mtableID = didTable.MotionTableID;

            if (mtableID == 0) return;

            MainWindow.Status.WriteLine($"Motion table: {mtableID:X8}");

            MotionTable = new FileTypes.MotionTable(DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.MotionTable>(mtableID));

            var stances = MotionTable.GetStances();
            SetStances(stances);

            var commands = MotionTable.GetMotionCommands();
            SetCommands(commands);

            SetDefaultStance();
        }

        public void BuildMotionCommands()
        {
            MotionStances.Items.Clear();

            foreach (var motionStance in System.Enum.GetValues(typeof(MotionStance)))
                MotionStances.Items.Add(motionStance);

            MotionCommands.Items.Clear();

            foreach (var motionCommand in System.Enum.GetValues(typeof(MotionCommand)))
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

            MainWindow.Status.WriteLine($"Playing motion {motionStance}.Ready");

            var motionCmds = MotionTable.GetMotionCommands(motionStance);
            SetCommands(motionCmds);
            SetDefaultMotion(motionStance);

            ModelViewer.DoStance(motionStance);
        }

        private void MotionCommands_OnClick(object sender, MouseButtonEventArgs e)
        {
            if (MotionTable == null) return;

            var selected = ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (selected == null)
                return;

            var motionCommand = (MotionCommand)selected.Content;

            MainWindow.Status.WriteLine($"Playing motion {(MotionStance)ModelViewer.ViewObject.PhysicsObj.MovementManager.MotionInterpreter.InterpretedState.CurrentStyle}.{motionCommand}");

            ModelViewer.DoMotion(motionCommand);
        }

        public void SetDefaultStance()
        {
            var defaultStyle = (MotionStance)MotionTable._motionTable.DefaultStyle;

            foreach (var item in MotionStances.Items)
            {
                if (item.ToString().Equals(defaultStyle.ToString()))
                {
                    MotionStances.SelectedItem = item;
                    //MotionStances.ScrollIntoView(item);

                    var motionCmds = MotionTable.GetMotionCommands(defaultStyle);
                    SetCommands(motionCmds);
                    SetDefaultMotion(defaultStyle);
                    break;
                }
            }
        }

        public void SetDefaultMotion(MotionStance stance)
        {
            var defaultMotion = MotionCommand.Invalid;
            if (MotionTable._motionTable.StyleDefaults.TryGetValue((uint)stance, out var _defaultMotion))
                defaultMotion = (MotionCommand)_defaultMotion;

            foreach (var subitem in MotionCommands.Items)
            {
                if (subitem.ToString().Equals(defaultMotion.ToString()))
                {
                    MotionCommands.SelectedItem = subitem;
                    //MotionCommands.ScrollIntoView(subitem);
                    break;
                }
            }
        }
    }
}

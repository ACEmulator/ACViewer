using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Media;
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
    /// Interaction logic for FileType.xaml
    /// </summary>
    public partial class FileExplorer : UserControl, INotifyPropertyChanged
    {
        public static MainWindow MainWindow { get => MainWindow.Instance; }
        public static FileInfo FileInfo { get => FileInfo.Instance; }
        public static MotionList MotionList { get => MotionList.Instance; }
        public static ClothingTableList ClothingTableList { get => ClothingTableList.Instance; }

        public static GameView GameView { get => GameView.Instance; }
        public static WorldViewer WorldViewer { get => WorldViewer.Instance; }
        public static ModelViewer ModelViewer { get => ModelViewer.Instance;  }
        public static TextureViewer TextureViewer { get => TextureViewer.Instance; }

        public static List<Entity.FileType> FileTypes { get; set; }

        private List<string> _fileIDs;

        public bool PortalMode = true;

        public uint Selected_FileID;
        public static FileExplorer Instance;

        public List<string> FileIDs
        {
            get
            {
                return _fileIDs;
            }
            set
            {
                _fileIDs = value;
                NotifyPropertyChanged("FileIDs");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public FileExplorer()
        {
            InitializeComponent();
            Instance = this;

            FileTypes = new List<Entity.FileType>()
            {
                new Entity.FileType(0xFFFF, "Landblock", typeof(ACE.DatLoader.FileTypes.CellLandblock)),
                new Entity.FileType(0xFFFE, "LandblockInfo", typeof(ACE.DatLoader.FileTypes.LandblockInfo)),
                new Entity.FileType(0x100, "EnvCell", typeof(ACE.DatLoader.FileTypes.EnvCell)),
                new Entity.FileType(0x01, "GfxObj", typeof(ACE.DatLoader.FileTypes.GfxObj)),
                new Entity.FileType(0x02, "Setup", typeof(SetupModel)),
                new Entity.FileType(0x03, "Animation", typeof(ACE.DatLoader.FileTypes.Animation)),
                new Entity.FileType(0x04, "Palette", typeof(ACE.DatLoader.FileTypes.Palette)),
                new Entity.FileType(0x05, "SurfaceTexture", typeof(ACE.DatLoader.FileTypes.SurfaceTexture)),
                new Entity.FileType(0x06, "Textures", typeof(ACE.DatLoader.FileTypes.Texture)),
                new Entity.FileType(0x08, "Surface", typeof(ACE.DatLoader.FileTypes.Surface)),
                new Entity.FileType(0x09, "MotionTable", typeof(ACE.DatLoader.FileTypes.MotionTable)),
                new Entity.FileType(0x0A, "Sound", typeof(Wave)),
                new Entity.FileType(0x0D, "Environment", typeof(ACE.DatLoader.FileTypes.Environment)),
                new Entity.FileType(0x0E000002, "CharGen", typeof(ACE.DatLoader.FileTypes.CharGen)),
                new Entity.FileType(0x0E000003, "VitalTable", typeof(ACE.DatLoader.FileTypes.SecondaryAttributeTable)),
                new Entity.FileType(0x0E000004, "SkillTable", typeof(ACE.DatLoader.FileTypes.SkillTable)),
                new Entity.FileType(0x0E000007, "ChatPoseTable", typeof(ACE.DatLoader.FileTypes.ChatPoseTable)),
                new Entity.FileType(0x0E00000D, "GeneratorTable", typeof(ACE.DatLoader.FileTypes.GeneratorTable)),
                new Entity.FileType(0x0E00000E, "SpellTable", typeof(ACE.DatLoader.FileTypes.SpellTable)),
                new Entity.FileType(0x0E00000F, "SpellComponents", typeof(ACE.DatLoader.FileTypes.SpellComponentsTable)),
                new Entity.FileType(0x0E000018, "XpTable", typeof(ACE.DatLoader.FileTypes.XpTable)),
                new Entity.FileType(0x0E00001A, "BadData", typeof(ACE.DatLoader.FileTypes.BadData)),
                new Entity.FileType(0x0E00001D, "ContractTable", typeof(ACE.DatLoader.FileTypes.ContractTable)),
                /*new Entity.FileType(0x0E00001E, "TabooTable", typeof(ACE.DatLoader.FileTypes.TabooTable)),*/
                new Entity.FileType(0x0E000020, "NameFilters", typeof(ACE.DatLoader.FileTypes.NameFilterTable)),
                new Entity.FileType(0x0F, "PaletteSet", typeof(ACE.DatLoader.FileTypes.PaletteSet)),
                new Entity.FileType(0x10, "Clothing", typeof(ACE.DatLoader.FileTypes.ClothingTable)),
                new Entity.FileType(0x11, "DegradeInfo", typeof(GfxObjDegradeInfo)),
                new Entity.FileType(0x12, "Scene", typeof(ACE.DatLoader.FileTypes.Scene)),
                new Entity.FileType(0x13, "Region", typeof(RegionDesc)),
                new Entity.FileType(0x20, "SoundTable", typeof(ACE.DatLoader.FileTypes.SoundTable)),
                new Entity.FileType(0x22, "Enums", typeof(ACE.DatLoader.FileTypes.EnumMapper)),
                new Entity.FileType(0x23, "StringTable", typeof(ACE.DatLoader.FileTypes.StringTable)),
                new Entity.FileType(0x25, "DIDs", typeof(ACE.DatLoader.FileTypes.DidMapper)),
                new Entity.FileType(0x27, "DualDIDs", typeof(ACE.DatLoader.FileTypes.DualDidMapper)),
                new Entity.FileType(0x30, "CombatTable", typeof(CombatManeuverTable)),
                new Entity.FileType(0x32, "EmitterInfo", typeof(ACE.DatLoader.FileTypes.ParticleEmitterInfo)),
                new Entity.FileType(0x33, "PhysicsScript", typeof(ACE.DatLoader.FileTypes.PhysicsScript)),
                new Entity.FileType(0x34, "PhysicsScriptTable", typeof(ACE.DatLoader.FileTypes.PhysicsScriptTable)),
            };

            DIDTables.Load();

            DataContext = this;
        }

        private void FileType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatManager.CellDat == null || DatManager.PortalDat == null)
                return;

            var selected = (Entity.FileType)FileType.SelectedItem;

            PortalMode = selected.ID != 0xFFFF && selected.ID != 0xFFFE && selected.ID != 0x100;

            switch (selected.ID)
            {
                case 0x10: // ClothingTable
                    ScriptList.Instance.Visibility = Visibility.Hidden;
                    ClothingTableList.Instance.Visibility = Visibility.Visible;
                    MotionList.Instance.Visibility = Visibility.Hidden;
                    break;
                case 0x34: // PhysicsScriptTable
                    ScriptList.Instance.Visibility = Visibility.Hidden;
                    ClothingTableList.Instance.Visibility = Visibility.Hidden;
                    MotionList.Instance.Visibility = Visibility.Visible;
                    break;
                default:
                    ScriptList.Instance.Visibility = Visibility.Hidden;
                    ClothingTableList.Instance.Visibility = Visibility.Hidden;
                    MotionList.Instance.Visibility = Visibility.Visible;
                    break;
            }

            // strings
            if (selected.ID == 0x23)
                FileIDs = DatManager.LanguageDat.AllFiles.Keys.Where(i => i >> 24 == selected.ID).OrderBy(i => i).Select(i => i.ToString("X8")).ToList();

            // portal files
            else if (selected.ID <= 0x34)
                FileIDs = DatManager.PortalDat.AllFiles.Keys.Where(i => i >> 24 == selected.ID).OrderBy(i => i).Select(i => i.ToString("X8")).ToList();

            // landblock
            else if (selected.ID == 0xFFFF)
            {
                FileIDs = DatManager.CellDat.AllFiles.Keys.Where(i => (i & 0xFFFF) == selected.ID).OrderBy(i => i).Select(i => i.ToString("X8")).ToList();
                MapViewer.Instance.Init();
            }

            // landblock info
            else if (selected.ID == 0xFFFE)
            {
                FileIDs = DatManager.CellDat.AllFiles.Keys.Where(i => (i & 0xFFFF) == selected.ID).OrderBy(i => i).Select(i => i.ToString("X8")).ToList();
                MapViewer.Instance.Init();
            }
            // envcell
            else if (selected.ID == 0x100)
                FileIDs = DatManager.CellDat.AllFiles.Keys.Where(i => (i & 0xFFFF) >= selected.ID && (i & 0xFFFF) < 0xFFFE).OrderBy(i => i).Select(i => i.ToString("X8")).ToList();

            // other
            else
                FileIDs = DatManager.PortalDat.AllFiles.Keys.Where(i => i == selected.ID).OrderBy(i => i).Select(i => i.ToString("X8")).ToList();

            MainWindow.Status.WriteLine($"{selected.Name}s: {FileIDs.Count:N0}");
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Files_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var fileID = Convert.ToUInt32((string)Files.SelectedItem, 16);
            if (fileID == 0) return;

            Selected_FileID = fileID;

            if (PortalMode)
                ReadPortalFile(fileID);
            else
                ReadCellFile(fileID);
        }

        public void ReadCellFile(uint fileID)
        {
            switch (fileID & 0xFFFF)
            {
                case 0xFFFF:
                    var landblock = DatManager.CellDat.ReadFromDat<ACE.DatLoader.FileTypes.CellLandblock>(fileID);
                    FileInfo.SetInfo(new FileTypes.CellLandblock(landblock).BuildTree());

                    GameView.ViewMode = ViewMode.World;
                    WorldViewer.Instance = new WorldViewer();
                    WorldViewer.LoadLandblock(fileID);

                    break;
                case 0xFFFE:
                    var landblockInfo = DatManager.CellDat.ReadFromDat<ACE.DatLoader.FileTypes.LandblockInfo>(fileID);
                    FileInfo.SetInfo(new FileTypes.LandblockInfo(landblockInfo).BuildTree());

                    GameView.ViewMode = ViewMode.World;
                    WorldViewer.Instance = new WorldViewer();
                    WorldViewer.LoadLandblock(fileID | 0xFFFF);

                    break;
                /* >= 0x100 && < 0xFFEE */
                default:
                    var envCell = DatManager.CellDat.ReadFromDat<ACE.DatLoader.FileTypes.EnvCell>(fileID);
                    FileInfo.SetInfo(new FileTypes.EnvCell(envCell).BuildTree());
                    GameView.ViewMode = ViewMode.Model;
                    ModelViewer.LoadEnvCell(fileID);
                    break;
            }
        }

        public static float PaletteScale = 12.0f;

        public void ReadPortalFile(uint fileID)
        {
            switch (fileID >> 24)
            {
                case 0x01:
                    var gfxObj = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.GfxObj>(fileID);
                    FileInfo.SetInfo(new FileTypes.GfxObj(gfxObj).BuildTree());
                    GameView.ViewMode = ViewMode.Model;
                    ModelViewer.LoadModel(fileID);
                    break;
                case 0x02:
                    var setup = DatManager.PortalDat.ReadFromDat<SetupModel>(fileID);
                    FileInfo.SetInfo(new Setup(setup).BuildTree());
                    GameView.ViewMode = ViewMode.Model;
                    ModelViewer.LoadModel(fileID);
                    MotionList.OnClickSetup(fileID);
                    break;
                case 0x03:
                    var anim = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.Animation>(fileID);
                    FileInfo.SetInfo(new FileTypes.Animation(anim).BuildTree());
                    break;
                case 0x04:
                    var palette = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.Palette>(fileID);
                    FileInfo.SetInfo(new FileTypes.Palette(palette).BuildTree());
                    GameView.ViewMode = ViewMode.Texture;
                    TextureViewer.LoadTexture(fileID);
                    if (TextureViewer.CurScale < PaletteScale)
                        TextureViewer.SetScale(PaletteScale);
                    break;
                case 0x05:
                    var surfaceTexture = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.SurfaceTexture>(fileID);
                    FileInfo.SetInfo(new FileTypes.SurfaceTexture(surfaceTexture).BuildTree());
                    GameView.ViewMode = ViewMode.Texture;
                    TextureViewer.LoadTexture(fileID);
                    break;
                case 0x06:
                    var texture = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.Texture>(fileID);
                    FileInfo.SetInfo(new FileTypes.Texture(texture).BuildTree());
                    GameView.ViewMode = ViewMode.Texture;
                    TextureViewer.LoadTexture(fileID);
                    break;
                case 0x08:
                    var surface = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.Surface>(fileID);
                    FileInfo.SetInfo(new FileTypes.Surface(surface).BuildTree(fileID));
                    GameView.ViewMode = ViewMode.Texture;
                    TextureViewer.LoadTexture(fileID);
                    break;
                case 0x09:
                    var motionTable = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.MotionTable>(fileID);
                    FileInfo.SetInfo(new FileTypes.MotionTable(motionTable).BuildTree());
                    break;
                case 0x0A:
                    var sound = DatManager.PortalDat.ReadFromDat<Wave>(fileID);
                    FileInfo.SetInfo(new FileTypes.Sound(sound).BuildTree(fileID));
                    var stream = new MemoryStream();
                    sound.ReadData(stream);
                    var soundPlayer = new SoundPlayer(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    soundPlayer.Play();
                    stream.Close();
                    break;
                case 0x0D:
                    var environment = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.Environment>(fileID);
                    FileInfo.SetInfo(new FileTypes.Environment(environment).BuildTree());
                    GameView.ViewMode = ViewMode.Model;
                    ModelViewer.LoadEnvironment(fileID);
                    break;
                case 0x0E:
                    if (fileID == 0x0E000002)
                    {
                        var charGen = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.CharGen>(fileID);
                        FileInfo.SetInfo(new FileTypes.CharGen(charGen).BuildTree());
                    }
                    else if (fileID == 0x0E000003)
                    {
                        var vitalTable = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.SecondaryAttributeTable>(fileID);
                        FileInfo.SetInfo(new FileTypes.SecondaryAttributeTable(vitalTable).BuildTree());
                    }
                    else if (fileID == 0x0E000004)
                    {
                        var skillTable = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.SkillTable>(fileID);
                        FileInfo.SetInfo(new FileTypes.SkillTable(skillTable).BuildTree());
                    }
                    else if (fileID == 0x0E000007)
                    {
                        var chatPoseTable = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.ChatPoseTable>(fileID);
                        FileInfo.SetInfo(new FileTypes.ChatPoseTable(chatPoseTable).BuildTree());
                    }
                    else if (fileID == 0x0E00000D)
                    {
                        var generatorTable = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.GeneratorTable>(fileID);
                        FileInfo.SetInfo(new FileTypes.GeneratorTable(generatorTable).BuildTree());
                    }
                    else if (fileID == 0x0E00000E)
                    {
                        var spellTable = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.SpellTable>(fileID);
                        FileInfo.SetInfo(new FileTypes.SpellTable(spellTable).BuildTree());
                    }
                    else if (fileID == 0x0E00000F)
                    {
                        var spellComponentsTable = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.SpellComponentsTable>(fileID);
                        FileInfo.SetInfo(new FileTypes.SpellComponentsTable(spellComponentsTable).BuildTree());
                    }
                    else if (fileID == 0x0E000018)
                    {
                        var xpTable = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.XpTable>(fileID);
                        FileInfo.SetInfo(new FileTypes.XpTable(xpTable).BuildTree());
                    }
                    else if (fileID == 0x0E00001A)
                    {
                        var badData = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.BadData>(fileID);
                        FileInfo.SetInfo(new FileTypes.BadData(badData).BuildTree());
                    }
                    else if (fileID == 0x0E00001D)
                    {
                        var contractTable = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.ContractTable>(fileID);
                        FileInfo.SetInfo(new FileTypes.ContractTable(contractTable).BuildTree());
                    }
                    else if (fileID == 0x0E00001E)
                    {
                        var tabooTable = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.TabooTable>(fileID);
                        FileInfo.SetInfo(new FileTypes.TabooTable(tabooTable).BuildTree());
                    }
                    else if (fileID == 0x0E000020)
                    {
                        var nameFilterTable = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.NameFilterTable>(fileID);
                        FileInfo.SetInfo(new FileTypes.NameFilterTable(nameFilterTable).BuildTree());
                    }
                    break;
                case 0x0F:
                    var paletteSet = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.PaletteSet>(fileID);
                    FileInfo.SetInfo(new FileTypes.PaletteSet(paletteSet).BuildTree());
                    GameView.ViewMode = ViewMode.Texture;
                    TextureViewer.LoadTexture(fileID);
                    if (TextureViewer.CurScale < PaletteScale / 2.0f)
                        TextureViewer.SetScale(PaletteScale / 2.0f);
                    break;
                case 0x10:
                    var clothing = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.ClothingTable>(fileID);
                    FileInfo.SetInfo(new FileTypes.ClothingTable(clothing).BuildTree());
                    GameView.ViewMode = ViewMode.Model;
                    ClothingTableList.OnClickClothingBase(clothing, fileID);
                    break;
                case 0x11:
                    var degradeInfo = DatManager.PortalDat.ReadFromDat<GfxObjDegradeInfo>(fileID);
                    FileInfo.SetInfo(new DegradeInfo(degradeInfo).BuildTree());
                    break;
                case 0x12:
                    var scene = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.Scene>(fileID);
                    FileInfo.SetInfo(new FileTypes.Scene(scene).BuildTree());
                    break;
                case 0x13:
                    var region = DatManager.PortalDat.ReadFromDat<RegionDesc>(fileID);
                    FileInfo.SetInfo(new Region(region).BuildTree());
                    break;
                case 0x20:
                    var soundTable = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.SoundTable>(fileID);
                    FileInfo.SetInfo(new FileTypes.SoundTable(soundTable).BuildTree());
                    break;
                case 0x22:
                    var enumMapper = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.EnumMapper>(fileID);
                    FileInfo.SetInfo(new FileTypes.EnumMapper(enumMapper).BuildTree());
                    break;
                case 0x23:
                    var stringTable = DatManager.LanguageDat.ReadFromDat<ACE.DatLoader.FileTypes.StringTable>(fileID);
                    FileInfo.SetInfo(new FileTypes.StringTable(stringTable).BuildTree());
                    break;
                case 0x25:
                    var didMapper = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.DidMapper>(fileID);
                    FileInfo.SetInfo(new FileTypes.DidMapper(didMapper).BuildTree());
                    break;
                case 0x27:
                    var dualDidMapper = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.DualDidMapper>(fileID);
                    FileInfo.SetInfo(new FileTypes.DualDidMapper(dualDidMapper).BuildTree());
                    break;
                case 0x30:
                    var combatTable = DatManager.PortalDat.ReadFromDat<CombatManeuverTable>(fileID);
                    FileInfo.SetInfo(new CombatTable(combatTable).BuildTree());
                    break;
                case 0x32:
                    var emitterInfo = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.ParticleEmitterInfo>(fileID);
                    FileInfo.SetInfo(new FileTypes.ParticleEmitterInfo(emitterInfo).BuildTree());
                    ParticleViewer.Player.PhysicsObj.ParticleManager.ParticleTable.Clear();
                    ParticleViewer.Instance.InitEmitter(fileID, 1.0f);
                    break;
                case 0x33:
                    var playScript = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.PhysicsScript>(fileID);
                    FileInfo.SetInfo(new FileTypes.PhysicsScript(playScript).BuildTree());
                    ParticleViewer.Player.PhysicsObj.ParticleManager.ParticleTable.Clear();
                    ParticleViewer.Instance.InitEmitter(fileID, 1.0f);
                    break;
                case 0x34:
                    var pScriptTable = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.PhysicsScriptTable>(fileID);
                    FileInfo.SetInfo(new FileTypes.PhysicsScriptTable(pScriptTable).BuildTree());
                    ScriptList.Instance.ScriptTable_OnClick(fileID);
                    break;
            }
        }
    }
}

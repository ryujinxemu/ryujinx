using ARMeilleure.Translation.AOT;
using DiscordRPC;
using Gtk;
using JsonPrettyPrinterPlus;
using Ryujinx.Audio;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Gal.OpenGL;
using Ryujinx.Graphics.Gal;
using Ryujinx.HLE.FileSystem;
using Ryujinx.Profiler;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Utf8Json;
using Utf8Json.Resolvers;

using GUI = Gtk.Builder.ObjectAttribute;

namespace Ryujinx.Ui
{
    public class MainWindow : Window
    {
        private static HLE.Switch _device;

        private static IGalRenderer _renderer;

        private static IAalOutput _audioOut;

        private static GlScreen _screen;

        private static ListStore _tableStore;

        private static bool _updatingGameTable;
        private static bool _gameLoaded;
        private static bool _ending;

        private static TreeViewColumn _favColumn;
        private static TreeViewColumn _appColumn;
        private static TreeViewColumn _devColumn;
        private static TreeViewColumn _versionColumn;
        private static TreeViewColumn _timePlayedColumn;
        private static TreeViewColumn _lastPlayedColumn;
        private static TreeViewColumn _fileExtColumn;
        private static TreeViewColumn _fileSizeColumn;
        private static TreeViewColumn _pathColumn;

        private static TreeView _treeView;

        public static bool DiscordIntegrationEnabled { get; set; }

        public static DiscordRpcClient DiscordClient;

        public static RichPresence DiscordPresence;

#pragma warning disable CS0649
#pragma warning disable IDE0044
        [GUI] Window        _mainWin;
        [GUI] CheckMenuItem _fullScreen;
        [GUI] MenuItem      _stopEmulation;
        [GUI] CheckMenuItem _favToggle;
        [GUI] CheckMenuItem _iconToggle;
        [GUI] CheckMenuItem _appToggle;
        [GUI] CheckMenuItem _developerToggle;
        [GUI] CheckMenuItem _versionToggle;
        [GUI] CheckMenuItem _timePlayedToggle;
        [GUI] CheckMenuItem _lastPlayedToggle;
        [GUI] CheckMenuItem _fileExtToggle;
        [GUI] CheckMenuItem _fileSizeToggle;
        [GUI] CheckMenuItem _pathToggle;
        [GUI] TreeView      _gameTable;
        [GUI] Label         _progressLabel;
        [GUI] LevelBar      _progressBar;
#pragma warning restore CS0649
#pragma warning restore IDE0044

        public MainWindow() : this(new Builder("Ryujinx.Ui.MainWindow.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetObject("_mainWin").Handle)
        {
            builder.Autoconnect(this);

            DeleteEvent += Window_Close;

            ApplicationLibrary.ApplicationAdded += Application_Added;

            _renderer = new OglRenderer();

            _audioOut = InitializeAudioEngine();

            _device = new HLE.Switch(_renderer, _audioOut);

            _treeView = _gameTable;

            Configuration.Load(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json"));
            Configuration.InitialConfigure(_device);

            ApplyTheme();

            if (DiscordIntegrationEnabled)
            {
                DiscordClient   = new DiscordRpcClient("568815339807309834");
                DiscordPresence = new RichPresence
                {
                    Assets = new Assets
                    {
                        LargeImageKey  = "ryujinx",
                        LargeImageText = "Ryujinx is an emulator for the Nintendo Switch"
                    },
                    Details    = "Main Menu",
                    State      = "Idling",
                    Timestamps = new Timestamps(DateTime.UtcNow)
                };

                DiscordClient.Initialize();
                DiscordClient.SetPresence(DiscordPresence);
            }

            _mainWin.Icon            = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.Icon.png");
            _stopEmulation.Sensitive = false;

            if (SwitchSettings.SwitchConfig.GuiColumns.FavColumn)        { _favToggle.Active        = true; }
            if (SwitchSettings.SwitchConfig.GuiColumns.IconColumn)       { _iconToggle.Active       = true; }
            if (SwitchSettings.SwitchConfig.GuiColumns.AppColumn)        { _appToggle.Active        = true; }
            if (SwitchSettings.SwitchConfig.GuiColumns.DevColumn)        { _developerToggle.Active  = true; }
            if (SwitchSettings.SwitchConfig.GuiColumns.VersionColumn)    { _versionToggle.Active    = true; }
            if (SwitchSettings.SwitchConfig.GuiColumns.TimePlayedColumn) { _timePlayedToggle.Active = true; }
            if (SwitchSettings.SwitchConfig.GuiColumns.LastPlayedColumn) { _lastPlayedToggle.Active = true; }
            if (SwitchSettings.SwitchConfig.GuiColumns.FileExtColumn)    { _fileExtToggle.Active    = true; }
            if (SwitchSettings.SwitchConfig.GuiColumns.FileSizeColumn)   { _fileSizeToggle.Active   = true; }
            if (SwitchSettings.SwitchConfig.GuiColumns.PathColumn)       { _pathToggle.Active       = true; }

            _gameTable.Model = _tableStore = new ListStore(
                typeof(bool), 
                typeof(Gdk.Pixbuf), 
                typeof(string), 
                typeof(string), 
                typeof(string), 
                typeof(string), 
                typeof(string), 
                typeof(string), 
                typeof(string), 
                typeof(string));
            
            _tableStore.SetSortFunc(5, TimePlayedSort);
            _tableStore.SetSortFunc(6, LastPlayedSort);
            _tableStore.SetSortFunc(8, FileSizeSort);
            _tableStore.SetSortColumnId(0, SortType.Descending);

            UpdateColumns();
#pragma warning disable CS4014
            UpdateGameTable();
#pragma warning restore CS4014
        }

        internal static void ApplyTheme()
        {
            if (!SwitchSettings.SwitchConfig.EnableCustomTheme)
            {
                return;
            }

            if (File.Exists(SwitchSettings.SwitchConfig.CustomThemePath) && (System.IO.Path.GetExtension(SwitchSettings.SwitchConfig.CustomThemePath) == ".css"))
            {
                CssProvider cssProvider = new CssProvider();

                cssProvider.LoadFromPath(SwitchSettings.SwitchConfig.CustomThemePath);

                StyleContext.AddProviderForScreen(Gdk.Screen.Default, cssProvider, 800);
            }
            else
            {
                Logger.PrintWarning(LogClass.Application, $"The \"custom_theme_path\" section in \"Config.json\" contains an invalid path: \"{SwitchSettings.SwitchConfig.CustomThemePath}\".");
            }
        }

        private void UpdateColumns()
        {
            foreach (TreeViewColumn column in _gameTable.Columns)
            {
                _gameTable.RemoveColumn(column);
            }

            CellRendererToggle favToggle = new CellRendererToggle();
            favToggle.Toggled += FavToggle_Toggled;

            if (SwitchSettings.SwitchConfig.GuiColumns.FavColumn)        { _gameTable.AppendColumn("Fav",         favToggle,                "active", 0); }
            if (SwitchSettings.SwitchConfig.GuiColumns.IconColumn)       { _gameTable.AppendColumn("Icon",        new CellRendererPixbuf(), "pixbuf", 1); }
            if (SwitchSettings.SwitchConfig.GuiColumns.AppColumn)        { _gameTable.AppendColumn("Application", new CellRendererText(),   "text",   2); }
            if (SwitchSettings.SwitchConfig.GuiColumns.DevColumn)        { _gameTable.AppendColumn("Developer",   new CellRendererText(),   "text",   3); }
            if (SwitchSettings.SwitchConfig.GuiColumns.VersionColumn)    { _gameTable.AppendColumn("Version",     new CellRendererText(),   "text",   4); }
            if (SwitchSettings.SwitchConfig.GuiColumns.TimePlayedColumn) { _gameTable.AppendColumn("Time Played", new CellRendererText(),   "text",   5); }
            if (SwitchSettings.SwitchConfig.GuiColumns.LastPlayedColumn) { _gameTable.AppendColumn("Last Played", new CellRendererText(),   "text",   6); }
            if (SwitchSettings.SwitchConfig.GuiColumns.FileExtColumn)    { _gameTable.AppendColumn("File Ext",    new CellRendererText(),   "text",   7); }
            if (SwitchSettings.SwitchConfig.GuiColumns.FileSizeColumn)   { _gameTable.AppendColumn("File Size",   new CellRendererText(),   "text",   8); }
            if (SwitchSettings.SwitchConfig.GuiColumns.PathColumn)       { _gameTable.AppendColumn("Path",        new CellRendererText(),   "text",   9); }

            foreach (TreeViewColumn column in _gameTable.Columns)
            {
                if (column.Title == "Fav")              { _favColumn        = column; }
                else if (column.Title == "Application") { _appColumn        = column; }
                else if (column.Title == "Developer")   { _devColumn        = column; }
                else if (column.Title == "Version")     { _versionColumn    = column; }
                else if (column.Title == "Time Played") { _timePlayedColumn = column; }
                else if (column.Title == "Last Played") { _lastPlayedColumn = column; }
                else if (column.Title == "File Ext")    { _fileExtColumn    = column; }
                else if (column.Title == "File Size")   { _fileSizeColumn   = column; }
                else if (column.Title == "Path")        { _pathColumn       = column; }
            }

            if (SwitchSettings.SwitchConfig.GuiColumns.FavColumn)        { _favColumn.SortColumnId        = 0; }
            if (SwitchSettings.SwitchConfig.GuiColumns.IconColumn)       { _appColumn.SortColumnId        = 2; }
            if (SwitchSettings.SwitchConfig.GuiColumns.AppColumn)        { _devColumn.SortColumnId        = 3; }
            if (SwitchSettings.SwitchConfig.GuiColumns.DevColumn)        { _versionColumn.SortColumnId    = 4; }
            if (SwitchSettings.SwitchConfig.GuiColumns.TimePlayedColumn) { _timePlayedColumn.SortColumnId = 5; }
            if (SwitchSettings.SwitchConfig.GuiColumns.LastPlayedColumn) { _lastPlayedColumn.SortColumnId = 6; }
            if (SwitchSettings.SwitchConfig.GuiColumns.FileExtColumn)    { _fileExtColumn.SortColumnId    = 7; }
            if (SwitchSettings.SwitchConfig.GuiColumns.FileSizeColumn)   { _fileSizeColumn.SortColumnId   = 8; }
            if (SwitchSettings.SwitchConfig.GuiColumns.PathColumn)       { _pathColumn.SortColumnId       = 9; }
        }

        internal static async Task UpdateGameTable()
        {
            if (_updatingGameTable)
            {
                return;
            }

            _updatingGameTable = true;

            _tableStore.Clear();

            await Task.Run(() => ApplicationLibrary.LoadApplications(SwitchSettings.SwitchConfig.GameDirs, _device.System.KeySet, _device.System.State.DesiredTitleLanguage));

            _updatingGameTable = false;
        }

        internal void LoadApplication(string path)
        {
            if (_gameLoaded)
            {
                GtkDialog.CreateErrorDialog("A game has already been loaded. Please close the emulator and try again");
            }
            else
            {
                Logger.RestartTime();

                if (Directory.Exists(path))
                {
                    string[] romFsFiles = Directory.GetFiles(path, "*.istorage");

                    if (romFsFiles.Length == 0)
                    {
                        romFsFiles = Directory.GetFiles(path, "*.romfs");
                    }

                    if (romFsFiles.Length > 0)
                    {
                        Logger.PrintInfo(LogClass.Application, "Loading as cart with RomFS.");
                        _device.LoadCart(path, romFsFiles[0]);
                    }
                    else
                    {
                        Logger.PrintInfo(LogClass.Application, "Loading as cart WITHOUT RomFS.");
                        _device.LoadCart(path);
                    }
                }
                else if (File.Exists(path))
                {
                    switch (System.IO.Path.GetExtension(path).ToLowerInvariant())
                    {
                        case ".xci":
                            Logger.PrintInfo(LogClass.Application, "Loading as XCI.");
                            _device.LoadXci(path);
                            break;
                        case ".nca":
                            Logger.PrintInfo(LogClass.Application, "Loading as NCA.");
                            _device.LoadNca(path);
                            break;
                        case ".nsp":
                        case ".pfs0":
                            Logger.PrintInfo(LogClass.Application, "Loading as NSP.");
                            _device.LoadNsp(path);
                            break;
                        default:
                            Logger.PrintInfo(LogClass.Application, "Loading as homebrew.");
                            try
                            {
                                _device.LoadProgram(path);
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                Logger.PrintError(LogClass.Application, "The file which you have specified is unsupported by Ryujinx.");
                            }
                            break;
                    }
                }
                else
                {
                    Logger.PrintWarning(LogClass.Application, "Please specify a valid XCI/NCA/NSP/PFS0/NRO file.");
                    End();
                }

#if MACOS_BUILD
                CreateGameWindow();
#else
                new Thread(CreateGameWindow).Start();
#endif

                _gameLoaded              = true;
                _stopEmulation.Sensitive = true;

                if (DiscordIntegrationEnabled)
                {
                    if (File.ReadAllLines(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RPsupported.dat")).Contains(_device.System.TitleId))
                    {
                        DiscordPresence.Assets.LargeImageKey = _device.System.TitleId;
                    }

                    string state = _device.System.TitleId;

                    if (state == null)
                    {
                        state = "Ryujinx";
                    }
                    else
                    {
                        state = state.ToUpper();
                    }

                    string details = "Idling";

                    if (_device.System.TitleName != null)
                    {
                        details = $"Playing {_device.System.TitleName}";
                    }

                    DiscordPresence.Details               = details;
                    DiscordPresence.State                 = state;
                    DiscordPresence.Assets.LargeImageText = _device.System.TitleName;
                    DiscordPresence.Assets.SmallImageKey  = "ryujinx";
                    DiscordPresence.Assets.SmallImageText = "Ryujinx is an emulator for the Nintendo Switch";
                    DiscordPresence.Timestamps            = new Timestamps(DateTime.UtcNow);

                    DiscordClient.SetPresence(DiscordPresence);
                }

                string metadataFolder = System.IO.Path.Combine(new VirtualFileSystem().GetBasePath(), "games", _device.System.TitleId, "gui");
                string metadataFile   = System.IO.Path.Combine(metadataFolder, "metadata.json");

                IJsonFormatterResolver resolver = CompositeResolver.Create(new[] { StandardResolver.AllowPrivateSnakeCase });

                ApplicationMetadata appMetadata;

                if (!File.Exists(metadataFile))
                {
                    Directory.CreateDirectory(metadataFolder);

                    appMetadata = new ApplicationMetadata
                    {
                        Favorite   = false,
                        TimePlayed = 0,
                        LastPlayed = "Never"
                    };

                    byte[] data = JsonSerializer.Serialize(appMetadata, resolver);
                    File.WriteAllText(metadataFile, Encoding.UTF8.GetString(data, 0, data.Length).PrettyPrintJson());
                }

                using (Stream stream = File.OpenRead(metadataFile))
                {
                    appMetadata = JsonSerializer.Deserialize<ApplicationMetadata>(stream, resolver);
                }

                appMetadata.LastPlayed = DateTime.UtcNow.ToString();

                byte[] saveData = JsonSerializer.Serialize(appMetadata, resolver);
                File.WriteAllText(metadataFile, Encoding.UTF8.GetString(saveData, 0, saveData.Length).PrettyPrintJson());
            }
        }

        private static void CreateGameWindow()
        {
            Configuration.ConfigureHid(_device, SwitchSettings.SwitchConfig);
            
            using (_screen = new GlScreen(_device, _renderer))
            {
                _screen.MainLoop();

                End();
            }
        }

        private static void End()
        {
            if (_ending)
            {
                return;
            }

            _ending = true;

            if (_gameLoaded)
            {
                string metadataFolder = System.IO.Path.Combine(new VirtualFileSystem().GetBasePath(), "games", _device.System.TitleId, "gui");
                string metadataFile   = System.IO.Path.Combine(metadataFolder, "metadata.json");

                IJsonFormatterResolver resolver = CompositeResolver.Create(new[] { StandardResolver.AllowPrivateSnakeCase });

                ApplicationMetadata appMetadata;

                if (!File.Exists(metadataFile))
                {
                    Directory.CreateDirectory(metadataFolder);

                    appMetadata = new ApplicationMetadata
                    {
                        Favorite   = false,
                        TimePlayed = 0,
                        LastPlayed = "Never"
                    };

                    byte[] data = JsonSerializer.Serialize(appMetadata, resolver);
                    File.WriteAllText(metadataFile, Encoding.UTF8.GetString(data, 0, data.Length).PrettyPrintJson());
                }

                using (Stream stream = File.OpenRead(metadataFile))
                {
                    appMetadata = JsonSerializer.Deserialize<ApplicationMetadata>(stream, resolver);
                }

                DateTime lastPlayedDateTime = DateTime.Parse(appMetadata.LastPlayed);
                double   sessionTimePlayed  = DateTime.UtcNow.Subtract(lastPlayedDateTime).TotalSeconds;

                appMetadata.TimePlayed += Math.Round(sessionTimePlayed, MidpointRounding.AwayFromZero);

                byte[] saveData = JsonSerializer.Serialize(appMetadata, resolver);
                File.WriteAllText(metadataFile, Encoding.UTF8.GetString(saveData, 0, saveData.Length).PrettyPrintJson());
            }

            Profile.FinishProfiling();
            Aot.Dispose();
            _device.Dispose();
            _audioOut.Dispose();
            DiscordClient?.Dispose();
            Logger.Shutdown();
            Environment.Exit(0);
        }

        /// <summary>
        /// Picks an <see cref="IAalOutput"/> audio output renderer supported on this machine
        /// </summary>
        /// <returns>An <see cref="IAalOutput"/> supported by this machine</returns>
        private static IAalOutput InitializeAudioEngine()
        {
            if (SoundIoAudioOut.IsSupported)
            {
                return new SoundIoAudioOut();
            }
            else if (OpenALAudioOut.IsSupported)
            {
                return new OpenALAudioOut();
            }
            else
            {
                return new DummyAudioOut();
            }
        }

        //Events
        private void Application_Added(object sender, ApplicationAddedEventArgs e)
        {
            Application.Invoke(delegate
            {
                _tableStore.AppendValues(
                    e.AppData.Favorite,
                    new Gdk.Pixbuf(e.AppData.Icon, 75, 75),
                    $"{e.AppData.TitleName}\n{e.AppData.TitleId.ToUpper()}",
                    e.AppData.Developer,
                    e.AppData.Version,
                    e.AppData.TimePlayed,
                    e.AppData.LastPlayed,
                    e.AppData.FileExtension,
                    e.AppData.FileSize,
                    e.AppData.Path);

                _progressLabel.Text = $"{e.NumAppsLoaded}/{e.NumAppsFound} Games Loaded";
                _progressBar.Value  = (float)e.NumAppsLoaded / e.NumAppsFound;
            });
        }

        private void FavToggle_Toggled(object sender, ToggledArgs args)
        {
            _tableStore.GetIter(out TreeIter treeIter, new TreePath(args.Path));

            string titleId      = _tableStore.GetValue(treeIter, 2).ToString().Split("\n")[1].ToLower();
            string metadataPath = System.IO.Path.Combine(new VirtualFileSystem().GetBasePath(), "games", titleId, "gui", "metadata.json");

            IJsonFormatterResolver resolver = CompositeResolver.Create(new[] { StandardResolver.AllowPrivateSnakeCase });

            ApplicationMetadata appMetadata;
            
            using (Stream stream = File.OpenRead(metadataPath))
            {
                appMetadata = JsonSerializer.Deserialize<ApplicationMetadata>(stream, resolver);
            }

            if ((bool)_tableStore.GetValue(treeIter, 0))
            {
                _tableStore.SetValue(treeIter, 0, false);

                appMetadata.Favorite = false;
            }
            else
            {
                _tableStore.SetValue(treeIter, 0, true);

                appMetadata.Favorite = true;
            }

            byte[] saveData = JsonSerializer.Serialize(appMetadata, resolver);
            File.WriteAllText(metadataPath, Encoding.UTF8.GetString(saveData, 0, saveData.Length).PrettyPrintJson());
        }

        private void Row_Activated(object sender, RowActivatedArgs args)
        {
            _tableStore.GetIter(out TreeIter treeIter, new TreePath(args.Path.ToString()));
            string path = (string)_tableStore.GetValue(treeIter, 9);

            LoadApplication(path);
        }

        private void Load_Application_File(object sender, EventArgs args)
        {
            FileChooserDialog fileChooser = new FileChooserDialog("Choose the file to open", this, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);

            fileChooser.Filter = new FileFilter();
            fileChooser.Filter.AddPattern("*.nsp" );
            fileChooser.Filter.AddPattern("*.pfs0");
            fileChooser.Filter.AddPattern("*.xci" );
            fileChooser.Filter.AddPattern("*.nca" );
            fileChooser.Filter.AddPattern("*.nro" );
            fileChooser.Filter.AddPattern("*.nso" );

            if (fileChooser.Run() == (int)ResponseType.Accept)
            {
                LoadApplication(fileChooser.Filename);
            }

            fileChooser.Dispose();
        }

        private void Load_Application_Folder(object sender, EventArgs args)
        {
            FileChooserDialog fileChooser = new FileChooserDialog("Choose the folder to open", this, FileChooserAction.SelectFolder, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);

            if (fileChooser.Run() == (int)ResponseType.Accept)
            {
                LoadApplication(fileChooser.Filename);
            }

            fileChooser.Dispose();
        }

        private void Open_Ryu_Folder(object sender, EventArgs args)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName        = new VirtualFileSystem().GetBasePath(),
                UseShellExecute = true,
                Verb            = "open"
            });
        }

        private void Exit_Pressed(object sender, EventArgs args)
        {
            _screen?.Exit();
            End();
        }

        private void Window_Close(object sender, DeleteEventArgs args)
        {
            _screen?.Exit();
            End();
        }

        private void StopEmulation_Pressed(object sender, EventArgs args)
        {
            // TODO: Write logic to kill running game

            _gameLoaded = false;
        }

        private void FullScreen_Toggled(object sender, EventArgs args)
        {
            if (_fullScreen.Active)
            {
                Fullscreen();
            }
            else
            {
                Unfullscreen();
            }
        }

        private void Settings_Pressed(object sender, EventArgs args)
        {
            SwitchSettings settingsWin = new SwitchSettings(_device);
            settingsWin.Show();
        }

        private void Update_Pressed(object sender, EventArgs args)
        {
            string ryuUpdater = System.IO.Path.Combine(new VirtualFileSystem().GetBasePath(), "RyuUpdater.exe");

            try
            {
                Process.Start(new ProcessStartInfo(ryuUpdater, "/U") { UseShellExecute = true });
            }
            catch(System.ComponentModel.Win32Exception)
            {
                GtkDialog.CreateErrorDialog("Update canceled by user or updater was not found");
            }
        }

        private void About_Pressed(object sender, EventArgs args)
        {
            AboutWindow aboutWin = new AboutWindow();
            aboutWin.Show();
        }

        private void Fav_Toggled(object sender, EventArgs args)
        {
            GuiColumns updatedColumns = SwitchSettings.SwitchConfig.GuiColumns;

            updatedColumns.FavColumn               = _favToggle.Active;
            SwitchSettings.SwitchConfig.GuiColumns = updatedColumns;

            Configuration.SaveConfig(SwitchSettings.SwitchConfig, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json"));

            UpdateColumns();
        }

        private void Icon_Toggled(object sender, EventArgs args)
        {
            GuiColumns updatedColumns = SwitchSettings.SwitchConfig.GuiColumns;

            updatedColumns.IconColumn              = _iconToggle.Active;
            SwitchSettings.SwitchConfig.GuiColumns = updatedColumns;

            Configuration.SaveConfig(SwitchSettings.SwitchConfig, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json"));

            UpdateColumns();
        }

        private void Title_Toggled(object sender, EventArgs args)
        {
            GuiColumns updatedColumns = SwitchSettings.SwitchConfig.GuiColumns;

            updatedColumns.AppColumn               = _appToggle.Active;
            SwitchSettings.SwitchConfig.GuiColumns = updatedColumns;

            Configuration.SaveConfig(SwitchSettings.SwitchConfig, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json"));

            UpdateColumns();
        }

        private void Developer_Toggled(object sender, EventArgs args)
        {
            GuiColumns updatedColumns = SwitchSettings.SwitchConfig.GuiColumns;

            updatedColumns.DevColumn               = _developerToggle.Active;
            SwitchSettings.SwitchConfig.GuiColumns = updatedColumns;

            Configuration.SaveConfig(SwitchSettings.SwitchConfig, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json"));

            UpdateColumns();
        }

        private void Version_Toggled(object sender, EventArgs args)
        {
            GuiColumns updatedColumns = SwitchSettings.SwitchConfig.GuiColumns;

            updatedColumns.VersionColumn           = _versionToggle.Active;
            SwitchSettings.SwitchConfig.GuiColumns = updatedColumns;

            Configuration.SaveConfig(SwitchSettings.SwitchConfig, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json"));

            UpdateColumns();
        }

        private void TimePlayed_Toggled(object sender, EventArgs args)
        {
            GuiColumns updatedColumns = SwitchSettings.SwitchConfig.GuiColumns;

            updatedColumns.TimePlayedColumn        = _timePlayedToggle.Active;
            SwitchSettings.SwitchConfig.GuiColumns = updatedColumns;

            Configuration.SaveConfig(SwitchSettings.SwitchConfig, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json"));

            UpdateColumns();
        }

        private void LastPlayed_Toggled(object sender, EventArgs args)
        {
            GuiColumns updatedColumns = SwitchSettings.SwitchConfig.GuiColumns;

            updatedColumns.LastPlayedColumn        = _lastPlayedToggle.Active;
            SwitchSettings.SwitchConfig.GuiColumns = updatedColumns;

            Configuration.SaveConfig(SwitchSettings.SwitchConfig, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json"));

            UpdateColumns();
        }

        private void FileExt_Toggled(object sender, EventArgs args)
        {
            GuiColumns updatedColumns = SwitchSettings.SwitchConfig.GuiColumns;

            updatedColumns.FileExtColumn           = _fileExtToggle.Active;
            SwitchSettings.SwitchConfig.GuiColumns = updatedColumns;

            Configuration.SaveConfig(SwitchSettings.SwitchConfig, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json"));

            UpdateColumns();
        }

        private void FileSize_Toggled(object sender, EventArgs args)
        {
            GuiColumns updatedColumns = SwitchSettings.SwitchConfig.GuiColumns;

            updatedColumns.FileSizeColumn          = _fileSizeToggle.Active;
            SwitchSettings.SwitchConfig.GuiColumns = updatedColumns;

            Configuration.SaveConfig(SwitchSettings.SwitchConfig, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json"));

            UpdateColumns();
        }

        private void Path_Toggled(object sender, EventArgs args)
        {
            GuiColumns updatedColumns = SwitchSettings.SwitchConfig.GuiColumns;

            updatedColumns.PathColumn              = _pathToggle.Active;
            SwitchSettings.SwitchConfig.GuiColumns = updatedColumns;

            Configuration.SaveConfig(SwitchSettings.SwitchConfig, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json"));

            UpdateColumns();
        }

        private void RefreshList_Pressed(object sender, ButtonReleaseEventArgs args)
        {
#pragma warning disable CS4014
            UpdateGameTable();
#pragma warning restore CS4014
        }

        private static int TimePlayedSort(ITreeModel model, TreeIter a, TreeIter b)
        {
            string aValue = model.GetValue(a, 5).ToString();
            string bValue = model.GetValue(b, 5).ToString();

            if (aValue.Length > 4 && aValue.Substring(aValue.Length - 4) == "mins")
            {
                aValue = (float.Parse(aValue.Substring(0, aValue.Length - 5)) * 60).ToString();
            }
            else if (aValue.Length > 3 && aValue.Substring(aValue.Length - 3) == "hrs")
            {
                aValue = (float.Parse(aValue.Substring(0, aValue.Length - 4)) * 3600).ToString();
            }
            else if (aValue.Length > 4 && aValue.Substring(aValue.Length - 4) == "days")
            {
                aValue = (float.Parse(aValue.Substring(0, aValue.Length - 5)) * 86400).ToString();
            }
            else
            {
                aValue = aValue.Substring(0, aValue.Length - 1);
            }

            if (bValue.Length > 4 && bValue.Substring(bValue.Length - 4) == "mins")
            {
                bValue = (float.Parse(bValue.Substring(0, bValue.Length - 5)) * 60).ToString();
            }
            else if (bValue.Length > 3 && bValue.Substring(bValue.Length - 3) == "hrs")
            {
                bValue = (float.Parse(bValue.Substring(0, bValue.Length - 4)) * 3600).ToString();
            }
            else if (bValue.Length > 4 && bValue.Substring(bValue.Length - 4) == "days")
            {
                bValue = (float.Parse(bValue.Substring(0, bValue.Length - 5)) * 86400).ToString();
            }
            else
            {
                bValue = bValue.Substring(0, bValue.Length - 1);
            }

            if (float.Parse(aValue) > float.Parse(bValue))
            {
                return -1;
            }
            else if (float.Parse(bValue) > float.Parse(aValue))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        private static int LastPlayedSort(ITreeModel model, TreeIter a, TreeIter b)
        {
            string aValue = model.GetValue(a, 6).ToString();
            string bValue = model.GetValue(b, 6).ToString();

            if (aValue == "Never")
            {
                aValue = DateTime.UnixEpoch.ToString();
            }

            if (bValue == "Never")
            {
                bValue = DateTime.UnixEpoch.ToString();
            }

            return DateTime.Compare(DateTime.Parse(bValue), DateTime.Parse(aValue));
        }

        private static int FileSizeSort(ITreeModel model, TreeIter a, TreeIter b)
        {
            string aValue = model.GetValue(a, 8).ToString();
            string bValue = model.GetValue(b, 8).ToString();

            if (aValue.Substring(aValue.Length - 2) == "GB")
            {
                aValue = (float.Parse(aValue[0..^2]) * 1024).ToString();
            }
            else
            {
                aValue = aValue[0..^2];
            }

            if (bValue.Substring(bValue.Length - 2) == "GB")
            {
                bValue = (float.Parse(bValue[0..^2]) * 1024).ToString();
            }
            else
            {
                bValue = bValue[0..^2];
            }

            if (float.Parse(aValue) > float.Parse(bValue))
            {
                return -1;
            }
            else if (float.Parse(bValue) > float.Parse(aValue))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}

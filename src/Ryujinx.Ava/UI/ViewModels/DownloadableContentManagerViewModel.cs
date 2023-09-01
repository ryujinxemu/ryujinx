using Avalonia.Collections;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using DynamicData;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.UI.Helpers;
using Ryujinx.Ava.UI.Models;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Common.Utilities;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.Loaders.Processes.Extensions;
using Ryujinx.Ui.App.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Application = Avalonia.Application;
using Path = System.IO.Path;

namespace Ryujinx.Ava.UI.ViewModels
{
    public class DownloadableContentManagerViewModel : BaseModel
    {
        private readonly List<DownloadableContentContainer> _downloadableContentContainerList;
        private readonly string _downloadableContentJsonPath;

        private readonly VirtualFileSystem _virtualFileSystem;
        private AvaloniaList<DownloadableContentModel> _downloadableContents = new();
        private AvaloniaList<DownloadableContentModel> _views = new();
        private AvaloniaList<DownloadableContentModel> _selectedDownloadableContents = new();

        private string _search;
        private readonly ApplicationData _title;

        private static readonly DownloadableContentJsonSerializerContext _serializerContext = new(JsonHelper.GetDefaultSerializerOptions());

        public AvaloniaList<DownloadableContentModel> DownloadableContents
        {
            get => _downloadableContents;
            set
            {
                _downloadableContents = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UpdateCount));
                Sort();
            }
        }

        public AvaloniaList<DownloadableContentModel> Views
        {
            get => _views;
            set
            {
                _views = value;
                OnPropertyChanged();
            }
        }

        public AvaloniaList<DownloadableContentModel> SelectedDownloadableContents
        {
            get => _selectedDownloadableContents;
            set
            {
                _selectedDownloadableContents = value;
                OnPropertyChanged();
            }
        }

        public string Search
        {
            get => _search;
            set
            {
                _search = value;
                OnPropertyChanged();
                Sort();
            }
        }

        public string UpdateCount
        {
            get => string.Format(LocaleManager.Instance[LocaleKeys.DlcWindowHeading], DownloadableContents.Count);
        }

        public IStorageProvider StorageProvider;

        public DownloadableContentManagerViewModel(VirtualFileSystem virtualFileSystem, ApplicationData applicationData)
        {
            _virtualFileSystem = virtualFileSystem;

            _title = applicationData;

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                StorageProvider = desktop.MainWindow.StorageProvider;
            }

            _downloadableContentJsonPath = Path.Combine(AppDataManager.GamesDirPath, applicationData.TitleIdString, "dlc.json");

            if (!File.Exists(_downloadableContentJsonPath))
            {
                _downloadableContentContainerList = new List<DownloadableContentContainer>();
                Save();
            }

            try
            {
                _downloadableContentContainerList = JsonHelper.DeserializeFromFile(_downloadableContentJsonPath, _serializerContext.ListDownloadableContentContainer);
            }
            catch
            {
                Logger.Error?.Print(LogClass.Configuration, "Downloadable Content JSON failed to deserialize.");
                _downloadableContentContainerList = new List<DownloadableContentContainer>();
            }

            LoadDownloadableContents();
        }

        private void LoadDownloadableContents()
        {
            // Try to load DLCs from PFS first
            AddDownloadableContent(_title.Path);

            foreach (DownloadableContentContainer downloadableContentContainer in _downloadableContentContainerList)
            {
                if (File.Exists(downloadableContentContainer.ContainerPath))
                {
                    using FileStream containerFile = File.OpenRead(downloadableContentContainer.ContainerPath);

                    PartitionFileSystem partitionFileSystem = new();
                    partitionFileSystem.Initialize(containerFile.AsStorage()).ThrowIfFailure();

                    _virtualFileSystem.ImportTickets(partitionFileSystem);

                    foreach (DownloadableContentNca downloadableContentNca in downloadableContentContainer.DownloadableContentNcaList)
                    {
                        using UniqueRef<IFile> ncaFile = new();

                        partitionFileSystem.OpenFile(ref ncaFile.Ref, downloadableContentNca.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();

                        Nca nca = TryOpenNca(ncaFile.Get.AsStorage(), downloadableContentContainer.ContainerPath);
                        if (nca != null)
                        {
                            var content = new DownloadableContentModel(nca.Header.TitleId.ToString("X16"),
                                downloadableContentContainer.ContainerPath,
                                downloadableContentNca.FullPath,
                                downloadableContentNca.Enabled);

                            DownloadableContents.Add(content);

                            if (content.Enabled)
                            {
                                SelectedDownloadableContents.Add(content);
                            }

                            OnPropertyChanged(nameof(UpdateCount));
                        }
                    }
                }
            }

            // NOTE: Save the list again to remove leftovers.
            Save();
            Sort();
        }

        public void Sort()
        {
            DownloadableContents.AsObservableChangeSet()
                .Filter(Filter)
                .Bind(out var view).AsObservableList();

            _views.Clear();
            _views.AddRange(view);
            OnPropertyChanged(nameof(Views));
        }

        private bool Filter(object arg)
        {
            if (arg is DownloadableContentModel content)
            {
                return string.IsNullOrWhiteSpace(_search) || content.FileName.ToLower().Contains(_search.ToLower()) || content.TitleId.ToLower().Contains(_search.ToLower());
            }

            return false;
        }

        private Nca TryOpenNca(IStorage ncaStorage, string containerPath)
        {
            try
            {
                return new Nca(_virtualFileSystem.KeySet, ncaStorage);
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await ContentDialogHelper.CreateErrorDialog(string.Format(LocaleManager.Instance[LocaleKeys.DialogLoadNcaErrorMessage], ex.Message, containerPath));
                });
            }

            return null;
        }

        public async void Add()
        {
            var result = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = LocaleManager.Instance[LocaleKeys.SelectDlcDialogTitle],
                AllowMultiple = true,
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new("NSP")
                    {
                        Patterns = new[] { "*.nsp" },
                        AppleUniformTypeIdentifiers = new[] { "com.ryujinx.nsp" },
                        MimeTypes = new[] { "application/x-nx-nsp" },
                    },
                },
            });

            foreach (var file in result)
            {
                if (!AddDownloadableContent(file.Path.LocalPath))
                {
                    await ContentDialogHelper.CreateErrorDialog(LocaleManager.Instance[LocaleKeys.DialogDlcNoDlcErrorMessage]);
                }
            }
        }

        private bool AddDownloadableContent(string path)
        {
            if (!File.Exists(path) || DownloadableContents.FirstOrDefault(x => x.ContainerPath == path) != null)
            {
                return true;
            }

            using FileStream containerFile = File.OpenRead(path);

            IFileSystem partitionFileSystem;

            if (Path.GetExtension(path).ToLower() == ".xci")
            {
                partitionFileSystem = new Xci(_virtualFileSystem.KeySet, containerFile.AsStorage()).OpenPartition(XciPartitionType.Secure);
            }
            else
            {
                var pfsTemp = new PartitionFileSystem();
                pfsTemp.Initialize(containerFile.AsStorage()).ThrowIfFailure();
                partitionFileSystem = pfsTemp;
            }

            _virtualFileSystem.ImportTickets(partitionFileSystem);

            foreach (DirectoryEntryEx fileEntry in partitionFileSystem.EnumerateEntries("/", "*.nca"))
            {
                using var ncaFile = new UniqueRef<IFile>();

                partitionFileSystem.OpenFile(ref ncaFile.Ref, fileEntry.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();

                Nca nca = TryOpenNca(ncaFile.Get.AsStorage(), path);
                if (nca == null)
                {
                    continue;
                }

                if (nca.Header.ContentType == NcaContentType.PublicData)
                {
                    if (nca.GetProgramIdBase() != (_title.TitleIdBase))
                    {
                        break;
                    }

                    var content = new DownloadableContentModel(nca.Header.TitleId.ToString("X16"), path, fileEntry.FullPath, true);
                    DownloadableContents.Add(content);
                    SelectedDownloadableContents.Add(content);

                    OnPropertyChanged(nameof(UpdateCount));
                    Sort();

                    return true;
                }
            }

            return false;
        }

        public void Remove(DownloadableContentModel model)
        {
            DownloadableContents.Remove(model);
            OnPropertyChanged(nameof(UpdateCount));
            Sort();
        }

        public void RemoveAll()
        {
            DownloadableContents.Clear();
            OnPropertyChanged(nameof(UpdateCount));
            Sort();
        }

        public void EnableAll()
        {
            SelectedDownloadableContents = new(DownloadableContents);
        }

        public void DisableAll()
        {
            SelectedDownloadableContents.Clear();
        }

        public void Save()
        {
            _downloadableContentContainerList.Clear();

            DownloadableContentContainer container = default;

            foreach (DownloadableContentModel downloadableContent in DownloadableContents)
            {
                if (container.ContainerPath != downloadableContent.ContainerPath)
                {
                    if (!string.IsNullOrWhiteSpace(container.ContainerPath))
                    {
                        _downloadableContentContainerList.Add(container);
                    }

                    container = new DownloadableContentContainer
                    {
                        ContainerPath = downloadableContent.ContainerPath,
                        DownloadableContentNcaList = new List<DownloadableContentNca>(),
                    };
                }

                container.DownloadableContentNcaList.Add(new DownloadableContentNca
                {
                    Enabled = downloadableContent.Enabled,
                    TitleId = Convert.ToUInt64(downloadableContent.TitleId, 16),
                    FullPath = downloadableContent.FullPath,
                });
            }

            if (!string.IsNullOrWhiteSpace(container.ContainerPath))
            {
                _downloadableContentContainerList.Add(container);
            }

            JsonHelper.SerializeToFile(_downloadableContentJsonPath, _downloadableContentContainerList, _serializerContext.ListDownloadableContentContainer);
        }

    }
}

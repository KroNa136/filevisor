using FileSystemEntryType = FileVisor.Models.FileSystemEntry.FileSystemEntryType;
using Path = System.IO.Path;
using SortColumn = FileVisor.Models.Settings.SortColumn;
using SortDirection = FileVisor.Models.Settings.SortDirection;

using FileVisor.Converters;
using FileVisor.Models;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;

using static FileVisor.CustomDialogBox.DialogBox;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace FileVisor
{
    internal static class FileSystemEntryFetcher
    {
        const string SYSTEM_DIRECTORIES_PATH_NAME = "Системные папки";
        const string DEFAULT_DIRECTORY_TYPE = "Папка с файлами";
        const string SYSTEM_DIRECTORY_TYPE = "Системная папка";
        const string DEFAULT_FILE_TYPE = "Файл";
        const string SHORTCUT_TYPE = "Ярлык";

        static List<FileSystemEntry> cache = new List<FileSystemEntry>();
        static List<string> cacheIndex = new List<string>();

        public static FileSystemEntry GetRootEntry()
        {
            FileSystemEntry entry = new FileSystemEntry()
            {
                EntryType = FileSystemEntryType.Root,
                Name = FileSystemHelper.ROOT_PATH_NAME,
                Icon = OSIcon.IconReader.ExtractIconFromFile("C:\\Windows\\system32\\shell32.dll", 15),
                ChildEntries = new ObservableCollection<FileSystemEntry>()
            };

            return entry;
        }

        public static List<FileSystemEntry> GetDrives(Settings settings)
        {
            List<FileSystemEntry> drives = new List<FileSystemEntry>();

            try
            {
                foreach (DriveInfo driveInfo in DriveInfo.GetDrives())
                {
                    FileSystemEntry drive = GetDrive(driveInfo);

                    if (drive != null)
                        drives.Add(drive);
                }

                return Sort(drives, settings.SelectedSortColumn, settings.SelectedSortDirection);
            }
            catch (Exception ex)
            {
                ShowDialogBox(ex.Message, null, DialogBoxType.Error, DialogBoxButtons.OK);
            }

            return Sort(drives, settings.SelectedSortColumn, settings.SelectedSortDirection);
        }

        public static List<FileSystemEntry> GetDirectories(string path, Settings settings)
        {
            List<FileSystemEntry> directories = new List<FileSystemEntry>();

            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return directories;

            try
            {
                foreach (string directoryName in Directory.GetDirectories(path))
                {
                    FileSystemEntry directory = GetDirectory(directoryName, settings);

                    if (directory != null)
                        directories.Add(directory);
                }

                return Sort(directories, settings.SelectedSortColumn, settings.SelectedSortDirection);
            }
            catch (Exception ex)
            {
                ShowDialogBox(ex.Message, null, DialogBoxType.Error, DialogBoxButtons.OK);
            }

            return Sort(directories, settings.SelectedSortColumn, settings.SelectedSortDirection);
        }

        public static List<FileSystemEntry> GetFiles(string path, Settings settings)
        {
            List<FileSystemEntry> files = new List<FileSystemEntry>();

            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return files;

            try
            {
                foreach (string fileName in Directory.GetFiles(path))
                {
                    FileSystemEntry file = GetFile(fileName, settings);

                    if (file != null)
                        files.Add(file);
                }

                return Sort(files, settings.SelectedSortColumn, settings.SelectedSortDirection);
            }
            catch (Exception ex)
            {
                ShowDialogBox(ex.Message, null, DialogBoxType.Error, DialogBoxButtons.OK);
            }

            return Sort(files, settings.SelectedSortColumn, settings.SelectedSortDirection);
        }

        public static FileSystemEntry GetSystemDirectoriesEntry()
        {
            FileSystemEntry entry = new FileSystemEntry()
            {
                EntryType = FileSystemEntryType.Virtual,
                Name = SYSTEM_DIRECTORIES_PATH_NAME,
                Icon = Properties.Resources.logo_96x96,
                ChildEntries = new ObservableCollection<FileSystemEntry>()
            };

            return entry;
        }

        public static List<FileSystemEntry> GetSystemDirectories()
        {
            List<FileSystemEntry> directories = new List<FileSystemEntry>();

            IKnownFolder folder;
            DirectoryInfo directoryInfo;
            
            try
            {
                Icon[] shellIcons = OSIcon.IconReader.ExtractIconsFromFile("C:\\Windows\\system32\\imageres.dll", true);

                folder = KnownFolders.Desktop;
                directoryInfo = new DirectoryInfo(folder.Path);

                FileSystemEntry desktopEntry = new FileSystemEntry()
                {
                    EntryType = FileSystemEntryType.Directory,
                    FullName = directoryInfo.FullName,
                    Name = folder.LocalizedName,
                    Icon = shellIcons[174],
                    Path = directoryInfo.FullName.Substring(0, directoryInfo.FullName.LastIndexOf(Path.DirectorySeparatorChar)),
                    Type = SYSTEM_DIRECTORY_TYPE,
                    DateCreated = directoryInfo.CreationTime,
                    DateModified = directoryInfo.LastWriteTime,
                    DateOpened = directoryInfo.LastAccessTime,
                    ReadableSize = "-",
                    IsReadOnly = directoryInfo.Attributes.HasFlag(FileAttributes.ReadOnly),
                    IsHidden = directoryInfo.Attributes.HasFlag(FileAttributes.Hidden),
                    IsSystem = directoryInfo.Attributes.HasFlag(FileAttributes.System)
                };

                directories.Add(desktopEntry);

                folder = KnownFolders.Documents;
                directoryInfo = new DirectoryInfo(folder.Path);

                FileSystemEntry documentsEntry = new FileSystemEntry()
                {
                    EntryType = FileSystemEntryType.Directory,
                    FullName = directoryInfo.FullName,
                    Name = folder.LocalizedName,
                    Icon = shellIcons[107],
                    Path = directoryInfo.FullName.Substring(0, directoryInfo.FullName.LastIndexOf(Path.DirectorySeparatorChar)),
                    Type = SYSTEM_DIRECTORY_TYPE,
                    DateCreated = directoryInfo.CreationTime,
                    DateModified = directoryInfo.LastWriteTime,
                    DateOpened = directoryInfo.LastAccessTime,
                    ReadableSize = "-",
                    IsReadOnly = directoryInfo.Attributes.HasFlag(FileAttributes.ReadOnly),
                    IsHidden = directoryInfo.Attributes.HasFlag(FileAttributes.Hidden),
                    IsSystem = directoryInfo.Attributes.HasFlag(FileAttributes.System)
                };

                directories.Add(documentsEntry);

                folder = KnownFolders.Downloads;
                directoryInfo = new DirectoryInfo(folder.Path);

                FileSystemEntry downloadsEntry = new FileSystemEntry()
                {
                    EntryType = FileSystemEntryType.Directory,
                    FullName = directoryInfo.FullName,
                    Name = folder.LocalizedName,
                    Icon = shellIcons[175],
                    Path = directoryInfo.FullName.Substring(0, directoryInfo.FullName.LastIndexOf(Path.DirectorySeparatorChar)),
                    Type = SYSTEM_DIRECTORY_TYPE,
                    DateCreated = directoryInfo.CreationTime,
                    DateModified = directoryInfo.LastWriteTime,
                    DateOpened = directoryInfo.LastAccessTime,
                    ReadableSize = "-",
                    IsReadOnly = directoryInfo.Attributes.HasFlag(FileAttributes.ReadOnly),
                    IsHidden = directoryInfo.Attributes.HasFlag(FileAttributes.Hidden),
                    IsSystem = directoryInfo.Attributes.HasFlag(FileAttributes.System)
                };

                directories.Add(downloadsEntry);

                folder = KnownFolders.Music;
                directoryInfo = new DirectoryInfo(folder.Path);

                FileSystemEntry musicEntry = new FileSystemEntry()
                {
                    EntryType = FileSystemEntryType.Directory,
                    FullName = directoryInfo.FullName,
                    Name = folder.LocalizedName,
                    Icon = shellIcons[103],
                    Path = directoryInfo.FullName.Substring(0, directoryInfo.FullName.LastIndexOf(Path.DirectorySeparatorChar)),
                    Type = SYSTEM_DIRECTORY_TYPE,
                    DateCreated = directoryInfo.CreationTime,
                    DateModified = directoryInfo.LastWriteTime,
                    DateOpened = directoryInfo.LastAccessTime,
                    ReadableSize = "-",
                    IsReadOnly = directoryInfo.Attributes.HasFlag(FileAttributes.ReadOnly),
                    IsHidden = directoryInfo.Attributes.HasFlag(FileAttributes.Hidden),
                    IsSystem = directoryInfo.Attributes.HasFlag(FileAttributes.System)
                };

                directories.Add(musicEntry);

                folder = KnownFolders.Pictures;
                directoryInfo = new DirectoryInfo(folder.Path);

                FileSystemEntry picturesEntry = new FileSystemEntry()
                {
                    EntryType = FileSystemEntryType.Directory,
                    FullName = directoryInfo.FullName,
                    Name = folder.LocalizedName,
                    Icon = shellIcons[108],
                    Path = directoryInfo.FullName.Substring(0, directoryInfo.FullName.LastIndexOf(Path.DirectorySeparatorChar)),
                    Type = SYSTEM_DIRECTORY_TYPE,
                    DateCreated = directoryInfo.CreationTime,
                    DateModified = directoryInfo.LastWriteTime,
                    DateOpened = directoryInfo.LastAccessTime,
                    ReadableSize = "-",
                    IsReadOnly = directoryInfo.Attributes.HasFlag(FileAttributes.ReadOnly),
                    IsHidden = directoryInfo.Attributes.HasFlag(FileAttributes.Hidden),
                    IsSystem = directoryInfo.Attributes.HasFlag(FileAttributes.System)
                };

                directories.Add(picturesEntry);

                folder = KnownFolders.Videos;
                directoryInfo = new DirectoryInfo(folder.Path);

                FileSystemEntry videosEntry = new FileSystemEntry()
                {
                    EntryType = FileSystemEntryType.Directory,
                    FullName = directoryInfo.FullName,
                    Name = folder.LocalizedName,
                    Icon = shellIcons[178],
                    Path = directoryInfo.FullName.Substring(0, directoryInfo.FullName.LastIndexOf(Path.DirectorySeparatorChar)),
                    Type = SYSTEM_DIRECTORY_TYPE,
                    DateCreated = directoryInfo.CreationTime,
                    DateModified = directoryInfo.LastWriteTime,
                    DateOpened = directoryInfo.LastAccessTime,
                    ReadableSize = "-",
                    IsReadOnly = directoryInfo.Attributes.HasFlag(FileAttributes.ReadOnly),
                    IsHidden = directoryInfo.Attributes.HasFlag(FileAttributes.Hidden),
                    IsSystem = directoryInfo.Attributes.HasFlag(FileAttributes.System)
                };

                directories.Add(videosEntry);
            }
            catch (Exception ex)
            {
                ShowDialogBox(ex.Message, null, DialogBoxType.Error, DialogBoxButtons.OK);
            }

            return Sort(directories, SortColumn.Name, SortDirection.Ascending);
        }

        static List<FileSystemEntry> Sort(List<FileSystemEntry> fileSystemEntries, SortColumn sortColumn, SortDirection sortDirection)
        {
            switch (sortColumn)
            {
                case SortColumn.Name:
                {
                    fileSystemEntries.Sort(delegate (FileSystemEntry x, FileSystemEntry y)
                    {
                        if (x.FullName is null && y.FullName is null)
                            return 0;
                        else if (x.FullName is null)
                            return -1;
                        else if (y.FullName is null)
                            return 1;
                        else
                            return x.FullName.CompareTo(y.FullName);
                    });

                    break;
                }
                case SortColumn.Type:
                {
                    fileSystemEntries.Sort(delegate (FileSystemEntry x, FileSystemEntry y)
                    {
                        if (x.Type is null && y.Type is null)
                            return 0;
                        else if (x.Type is null)
                            return -1;
                        else if (y.Type is null)
                            return 1;
                        else
                            return x.Type.CompareTo(y.Type);
                    });

                    break;
                }
                case SortColumn.DateCreated:
                {
                    fileSystemEntries.Sort(delegate (FileSystemEntry x, FileSystemEntry y)
                    {
                        return x.DateCreated.CompareTo(y.DateCreated);
                    });

                    break;
                }
                case SortColumn.DateModified:
                {
                    fileSystemEntries.Sort(delegate (FileSystemEntry x, FileSystemEntry y)
                    {
                        return x.DateModified.CompareTo(y.DateModified);
                    });

                    break;
                }
                case SortColumn.Size:
                {
                    fileSystemEntries.Sort(delegate (FileSystemEntry x, FileSystemEntry y)
                    {
                        return x.ByteSize.CompareTo(y.ByteSize);
                    });

                    break;
                }
            }

            if (sortDirection is SortDirection.Descending)
                fileSystemEntries.Reverse();

            return fileSystemEntries;
        }

        public static FileSystemEntry GetDrive(DriveInfo driveInfo)
        {
            string driveName = driveInfo.Name;

            if (cacheIndex.Contains(driveName))
                return cache[cacheIndex.IndexOf(driveName)];

            if (!driveInfo.IsReady)
                return null;

            string driveType = FileSystemHelper.GetDriveType(driveInfo);

            string volumeLabel = string.IsNullOrEmpty(driveInfo.VolumeLabel) ?
                                 driveType :
                                 driveInfo.VolumeLabel;

            string driveLetter = driveName.Substring(0, 2);

            FileSystemEntry entry = new FileSystemEntry()
            {
                EntryType = FileSystemEntryType.Drive,
                FullName = driveName,
                Name = string.Format("{0} ({1})", volumeLabel, driveLetter),
                Icon = IconHelper.GetIconForFile(driveName, true, true),
                Type = driveType,
                ByteSize = driveInfo.TotalSize,
                ReadableSize = UnitConverter.BytesToReadableSize(driveInfo.TotalSize),
                IsReadOnly = false,
                IsHidden = false,
                IsSystem = false
            };

            if (entry.Icon != null)
            {
                cache.Add(entry);
                cacheIndex.Add(entry.FullName);
            }

            return entry;
        }

        public static FileSystemEntry GetDirectory(string path, Settings settings)
        {
            FileSystemEntry entry;

            if (cacheIndex.Contains(path))
            {
                entry = cache[cacheIndex.IndexOf(path)];

                if (!settings.ShowHiddenElements && entry.IsHidden)
                    return null;

                if (!settings.ShowSystemElements && entry.IsSystem)
                    return null;

                return entry;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            if (!settings.ShowHiddenElements && directoryInfo.Attributes.HasFlag(FileAttributes.Hidden))
                return null;

            if (!settings.ShowSystemElements && directoryInfo.Attributes.HasFlag(FileAttributes.System))
                return null;

            entry = new FileSystemEntry()
            {
                EntryType = FileSystemEntryType.Directory,
                FullName = directoryInfo.FullName,
                Name = directoryInfo.Name,
                Icon = IconHelper.GetIconForFile(path, true, true),
                Path = directoryInfo.FullName.Substring(0, directoryInfo.FullName.LastIndexOf(Path.DirectorySeparatorChar)),
                Type = DEFAULT_DIRECTORY_TYPE,
                DateCreated = directoryInfo.CreationTime,
                DateModified = directoryInfo.LastWriteTime,
                DateOpened = directoryInfo.LastAccessTime,
                ReadableSize = "-",
                IsReadOnly = directoryInfo.Attributes.HasFlag(FileAttributes.ReadOnly),
                IsHidden = directoryInfo.Attributes.HasFlag(FileAttributes.Hidden),
                IsSystem = directoryInfo.Attributes.HasFlag(FileAttributes.System)
            };

            if (entry.Icon != null)
            {
                cache.Add(entry);
                cacheIndex.Add(entry.FullName);
            }

            return entry;
        }

        public static FileSystemEntry GetFile(string path, Settings settings)
        {
            FileSystemEntry entry;

            if (cacheIndex.Contains(path))
            {
                entry = cache[cacheIndex.IndexOf(path)];

                if (!settings.ShowHiddenElements && entry.IsHidden)
                    return null;

                if (!settings.ShowSystemElements && entry.IsSystem)
                    return null;

                return entry;
            }

            FileInfo fileInfo = new FileInfo(path);

            if (!settings.ShowHiddenElements && fileInfo.Attributes.HasFlag(FileAttributes.Hidden))
                return null;

            if (!settings.ShowSystemElements && fileInfo.Attributes.HasFlag(FileAttributes.System))
                return null;

            entry = new FileSystemEntry()
            {
                FullName = path,
                Icon = IconHelper.GetIconForFile(path, true, false),
                Extension = fileInfo.Extension,
                Path = fileInfo.DirectoryName,
                DateCreated = fileInfo.CreationTime,
                DateModified = fileInfo.LastWriteTime,
                DateOpened = fileInfo.LastAccessTime,
                ByteSize = fileInfo.Length,
                ReadableSize = UnitConverter.BytesToReadableSize(fileInfo.Length),
                IsReadOnly = fileInfo.Attributes.HasFlag(FileAttributes.ReadOnly),
                IsHidden = fileInfo.Attributes.HasFlag(FileAttributes.Hidden),
                IsSystem = fileInfo.Attributes.HasFlag(FileAttributes.System)
            };

            if (entry.Extension.Equals(".lnk"))
            {
                entry.EntryType = FileSystemEntryType.Shortcut;
                entry.Name = FileSystemHelper.GetShortFileName(fileInfo.Name, false);
                entry.Type = SHORTCUT_TYPE;
            }
            else
            {
                entry.EntryType = FileSystemEntryType.File;

                entry.Name = settings.ShowFileExtensions ?
                                fileInfo.Name :
                                FileSystemHelper.GetShortFileName(fileInfo.Name, false);

                entry.Type = string.IsNullOrEmpty(entry.Extension) ?
                                DEFAULT_FILE_TYPE :
                                FileSystemHelper.FriendlyDocName(entry.Extension);
            }

            if (entry.Icon != null)
            {
                cache.Add(entry);
                cacheIndex.Add(entry.FullName);
            }

            return entry;
        }

        internal static void RemoveFromCache(string path)
        {
            if (path is null)
                return;

            if (!cacheIndex.Contains(path))
                return;

            int i = cacheIndex.IndexOf(path);
            cacheIndex.RemoveAt(i);
            cache.RemoveAt(i);
        }

        internal static void AddExtensionsToFilesInCache()
        {
            foreach (FileSystemEntry entry in cache.Where(e => e.EntryType is FileSystemEntryType.File))
            {
                if (entry.Name.Equals(FileSystemHelper.GetShortFileName(entry.FullName, false)))
                    entry.Name = FileSystemHelper.GetShortFileName(entry.FullName, true);
            }
        }

        internal static void RemoveExtensionsFromFilesInCache()
        {
            foreach (FileSystemEntry entry in cache.Where(e => e.EntryType is FileSystemEntryType.File))
            {
                if (entry.Name.Equals(FileSystemHelper.GetShortFileName(entry.FullName, true)))
                    entry.Name = FileSystemHelper.GetShortFileName(entry.FullName, false);
            }
        }
    }
}

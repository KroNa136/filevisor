using FileVisor.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace FileVisor.Models
{
    internal class DirectoryProperties : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string Name { get; set; }
        public Icon Icon { get; set; }
        public string Type { get; set; }
        public string Path { get; set; }

        string size;
        public string Size
        {
            get { return size; }
            set
            {
                size = value;
                OnPropertyChanged();
            }
        }

        string sizeOnDisk;
        public string SizeOnDisk
        {
            get { return sizeOnDisk; }
            set
            {
                sizeOnDisk = value;
                OnPropertyChanged();
            }
        }

        string contents;
        public string Contents
        {
            get { return contents; }
            set
            {
                contents = value;
                OnPropertyChanged();
            }
        }

        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public DateTime DateOpened { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsHidden { get; set; }
        public bool IsSystem { get; set; }
        public bool IsCompressed { get; set; }
        public bool IsEncrypted { get; set; }

        const string CALCULATING_PLACEHOLDER = "Вычисляется...";

        List<CancellationTokenSource> cancellationTokenSources = new List<CancellationTokenSource>();

        public DirectoryProperties(FileSystemEntry entry)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(entry.FullName);

            Name = entry.Name;
            Icon = OSIcon.IconReader.GetFolderIcon(OSIcon.IconSize.Jumbo, OSIcon.FolderState.Open).Icon;

            Type = entry.Type;

            Path = entry.Path;
            Size = CALCULATING_PLACEHOLDER;
            SizeOnDisk = CALCULATING_PLACEHOLDER;
            Contents = CALCULATING_PLACEHOLDER;
            GetDirectorySizeAsync(directoryInfo);
            GetDirectorySizeOnDiskAsync(directoryInfo);
            GetChildEntryCountAsync(directoryInfo);

            DateCreated = entry.DateCreated;
            DateModified = entry.DateModified;
            DateOpened = entry.DateOpened;

            IsReadOnly = entry.IsReadOnly;
            IsHidden = entry.IsHidden;
            IsSystem = entry.IsSystem;
            IsCompressed = directoryInfo.Attributes.HasFlag(FileAttributes.Compressed);
            IsEncrypted = directoryInfo.Attributes.HasFlag(FileAttributes.Encrypted);
        }

        public void CancelAllAsyncOperations()
        {
            cancellationTokenSources.ForEach(cts => cts.Cancel());
        }

        async void GetDirectorySizeAsync(DirectoryInfo directoryInfo, bool recursive = true)
        {
            var tokenSource = new CancellationTokenSource();
            cancellationTokenSources.Add(tokenSource);

            CancellationToken ct = tokenSource.Token;

            await Task.Run(() =>
            {
                long totalSize = GetDirectorySize(directoryInfo, recursive, ct);
                Size = string.Format("{0} ({1} Б)", UnitConverter.BytesToReadableSize(totalSize), totalSize);
            }, tokenSource.Token);

            cancellationTokenSources.Remove(tokenSource);
            tokenSource.Dispose();
        }

        async void GetDirectorySizeOnDiskAsync(DirectoryInfo directoryInfo, bool recursive = true)
        {
            var tokenSource = new CancellationTokenSource();
            cancellationTokenSources.Add(tokenSource);

            CancellationToken ct = tokenSource.Token;

            await Task.Run(() =>
            {
                long totalSizeOnDisk = GetDirectorySizeOnDisk(directoryInfo, recursive, ct);
                SizeOnDisk = string.Format("{0} ({1} Б)", UnitConverter.BytesToReadableSize(totalSizeOnDisk), totalSizeOnDisk);
            }, tokenSource.Token);

            cancellationTokenSources.Remove(tokenSource);
            tokenSource.Dispose();
        }

        async void GetChildEntryCountAsync(DirectoryInfo directoryInfo, bool recursive = true)
        {
            var tokenSource = new CancellationTokenSource();
            cancellationTokenSources.Add(tokenSource);

            CancellationToken ct = tokenSource.Token;

            long totalFiles = 0;
            long totalDirectories = 0;

            Task countFiles = Task.Run(() =>
            {
                totalFiles = GetFileCount(directoryInfo, recursive, ct);
            }, tokenSource.Token);

            Task countDirectories = Task.Run(() =>
            {
                totalDirectories = GetDirectoryCount(directoryInfo, recursive, ct);
            }, tokenSource.Token);

            try
            {
                await Task.WhenAll(countFiles, countDirectories);
                Contents = string.Format("Файлов: {0}, папок: {1}", totalFiles, totalDirectories);
            }
            catch (Exception)
            {
                Contents = "Ошибка вычисления";
            }

            cancellationTokenSources.Remove(tokenSource);
            tokenSource.Dispose();
        }

        long GetDirectorySize(DirectoryInfo directoryInfo, bool recursive, CancellationToken ct)
        {
            long totalSize = 0;

            if (ct.IsCancellationRequested)
                return totalSize;

            try
            {
                foreach (FileInfo fileInfo in directoryInfo.GetFiles())
                {
                    try
                    {
                        Interlocked.Add(ref totalSize, fileInfo.Length);
                    }
                    catch (Exception) { }

                    if (ct.IsCancellationRequested)
                        return totalSize;
                }
            }
            catch (Exception) { }

            if (recursive)
            {
                try
                {
                    Parallel.ForEach(directoryInfo.GetDirectories(), (subDirectory) =>
                    {
                        try
                        {
                            Interlocked.Add(ref totalSize, GetDirectorySize(subDirectory, recursive, ct));
                        }
                        catch (Exception) { }

                        if (ct.IsCancellationRequested)
                            return;
                    });
                }
                catch (Exception) { }
            }

            return totalSize;
        }

        long GetDirectorySizeOnDisk(DirectoryInfo directoryInfo, bool recursive, CancellationToken ct)
        {
            long totalSizeOnDisk = 0;

            if (ct.IsCancellationRequested)
                return totalSizeOnDisk;

            try
            {
                foreach (FileInfo fileInfo in directoryInfo.GetFiles())
                {
                    try
                    {
                        Interlocked.Add(ref totalSizeOnDisk, FileSystemHelper.GetFileSizeOnDisk(fileInfo.FullName));
                    }
                    catch (Exception) { }

                    if (ct.IsCancellationRequested)
                        return totalSizeOnDisk;
                }
            }
            catch (Exception) { }

            if (recursive)
            {
                try
                {
                    Parallel.ForEach(directoryInfo.GetDirectories(), (subDirectory) =>
                    {
                        try
                        {
                            Interlocked.Add(ref totalSizeOnDisk, GetDirectorySizeOnDisk(subDirectory, recursive, ct));
                        }
                        catch (Exception) { }

                        if (ct.IsCancellationRequested)
                            return;
                    });
                }
                catch (Exception) { }
            }

            return totalSizeOnDisk;
        }

        long GetFileCount(DirectoryInfo directoryInfo, bool recursive, CancellationToken ct)
        {
            long totalCount = 0;

            if (ct.IsCancellationRequested)
                return totalCount;

            try
            {
                Interlocked.Add(ref totalCount, directoryInfo.GetFiles().Length);
            }
            catch (Exception) { }

            if (ct.IsCancellationRequested)
                return totalCount;

            if (recursive)
            {
                try
                {
                    Parallel.ForEach(directoryInfo.GetDirectories(), (subDirectory) =>
                    {
                        try
                        {
                            Interlocked.Add(ref totalCount, GetFileCount(subDirectory, recursive, ct));
                        }
                        catch (Exception) { }

                        if (ct.IsCancellationRequested)
                            return;
                    });
                }
                catch (Exception) { }
            }

            return totalCount;
        }

        long GetDirectoryCount(DirectoryInfo directoryInfo, bool recursive, CancellationToken ct)
        {
            long totalCount = 0;

            if (ct.IsCancellationRequested)
                return totalCount;

            try
            {
                Interlocked.Add(ref totalCount, directoryInfo.GetDirectories().Length);
            }
            catch (Exception) { }

            if (ct.IsCancellationRequested)
                return totalCount;

            if (recursive)
            {
                try
                {
                    Parallel.ForEach(directoryInfo.GetDirectories(), (subDirectory) =>
                    {
                        try
                        {
                            Interlocked.Add(ref totalCount, GetDirectoryCount(subDirectory, recursive, ct));
                        }
                        catch (Exception) { }

                        if (ct.IsCancellationRequested)
                            return;
                    });
                }
                catch (Exception) { }
            }

            return totalCount;
        }
    }
}

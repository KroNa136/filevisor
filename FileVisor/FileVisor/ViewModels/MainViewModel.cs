using FileVisor.Models;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace FileVisor.ViewModels
{
    internal class MainViewModel
    {
        readonly object fileSystemEntriesLock = new object();
        public ObservableCollection<FileSystemEntry> FileSystemEntries { get; set; }
        public ObservableCollection<FileSystemEntry> TreeViewEntries { get; set; }

        internal void EnableSynchronization()
        {
            if (FileSystemEntries is null)
                return;

            BindingOperations.EnableCollectionSynchronization(FileSystemEntries, fileSystemEntriesLock);
        }
    }
}
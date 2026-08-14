using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace FileVisor.Models
{
    public class FileSystemEntry : INotifyPropertyChanged
    {
        public enum FileSystemEntryType
        {
            Virtual, Root, Drive, Directory, File, Shortcut
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public FileSystemEntryType EntryType { get; set; }
        public string FullName { get; set; }
        public Icon Icon { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public DateTime DateOpened { get; set; }
        public double ByteSize { get; set; }
        public string ReadableSize { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsHidden { get; set; }
        public bool IsSystem { get; set; }

        bool isCut;
        public bool IsCut
        {
            get { return isCut; }
            set
            {
                isCut = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FileSystemEntry> ChildEntries { get; set; }

        public static FileSystemEntry FindEntryInChildren(string fullName, FileSystemEntry parent)
        {
            foreach (FileSystemEntry child in parent.ChildEntries)
            {
                if (child.FullName.Equals(fullName))
                {
                    return child;
                }
                else
                {
                    FileSystemEntry entry = FindEntryInChildren(fullName, child);

                    if (entry is null)
                        continue;
                    else
                        return entry;
                }
            }

            return null;
        }
    }
}

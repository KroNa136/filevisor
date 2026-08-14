using FileVisor.Converters;
using Shell32;
using System;
using System.Drawing;
using System.IO;

namespace FileVisor.Models
{
    internal class ShortcutProperties
    {
        public string Name { get; set; }
        public Icon Icon { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string TargetPath { get; set; }
        public string WorkingDirectory { get; set; }
        public string Path { get; set; }
        public string Size { get; set; }
        public string SizeOnDisk { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public DateTime DateOpened { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsHidden { get; set; }
        public bool IsSystem { get; set; }
        public bool IsCompressed { get; set; }
        public bool IsEncrypted { get; set; }

        public ShortcutProperties(FileSystemEntry entry)
        {
            FileInfo fileInfo = new FileInfo(entry.FullName);
            ShellLinkObject shortcutObject = FileSystemHelper.GetShortcutObject(entry.FullName);

            long sizeOnDisk = FileSystemHelper.GetFileSizeOnDisk(entry.FullName);

            Name = entry.Name;
            Icon = OSIcon.IconReader.GetFileIcon(entry.FullName, OSIcon.IconSize.Jumbo).Icon;

            Type = entry.Type;
            Description = shortcutObject.Description;
            TargetPath = shortcutObject.Path;
            WorkingDirectory = shortcutObject.WorkingDirectory;

            Path = entry.Path;
            Size = string.Format("{0} ({1} Б)", entry.ReadableSize, entry.ByteSize);
            SizeOnDisk = string.Format("{0} ({1} Б)", UnitConverter.BytesToReadableSize(sizeOnDisk), sizeOnDisk);

            DateCreated = entry.DateCreated;
            DateModified = entry.DateModified;
            DateOpened = entry.DateOpened;

            IsReadOnly = entry.IsReadOnly;
            IsHidden = entry.IsHidden;
            IsSystem = entry.IsSystem;
            IsCompressed = fileInfo.Attributes.HasFlag(FileAttributes.Compressed);
            IsEncrypted = fileInfo.Attributes.HasFlag(FileAttributes.Encrypted);
        }
    }
}

using FileVisor.Converters;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace FileVisor.Models
{
    internal class FileProperties
    {
        public string Name { get; set; }
        public Icon Icon { get; set; }
        public string TypeWithExtension { get; set; }
        public Icon AppIcon { get; set; }
        public string AppName { get; set; }
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

        readonly string[] UNKNOWN_APP_NAME_EXTENSIONS =
        {
            "",
            ".ico",
            ".sys"
        };
        const string UNKNOWN_APP_NAME = "Неизвестное приложение";

        const string EXECUTABLE_EXTENSION = ".exe";
        const string EXECUTABLE_APP_NAME = "-";

        public FileProperties(FileSystemEntry entry)
        {
            FileInfo fileInfo = new FileInfo(entry.FullName);

            long sizeOnDisk = FileSystemHelper.GetFileSizeOnDisk(entry.FullName);

            Name = entry.Name;
            Icon = OSIcon.IconReader.GetFileIcon(entry.FullName, OSIcon.IconSize.Jumbo).Icon;

            TypeWithExtension = string.Format("{0} ({1})", entry.Type, entry.Extension);
            AppIcon = FileSystemHelper.FriendlyAppIcon(entry.Extension);
            AppName = UNKNOWN_APP_NAME_EXTENSIONS.Contains(entry.Extension) ?
                      UNKNOWN_APP_NAME :
                      entry.Extension.Equals(EXECUTABLE_EXTENSION) ?
                      EXECUTABLE_APP_NAME :
                      FileSystemHelper.FriendlyAppName(entry.Extension);

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

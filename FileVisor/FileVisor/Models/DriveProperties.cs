using FileVisor.Converters;
using System.Drawing;
using System.IO;

namespace FileVisor.Models
{
    internal class DriveProperties
    {
        public string Name { get; set; }
        public Icon Icon { get; set; }
        public string Type { get; set; }
        public string FileSystem { get; set; }
        public string UsedSpace { get; set; }
        public string AvailableSpace { get; set; }
        public string TotalSpace { get; set; }

        public DriveProperties(FileSystemEntry entry)
        {
            DriveInfo driveInfo = new DriveInfo(entry.FullName);

            Name = entry.Name;
            Icon = OSIcon.IconReader.GetFileIcon(entry.FullName, OSIcon.IconSize.Jumbo).Icon;

            Type = entry.Type;
            FileSystem = driveInfo.DriveFormat;

            long availableSpace = driveInfo.AvailableFreeSpace;
            long usedSpace = driveInfo.TotalSize - availableSpace;

            UsedSpace = string.Format("{0} ({1} Б)", UnitConverter.BytesToReadableSize(usedSpace), usedSpace);
            AvailableSpace = string.Format("{0} ({1} Б)", UnitConverter.BytesToReadableSize(availableSpace), availableSpace);
            TotalSpace = string.Format("{0} ({1} Б)", entry.ReadableSize, entry.ByteSize);
        }
    }
}

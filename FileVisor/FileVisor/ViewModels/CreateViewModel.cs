using ElementType = FileVisor.Models.CreatedElementType.ElementType;

using FileVisor.Models;
using System.Collections.ObjectModel;

namespace FileVisor.ViewModels
{
    internal class CreateViewModel
    {
        public static ObservableCollection<CreatedElementType> CreatedElementTypes { get; } = new ObservableCollection<CreatedElementType>()
        {
            new CreatedElementType()
            {
                ID = ElementType.Directory,
                Icon = OSIcon.IconReader.GetFolderIcon(OSIcon.IconSize.ExtraLarge, OSIcon.FolderState.Open).Icon,
                Name = "Папка",
                Extension = null,
            },
            new CreatedElementType()
            {
                ID = ElementType.TXT,
                Icon = OSIcon.IconReader.GetFileIcon(".txt", OSIcon.IconSize.ExtraLarge).Icon,
                Name = FileSystemHelper.FriendlyDocName(".txt"),
                Extension = "txt"
            },
            new CreatedElementType()
            {
                ID = ElementType.RTF,
                Icon = OSIcon.IconReader.GetFileIcon(".rtf", OSIcon.IconSize.ExtraLarge).Icon,
                Name = FileSystemHelper.FriendlyDocName(".rtf"),
                Extension = "rtf"
            },
            new CreatedElementType()
            {
                ID = ElementType.BMP,
                Icon = OSIcon.IconReader.GetFileIcon(".bmp", OSIcon.IconSize.ExtraLarge).Icon,
                Name = FileSystemHelper.FriendlyDocName(".bmp"),
                Extension = "bmp"
            },
            new CreatedElementType()
            {
                ID = ElementType.Other,
                Icon = IconHelper.GetIconForFile("not a path, this gets an empty file icon", true, false),
                Name = "Другое...",
                Extension = ""
            }
        };

        public ElementType SelectedID { get; set; } = ElementType.Directory;
    }
}

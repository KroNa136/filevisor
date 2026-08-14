using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FileVisor
{
    internal static class IconHelper
    {
        [DllImport("gdi32.dll", SetLastError = true)]
        static extern bool DeleteObject(IntPtr obj);

        const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;

        const uint SHGFI_ICON = 0x000000100;
        const uint SHGFI_LARGEICON = 0x000000000;
        const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;

        [DllImport("shell32")]
        static extern int SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, uint flags);

        [StructLayout(LayoutKind.Sequential)]
        struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        public static ImageSource ToImageSource(this Icon ico)
        {
            if (ico is null)
                return null;

            Bitmap bitmap = ico.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource image = Imaging.CreateBitmapSourceFromHBitmap
            (
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );

            DeleteObject(hBitmap);
            return image;
        }

        public static Icon GetIconForFile(string path, bool largeIcon, bool isDirectoryOrDrive)
        {
            uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES;
            if (largeIcon)
                flags |= SHGFI_LARGEICON;

            uint attributes = FILE_ATTRIBUTE_NORMAL;
            if (isDirectoryOrDrive)
                attributes |= FILE_ATTRIBUTE_DIRECTORY;

            int success = SHGetFileInfo
            (
                path,
                attributes,
                out SHFILEINFO shfi,
                (uint) Marshal.SizeOf(typeof(SHFILEINFO)),
                flags
            );

            if (success == 0)
                return null;

            if (shfi.hIcon == IntPtr.Zero)
                return null;
            else
                return Icon.FromHandle(shfi.hIcon);
        }
    }
}

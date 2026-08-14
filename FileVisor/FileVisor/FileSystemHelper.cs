using Shell32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Navigation;

namespace FileVisor
{
    internal static class FileSystemHelper
    {
        internal const string ROOT_PATH_NAME = "Этот компьютер";

        #region Get file info by its type/extension

        const string UNKNOWN_APP_NAME = "Неизвестное приложение";

        [Flags]
        enum AssocF
        {
            Init_NoRemapCLSID = 0x1,
            Init_ByExeName = 0x2,
            Open_ByExeName = 0x2,
            Init_DefaultToStar = 0x4,
            Init_DefaultToFolder = 0x8,
            NoUserSettings = 0x10,
            NoTruncate = 0x20,
            Verify = 0x40,
            RemapRunDll = 0x80,
            NoFixUps = 0x100,
            IgnoreBaseClass = 0x200
        }

        public enum AssocStr
        {
            Command = 1,
            Executable,
            FriendlyDocName,
            FriendlyAppName,
            NoOpen,
            ShellNewValue,
            DDECommand,
            DDEIfExec,
            DDEApplication,
            DDETopic
        }

        [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint AssocQueryString
        (
            AssocF flags,
            AssocStr str,
            string pszAssoc,
            string pszExtra,
            [Out] StringBuilder pszOut,
            [In][Out] ref uint pcchOut
        );

        internal static string FileExtentionInfo(AssocStr assocStr, string doctype)
        {
            uint pcchOut = 0;

            AssocQueryString(AssocF.Verify, assocStr, doctype, null, null, ref pcchOut);

            StringBuilder pszOut = new StringBuilder((int) pcchOut);
            AssocQueryString(AssocF.Verify, assocStr, doctype, null, pszOut, ref pcchOut);

            return pszOut.ToString();
        }

        internal static string FriendlyDocName(string doctype)
        {
            return FileExtentionInfo(AssocStr.FriendlyDocName, doctype);
        }

        internal static string FriendlyAppName(string doctype)
        {
            string appName = FileExtentionInfo(AssocStr.FriendlyAppName, doctype);

            if (string.IsNullOrEmpty(appName))
                return UNKNOWN_APP_NAME;

            return appName;
        }

        internal static Icon FriendlyAppIcon(string doctype)
        {
            string appExecutable = FileExtentionInfo(AssocStr.Executable, doctype);

            if (string.IsNullOrEmpty(appExecutable))
                return null;

            return OSIcon.IconReader.ExtractIconFromFileEx(appExecutable, OSIcon.IconSize.ExtraLarge).Icon;
        }

        #endregion

        #region Get file size on disk

        [DllImport("kernel32.dll")]
        static extern uint GetCompressedFileSizeW
        (
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
            [Out, MarshalAs(UnmanagedType.U4)] out uint lpFileSizeHigh
        );

        [DllImport("kernel32.dll", SetLastError = true, PreserveSig = true)]
        static extern int GetDiskFreeSpaceW
        (
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpRootPathName,
            out uint lpSectorsPerCluster,
            out uint lpBytesPerSector,
            out uint lpNumberOfFreeClusters,
            out uint lpTotalNumberOfClusters
        );

        internal static long GetFileSizeOnDisk(string file)
        {
            FileInfo info = new FileInfo(file);

            int result = GetDiskFreeSpaceW(info.Directory.Root.FullName, out uint sectorsPerCluster, out uint bytesPerSector, out _, out _);

            if (result == 0)
                throw new Win32Exception();

            uint clusterSize = sectorsPerCluster * bytesPerSector;
            uint losize = GetCompressedFileSizeW(file, out uint hosize);
            long size = (long) hosize << 32 | losize;

            return ((size + clusterSize - 1) / clusterSize) * clusterSize;
        }

        #endregion

        #region Move file to the recycle bin

        [Flags]
        public enum FileOperationFlags : ushort
        {
            FOF_SILENT = 0x0004,
            FOF_NOCONFIRMATION = 0x0010,
            FOF_ALLOWUNDO = 0x0040,
            FOF_SIMPLEPROGRESS = 0x0100,
            FOF_NOERRORUI = 0x0400,
            FOF_WANTNUKEWARNING = 0x4000,
        }

        public enum FileOperationType : uint
        {
            FO_MOVE = 0x0001,
            FO_COPY = 0x0002,
            FO_DELETE = 0x0003,
            FO_RENAME = 0x0004,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.U4)]
            public FileOperationType wFunc;
            public string pFrom;
            public string pTo;
            public FileOperationFlags fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lpszProgressTitle;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

        internal static bool Send(string path, FileOperationFlags flags)
        {
            try
            {
                var fs = new SHFILEOPSTRUCT
                {
                    wFunc = FileOperationType.FO_DELETE,
                    pFrom = path + '\0' + '\0',
                    fFlags = FileOperationFlags.FOF_ALLOWUNDO | flags
                };

                SHFileOperation(ref fs);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal static bool Send(string path)
        {
            return Send(path, FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_WANTNUKEWARNING);
        }

        internal static bool MoveToRecycleBin(string path)
        {
            return Send(path, FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_NOERRORUI | FileOperationFlags.FOF_SILENT);
        }

        static bool deleteFile(string path, FileOperationFlags flags)
        {
            try
            {
                var fs = new SHFILEOPSTRUCT
                {
                    wFunc = FileOperationType.FO_DELETE,
                    pFrom = path + '\0' + '\0',
                    fFlags = flags
                };

                SHFileOperation(ref fs);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal static bool DeleteCompletelySilent(string path)
        {
            return deleteFile
            (
                path,
                FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_NOERRORUI |
                FileOperationFlags.FOF_SILENT
            );
        }

        #endregion

        #region Get Shortcut Target

        internal static ShellLinkObject GetShortcutObject(string shortcutFilename)
        {
            string pathOnly = Path.GetDirectoryName(shortcutFilename);
            string filenameOnly = GetShortFileName(shortcutFilename, true);

            Shell shell = new Shell();
            Folder folder = shell.NameSpace(pathOnly);
            FolderItem folderItem = folder.ParseName(filenameOnly);

            if (folderItem != null)
            {
                ShellLinkObject link = (ShellLinkObject) folderItem.GetLink;
                return link;
            }

            return null;
        }

        #endregion

        #region Drive related functions

        internal static string GetDriveType(DriveInfo driveInfo)
        {
            switch (driveInfo.DriveType)
            {
                case DriveType.CDRom:
                    return "CD-дисковод";
                case DriveType.Fixed:
                    return "Локальный диск";
                case DriveType.Network:
                    return "Сетевой диск";
                case DriveType.NoRootDirectory:
                    return "Диск без корневой папки";
                case DriveType.Ram:
                    return "ОЗУ";
                case DriveType.Removable:
                    return "Съёмный диск";
                case DriveType.Unknown:
                    return "Неизвестно";
                default:
                    return "Неизвестно";
            }
        }

        #endregion

        #region Path related functions

        internal static string GetDriveName(string path)
        {
            return Directory.GetDirectoryRoot(path);
        }

        internal static string GetShortDirectoryName(string path)
        {
            return Path.GetFileName(path);
        }

        internal static string GetShortFileName(string path, bool includeExtension)
        {
            if (includeExtension)
                return Path.GetFileName(path);
            else
                return Path.GetFileNameWithoutExtension(path);
        }

        internal static string GetExtension(string path)
        {
            string extension = Path.GetExtension(path);
            return extension.StartsWith(".") ? extension.Substring(1) : extension;
        }

        internal static List<string> SplitDirectoryPath(this string path)
        {
            var directoryPaths = new List<string>();

            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            if (directoryInfo.Parent != null)
                directoryPaths.AddRange(SplitDirectoryPath(directoryInfo.Parent.FullName));

            directoryPaths.Add(path);

            return directoryPaths;
        }

        internal static bool IsRootDirectory(string path)
        {
            return Path.GetPathRoot(path).Equals(path);
        }

        internal static string GetFreeNumberedFileName(string fileName)
        {
            if (!File.Exists(fileName))
                return fileName;

            string filePath = fileName.Substring(0, fileName.LastIndexOf(Path.DirectorySeparatorChar));
            string resultName;
            int number = 1;

            while (true)
            {
                resultName = Path.Combine
                (
                    filePath,
                    GetShortFileName(fileName, false) + string.Format(" ({0})", number) + Path.GetExtension(fileName)
                );

                if (!File.Exists(resultName))
                    return resultName;

                number++;
            }
        }

        internal static string GetFreeNumberedDirectoryName(string directoryName)
        {
            if (!Directory.Exists(directoryName))
                return directoryName;

            string directoryPath = directoryName.Substring(0, directoryName.LastIndexOf(Path.DirectorySeparatorChar));
            string resultName;
            int number = 1;

            while (true)
            {
                resultName = Path.Combine
                (
                    directoryPath,
                    GetShortDirectoryName(directoryName) + string.Format(" ({0})", number)
                );

                if (!Directory.Exists(resultName))
                    return resultName;

                number++;
            }
        }

        #endregion
    }
}

using FileOperationType = FileVisor.Models.Settings.FileOperationType;
using FileSystemEntryType = FileVisor.Models.FileSystemEntry.FileSystemEntryType;
using Path = System.IO.Path;

using FileVisor.Converters;
using FileVisor.Models;
using FileVisor.ViewModels;
using Shell32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using static FileVisor.CustomDialogBox.DialogBox;

namespace FileVisor
{
    public partial class MainWindow : Window
    {
        List<Window> childWindows = new List<Window>();

        MainViewModel viewModel;

        Settings settings;

        List<string> pathHistory = new List<string>();
        int currentPathHistoryIndex;

        StringCollection cutFileNames = new StringCollection();

        const string DIRECTORY_INFO_TEMPLATE = "Всего элементов: {0}";
        const string DIRECTORY_INFO_SELECTION_TEMPLATE = "Всего элементов: {0}  /  Выбрано элементов: {1}";
        const string STORAGE_INFO_TEMPLATE = "{0} ({1})  /  {2} занято  /  {3} свободно";

        List<CancellationTokenSource> cancellationTokenSources = new List<CancellationTokenSource>();

        bool firstTimeLoad = true;

        public MainWindow()
        {
            WriteToPathHistory(null);
            Init();
        }

        public MainWindow(string path)
        {
            WriteToPathHistory(path);
            Init();
        }

        void Init()
        {
            InitializeComponent();
            
            viewModel = new MainViewModel()
            {
                FileSystemEntries = new ObservableCollection<FileSystemEntry>(),
                TreeViewEntries = new ObservableCollection<FileSystemEntry>()
            };

            viewModel.EnableSynchronization();

            DataContext = viewModel;

            SetSettings();
            ApplySettings();

            PropertyDescriptor pd = DependencyPropertyDescriptor.FromProperty
            (
                DataGridColumn.WidthProperty,
                typeof(DataGridColumn)
            );

            foreach (DataGridColumn column in directoryContentDataGrid.Columns)
                pd.AddValueChanged(column, new EventHandler(directoryContentDataGrid_ColumnWidthChanged));

            UpdateCurrentPath();
            InitializeTreeView();

            titleTextBlock.MaxWidth = Width - 166;

            firstTimeLoad = false;
        }

        internal Settings GetSettings()
        {
            return settings;
        }

        void SetSettings()
        {
            foreach (Window openWindow in Application.Current.Windows)
            {
                if (openWindow is MainWindow window && window != this)
                {
                    settings = window.GetSettings();
                    break;
                }
            }

            if (settings is null)
                settings = SettingsManager.GetSettings();
        }

        internal void SetSettings(Settings settings)
        {
            if (settings is null)
                SetSettings();
            else
                this.settings = settings;
        }

        internal void ApplySettings()
        {
            Width = settings.WindowWidth;
            Height = settings.WindowHeight;
            WindowState = settings.StartMaximized ? WindowState.Maximized : WindowState.Normal;

            treeViewColumn.Width = new GridLength(settings.TreeViewWidth);

            nameDataGridColumn.Width = settings.NameColumnWidth;
            typeDataGridColumn.Width = settings.TypeColumnWidth;
            dateCreatedDataGridColumn.Width = settings.DateCreatedColumnWidth;
            dateModifiedDataGridColumn.Width = settings.DateModifiedColumnWidth;
            sizeDataGridColumn.Width = settings.SizeColumnWidth;
        }

        void InitializeMenuPanel()
        {
            int selectedItemCount = directoryContentDataGrid.SelectedItems.Count;

            bool hasSelectedDirectories = false;
            bool hasSelectedShortcuts = false;

            foreach (FileSystemEntry entry in directoryContentDataGrid.SelectedItems)
            {
                switch (entry.EntryType)
                {
                    case FileSystemEntryType.Directory:
                        hasSelectedDirectories = true;
                        break;
                    case FileSystemEntryType.Shortcut:
                        hasSelectedShortcuts = true;
                        break;
                }

                if (hasSelectedDirectories && hasSelectedShortcuts)
                    break;
            }

            createButton.IsEnabled = !string.IsNullOrEmpty(GetCurrentPath());
            openButton.IsEnabled = selectedItemCount > 0;
            editButton.IsEnabled = !string.IsNullOrEmpty(GetCurrentPath()) && selectedItemCount > 0 && !hasSelectedDirectories && !hasSelectedShortcuts;
            cutButton.IsEnabled = !string.IsNullOrEmpty(GetCurrentPath()) && selectedItemCount > 0;
            copyButton.IsEnabled = !string.IsNullOrEmpty(GetCurrentPath()) && selectedItemCount > 0;
            pasteButton.IsEnabled = !string.IsNullOrEmpty(GetCurrentPath()) && Clipboard.ContainsFileDropList();
            renameButton.IsEnabled = selectedItemCount == 1;
            deleteButton.IsEnabled = !string.IsNullOrEmpty(GetCurrentPath()) && selectedItemCount > 0;
            copyPathButton.IsEnabled = selectedItemCount > 0;

            if (openButton.IsEnabled)
            {
                FileSystemEntry firstSelectedEntry = directoryContentDataGrid.SelectedItem as FileSystemEntry;
                openButtonIcon.Source = firstSelectedEntry.Icon.ToImageSource();
            }
            else
            {
                openButtonIcon.Source = Application.Current.Resources["Logo"] as BitmapImage;
            }
        }

        void InitializeNavigationPanel()
        {
            goBackButton.IsEnabled = currentPathHistoryIndex > 0;
            goForwardButton.IsEnabled = currentPathHistoryIndex < pathHistory.Count - 1;
            goUpButton.IsEnabled = !string.IsNullOrEmpty(GetCurrentPath());

            if (GetCurrentPath() is null)
                pathTextBox.Text = FileSystemHelper.ROOT_PATH_NAME;
            else
                pathTextBox.Text = GetCurrentPath();
        }

        void InitializeTreeView()
        {
            viewModel.TreeViewEntries.Clear();

            FileSystemEntry rootEntry = FileSystemEntryFetcher.GetRootEntry();
            viewModel.TreeViewEntries.Add(rootEntry);

            FileSystemEntryFetcher.GetDrives(settings).ForEach
            (
                entry => rootEntry.ChildEntries.Add(entry)
            );

            FileSystemEntry systemDirectoriesEntry = FileSystemEntryFetcher.GetSystemDirectoriesEntry();
            viewModel.TreeViewEntries.Add(systemDirectoriesEntry);

            FileSystemEntryFetcher.GetSystemDirectories().ForEach
            (
                entry => systemDirectoriesEntry.ChildEntries.Add(entry)
            );
        }

        async void InitializeMainPanel(/*bool keepSelection = false*/)
        {
            /*
            List<int> selectedIndices = new List<int>();

            if (keepSelection)
            {
                var selectedItems = directoryContentDataGrid.SelectedItems;

                foreach (FileSystemEntry entry in selectedItems)
                    selectedIndices.Add(selectedItems.IndexOf(entry));
            }
            */

            RemoveSelection();

            foreach (FileSystemEntry entry in viewModel.FileSystemEntries)
                entry.Icon?.Dispose();

            viewModel.FileSystemEntries.Clear();

            string currentPath = GetCurrentPath();

            var tokenSource = new CancellationTokenSource();
            cancellationTokenSources.Add(tokenSource);

            CancellationToken ct = tokenSource.Token;

            if (currentPath is null)
            {
                dateCreatedDataGridColumn.Visibility = Visibility.Collapsed;
                dateModifiedDataGridColumn.Visibility = Visibility.Collapsed;

                await Task.Run(() =>
                {
                    if (firstTimeLoad)
                        Thread.Sleep(100);

                    if (ct.IsCancellationRequested)
                        return;

                    FileSystemEntryFetcher.GetDrives(settings).ForEach(entry =>
                    {
                        if (ct.IsCancellationRequested)
                            return;

                        viewModel.FileSystemEntries.Add(entry);
                    });
                }, tokenSource.Token);

                cancellationTokenSources.Remove(tokenSource);
                tokenSource.Dispose();
            }
            else
            {
                dateCreatedDataGridColumn.Visibility = Visibility.Visible;
                dateModifiedDataGridColumn.Visibility = Visibility.Visible;

                await Task.Run(() =>
                {
                    if (firstTimeLoad)
                        Thread.Sleep(100);

                    if (ct.IsCancellationRequested)
                        return;

                    FileSystemEntryFetcher.GetDirectories(currentPath, settings).ForEach(entry =>
                    {
                        if (ct.IsCancellationRequested)
                            return;

                        viewModel.FileSystemEntries.Add(entry);
                    });

                    if (ct.IsCancellationRequested)
                        return;

                    FileSystemEntryFetcher.GetFiles(currentPath, settings).ForEach(entry =>
                    {
                        if (ct.IsCancellationRequested)
                            return;

                        viewModel.FileSystemEntries.Add(entry);
                    });
                }, tokenSource.Token);

                cancellationTokenSources.Remove(tokenSource);
                tokenSource.Dispose();
            }

            emptyDirectoryTextBlock.Visibility = viewModel.FileSystemEntries.Count > 0 ?
                                                 Visibility.Collapsed :
                                                 Visibility.Visible;

            if (Settings.CurrentFileOperationType is FileOperationType.Cut && Clipboard.ContainsFileDropList())
                cutFileNames = Clipboard.GetFileDropList();

            foreach (string fileName in cutFileNames)
            {
                var matchingEntries = viewModel.FileSystemEntries.Where(entry => entry.FullName.Equals(fileName));

                if (matchingEntries.Count() > 0)
                    matchingEntries.First().IsCut = true;
            }

            InitializeMenuPanel();
            InitializeStatusBar();

            /*
            if (keepSelection)
            {
                foreach (var item in directoryContentDataGrid.Items)
                {
                    DataGridRow row = directoryContentDataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                    row.IsSelected = selectedIndices.Contains(directoryContentDataGrid.Items.IndexOf(item));
                }
            }
            */
        }

        void InitializeStatusBar()
        {
            string currentPath = GetCurrentPath();

            int itemCount = viewModel.FileSystemEntries.Count;
            int selectedItemCount = directoryContentDataGrid.SelectedItems.Count;

            if (currentPath is null)
            {
                if (selectedItemCount > 0)
                    directoryInfoTextBlock.Text = string.Format
                    (
                        DIRECTORY_INFO_SELECTION_TEMPLATE,
                        itemCount, selectedItemCount
                    );
                else
                    directoryInfoTextBlock.Text = string.Format
                    (
                        DIRECTORY_INFO_TEMPLATE,
                        itemCount
                    );

                storageInfoTextBlock.Text = "";
            }
            else
            {
                if (selectedItemCount > 0)
                    directoryInfoTextBlock.Text = string.Format
                    (
                        DIRECTORY_INFO_SELECTION_TEMPLATE,
                        itemCount, selectedItemCount
                    );
                else
                    directoryInfoTextBlock.Text = string.Format
                    (
                        DIRECTORY_INFO_TEMPLATE,
                        itemCount
                    );

                DriveInfo driveInfo = new DriveInfo(FileSystemHelper.GetDriveName(currentPath));

                long availableSpaceBytes = driveInfo.AvailableFreeSpace;
                long usedSpaceBytes = driveInfo.TotalSize - availableSpaceBytes;

                string volumeLabel = string.IsNullOrEmpty(driveInfo.VolumeLabel) ?
                                     FileSystemHelper.GetDriveType(driveInfo) :
                                     driveInfo.VolumeLabel;

                string driveLetter = driveInfo.Name.Substring(0, 2);
                string usedSpace = UnitConverter.BytesToReadableSize(usedSpaceBytes);
                string availableSpace = UnitConverter.BytesToReadableSize(availableSpaceBytes);

                storageInfoTextBlock.Text = string.Format
                (
                    STORAGE_INFO_TEMPLATE,
                    volumeLabel, driveLetter, usedSpace, availableSpace
                );
            }
        }

        void mainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            settings.WindowWidth = Width;
            settings.WindowHeight = Height;
        }

        void mainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState is WindowState.Maximized)
            {
                maximizeButton.Visibility = Visibility.Collapsed;
                restoreButton.Visibility = Visibility.Visible;

                settings.StartMaximized = true;
            }
            else if (WindowState is WindowState.Normal)
            {
                maximizeButton.Visibility = Visibility.Visible;
                restoreButton.Visibility = Visibility.Collapsed;

                settings.StartMaximized = false;
            }
        }

        void mainWindow_Closing(object sender, CancelEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
                if (childWindows.Contains(window))
                    window.Close();

            if (!Application.Current.Windows.Cast<Window>().Any(w => w is MainWindow && w != this))
            {
                if (Clipboard.ContainsFileDropList())
                    Clipboard.Clear();

                SettingsManager.SaveSettings(settings);
            }
        }

        void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        void maximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }

        void restoreButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
        }

        void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        void WriteToPathHistory(string path)
        {
            pathHistory.Add(path);
            currentPathHistoryIndex = pathHistory.Count - 1;
        }

        internal string GetCurrentPath()
        {
            return pathHistory[currentPathHistoryIndex];
        }

        internal void UpdateCurrentPath()
        {
            cancellationTokenSources.ForEach(cts => cts.Cancel());

            InitializeNavigationPanel();
            InitializeMainPanel();
            InitializeMenuPanel();
            InitializeStatusBar();
        }

        internal void UpdateCurrentPathForAll()
        {
            foreach (Window openWindow in Application.Current.Windows)
                if (openWindow is MainWindow window)
                    window.UpdateCurrentPath();
        }

        void GoToDirectory(string path)
        {
            if (string.IsNullOrEmpty(path) && string.IsNullOrEmpty(GetCurrentPath()))
                return;
            
            if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(GetCurrentPath()) && path.Equals(GetCurrentPath()))
                return;

            if (currentPathHistoryIndex != pathHistory.Count - 1)
                pathHistory.RemoveRange(currentPathHistoryIndex + 1, pathHistory.Count - (currentPathHistoryIndex + 1));

            WriteToPathHistory(path);
            UpdateCurrentPath();
        }

        void OpenNewWindow(string path)
        {
            new MainWindow(path).Show();
        }

        void OpenNewWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenNewWindow(GetCurrentPath());
        }

        void OpenFile(string path)
        {
            try
            {
                Process.Start(path);
            }
            catch (Win32Exception ex)
            {
                ShowDialogBox(ex.Message, null, DialogBoxType.Error, DialogBoxButtons.OK);
            }
        }

        void OpenFile(string path, string verb)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(path) { Verb = verb };
            Process process = new Process { StartInfo = startInfo };
            
            try
            {
                process.Start();
            }
            catch (Win32Exception ex)
            {
                ShowDialogBox(ex.Message, null, DialogBoxType.Error, DialogBoxButtons.OK);
            }
        }

        void OpenFileSystemEntry(FileSystemEntry entry)
        {
            switch (entry.EntryType)
            {
                case FileSystemEntryType.Root:
                case FileSystemEntryType.Drive:
                case FileSystemEntryType.Directory:
                    GoToDirectory(entry.FullName);
                    break;
                case FileSystemEntryType.File:
                    OpenFile(entry.FullName);
                    break;
                case FileSystemEntryType.Shortcut:
                    ShellLinkObject shortcutObject = FileSystemHelper.GetShortcutObject(entry.FullName);
                    if (Directory.Exists(shortcutObject.Path))
                        GoToDirectory(shortcutObject.Path);
                    else
                        OpenFile(entry.FullName);
                    break;
            }
        }

        void OpenFileSystemEntry(FileSystemEntry entry, bool inNewWindow)
        {
            switch (entry.EntryType)
            {
                case FileSystemEntryType.Root:
                case FileSystemEntryType.Drive:
                case FileSystemEntryType.Directory:
                    if (inNewWindow)
                        OpenNewWindow(entry.FullName);
                    else
                        GoToDirectory(entry.FullName);
                    break;
                case FileSystemEntryType.File:
                    OpenFile(entry.FullName);
                    break;
                case FileSystemEntryType.Shortcut:
                    ShellLinkObject shortcutObject = FileSystemHelper.GetShortcutObject(entry.FullName);
                    if (Directory.Exists(shortcutObject.Path))
                    {
                        if (inNewWindow)
                            OpenNewWindow(shortcutObject.Path);
                        else
                            GoToDirectory(shortcutObject.Path);
                    }
                    else
                    {
                        OpenFile(entry.FullName);
                    }
                    break;
            }
        }

        void Create()
        {
            if (string.IsNullOrEmpty(GetCurrentPath()))
                return;

            CreateWindow createWindow = new CreateWindow(this);
            createWindow.Show();

            childWindows.Add(createWindow);
        }

        void createButton_Click(object sender, RoutedEventArgs e)
        {
            Create();
        }

        void CreateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Create();
        }

        void Open()
        {
            int selectedItemCount = directoryContentDataGrid.SelectedItems.Count;

            if (selectedItemCount == 0)
                return;

            if (selectedItemCount > 1)
            {
                foreach (FileSystemEntry entry in directoryContentDataGrid.SelectedItems)
                    OpenFileSystemEntry(entry, true);
            }
            else
            {
                OpenFileSystemEntry(directoryContentDataGrid.SelectedItem as FileSystemEntry);
            }
        }

        void openButton_Click(object sender, RoutedEventArgs e)
        {
            Open();
        }

        void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Open();
        }

        void Edit()
        {
            int selectedItemCount = directoryContentDataGrid.SelectedItems.Count;

            bool hasSelectedDirectories = false;
            bool hasSelectedShortcuts = false;

            foreach (FileSystemEntry entry in directoryContentDataGrid.SelectedItems)
            {
                switch (entry.EntryType)
                {
                    case FileSystemEntryType.Directory:
                        hasSelectedDirectories = true;
                        break;
                    case FileSystemEntryType.Shortcut:
                        hasSelectedShortcuts = true;
                        break;
                }

                if (hasSelectedDirectories && hasSelectedShortcuts)
                    break;
            }

            if (string.IsNullOrEmpty(GetCurrentPath()) || selectedItemCount == 0 || hasSelectedDirectories || hasSelectedShortcuts)
                return;

            foreach (FileSystemEntry entry in directoryContentDataGrid.SelectedItems)
                OpenFile(entry.FullName, "Edit");
        }


        void editButton_Click(object sender, RoutedEventArgs e)
        {
            Edit();
        }

        void EditCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Edit();
        }

        void OpenProperties()
        {
            if (directoryContentDataGrid.SelectedItems.Count == 0)
            {
                string currentPath = GetCurrentPath();

                if (string.IsNullOrEmpty(currentPath))
                {
                    if (settings.OldSystemPropertiesView)
                        Process.Start("shell:::{bb06c0e4-d293-4f75-8a90-cb05b6477eee}");
                    else
                        Process.Start("ms-settings:about");
                }
                else
                {
                    if (FileSystemHelper.IsRootDirectory(currentPath))
                        new PropertiesDriveWindow(this, FileSystemEntryFetcher.GetDrive(new DriveInfo(GetCurrentPath()))).Show();
                    else
                        new PropertiesDirectoryWindow(this, FileSystemEntryFetcher.GetDirectory(currentPath, settings)).Show();
                }
            }
            else
            {
                foreach (FileSystemEntry entry in directoryContentDataGrid.SelectedItems)
                {
                    switch (entry.EntryType)
                    {
                        case FileSystemEntryType.Root:
                            if (settings.OldSystemPropertiesView)
                                Process.Start("shell:::{bb06c0e4-d293-4f75-8a90-cb05b6477eee}");
                            else
                                Process.Start("ms-settings:about");
                            break;
                        case FileSystemEntryType.Drive:
                            new PropertiesDriveWindow(this, entry).Show();
                            break;
                        case FileSystemEntryType.Directory:
                            new PropertiesDirectoryWindow(this, entry).Show();
                            break;
                        case FileSystemEntryType.File:
                            new PropertiesFileWindow(this, entry).Show();
                            break;
                        case FileSystemEntryType.Shortcut:
                            new PropertiesShortcutWindow(this, entry).Show();
                            break;
                    }
                }
            }
        }

        void propertiesButton_Click(object sender, RoutedEventArgs e)
        {
            OpenProperties();
        }

        void OpenPropertiesCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenProperties();
        }

        void Cut()
        {
            int selectedItemCount = directoryContentDataGrid.SelectedItems.Count;

            if (string.IsNullOrEmpty(GetCurrentPath()) || selectedItemCount == 0)
                return;

            StringCollection fileNames = new StringCollection();

            foreach (FileSystemEntry entry in directoryContentDataGrid.SelectedItems)
                fileNames.Add(entry.FullName);

            Clipboard.SetFileDropList(fileNames);
            cutFileNames = fileNames;

            Settings.CurrentFileOperationType = FileOperationType.Cut;

            //InitializeMenuPanel();
            //InitializeMainPanel(true);
            UpdateCurrentPathForAll();
        }

        void cutButton_Click(object sender, RoutedEventArgs e)
        {
            Cut();
        }

        void CutCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Cut();
        }

        void Copy()
        {
            int selectedItemCount = directoryContentDataGrid.SelectedItems.Count;

            if (string.IsNullOrEmpty(GetCurrentPath()) || selectedItemCount == 0)
                return;

            StringCollection fileNames = new StringCollection();

            foreach (FileSystemEntry entry in directoryContentDataGrid.SelectedItems)
                fileNames.Add(entry.FullName);

            Clipboard.SetFileDropList(fileNames);

            foreach (string fileName in cutFileNames)
            {
                var matchingEntries = viewModel.FileSystemEntries.Where(entry => entry.FullName.Equals(fileName));

                if (matchingEntries.Count() > 0)
                    matchingEntries.First().IsCut = false;
            }

            cutFileNames.Clear();

            Settings.CurrentFileOperationType = FileOperationType.Copy;

            //InitializeMenuPanel();
            UpdateCurrentPathForAll();
        }

        void copyButton_Click(object sender, RoutedEventArgs e)
        {
            Copy();
        }

        void CopyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Copy();
        }

        bool CopyDirectoryTo(string sourceName, string destinationName)
        {
            if (Directory.Exists(destinationName))
            {
                DialogBoxResult dialogBoxResult = ShowDialogBox(string.Format("В папке назначения уже есть папка \"{0}\". Хотите слить копируемую папку с существующей?", FileSystemHelper.GetShortDirectoryName(destinationName)), "Слияние папок", DialogBoxType.Question, DialogBoxButtons.YesNo);
                if (dialogBoxResult is DialogBoxResult.No)
                    return false;
            }
            else
            {
                Directory.CreateDirectory(destinationName);
            }

            DirectoryInfo sourceDirectory = new DirectoryInfo(sourceName);

            foreach (FileInfo file in sourceDirectory.GetFiles())
            {
                string targetFileName = Path.Combine(destinationName, file.Name);

                if (File.Exists(targetFileName))
                {
                    DialogBoxResult dialogBoxResult = ShowDialogBox(string.Format("В папке назначения уже есть файл \"{0}\". Хотите заменить его?", FileSystemHelper.GetShortFileName(targetFileName, true)), "Замена файлов", DialogBoxType.Question, DialogBoxButtons.YesNo);
                    if (dialogBoxResult is DialogBoxResult.No)
                        continue;
                }

                file.CopyTo(targetFileName, true);
            }

            foreach (DirectoryInfo subDirectory in sourceDirectory.GetDirectories())
            {
                string newDestinationName = Path.Combine(destinationName, subDirectory.Name);
                CopyDirectoryTo(subDirectory.FullName, newDestinationName);
            }

            return true;
        }

        bool MoveDirectoryTo(string sourceName, string destinationName)
        {
            if (Directory.Exists(destinationName))
            {
                DialogBoxResult dialogBoxResult = ShowDialogBox(string.Format("В папке назначения уже есть папка \"{0}\". Хотите слить перемещаемую папку с существующей?", FileSystemHelper.GetShortDirectoryName(destinationName)), "Слияние папок", DialogBoxType.Question, DialogBoxButtons.YesNo);
                if (dialogBoxResult is DialogBoxResult.No)
                    return false;
            }
            else
            {
                Directory.CreateDirectory(destinationName);
            }

            DirectoryInfo sourceDirectory = new DirectoryInfo(sourceName);

            foreach (FileInfo fileInfo in sourceDirectory.GetFiles())
            {
                string targetFileName = Path.Combine(destinationName, fileInfo.Name);

                if (File.Exists(targetFileName))
                {
                    DialogBoxResult dialogBoxResult = ShowDialogBox(string.Format("В папке назначения уже есть файл \"{0}\". Хотите заменить его?", FileSystemHelper.GetShortFileName(targetFileName, true)), "Замена файлов", DialogBoxType.Question, DialogBoxButtons.YesNo);
                    if (dialogBoxResult is DialogBoxResult.No)
                        continue;
                }

                File.Delete(targetFileName);
                fileInfo.MoveTo(targetFileName);
            }

            foreach (DirectoryInfo subDirectory in sourceDirectory.GetDirectories())
            {
                string newDestinationName = Path.Combine(destinationName, subDirectory.Name);
                MoveDirectoryTo(subDirectory.FullName, newDestinationName);
            }

            if (Directory.GetFileSystemEntries(sourceName).Length == 0)
                Directory.Delete(sourceName, false);

            return true;
        }

        void Paste()
        {
            if (string.IsNullOrEmpty(GetCurrentPath()) || !Clipboard.ContainsFileDropList())
                return;

            if (Settings.CurrentFileOperationType is FileOperationType.None)
                return;

            StringCollection fileNames = Clipboard.GetFileDropList();
            StringCollection pastedFileNames = new StringCollection();

            foreach (string sourceName in fileNames)
            {
                string destinationName = Path.Combine(GetCurrentPath(), FileSystemHelper.GetShortFileName(sourceName, true));

                if (File.Exists(sourceName))
                {
                    if (sourceName.Equals(destinationName))
                    {
                        switch (Settings.CurrentFileOperationType)
                        {
                            case FileOperationType.Copy:
                                destinationName = FileSystemHelper.GetFreeNumberedFileName(destinationName);
                                break;
                            case FileOperationType.Cut:
                                ShowDialogBox(string.Format("Не удалось переместить файл \"{0}\" - исходное и конечное расположение совпадают.", FileSystemHelper.GetShortFileName(destinationName, true)), null, DialogBoxType.Error, DialogBoxButtons.OK);
                                continue;
                        }
                    }

                    if (File.Exists(destinationName))
                    {
                        DialogBoxResult dialogBoxResult = ShowDialogBox(string.Format("В папке назначения уже есть файл \"{0}\". Хотите заменить его?", FileSystemHelper.GetShortFileName(destinationName, true)), "Замена файлов", DialogBoxType.Question, DialogBoxButtons.YesNo);
                        if (dialogBoxResult is DialogBoxResult.No)
                            continue;
                    }

                    FileInfo fileInfo = new FileInfo(sourceName);

                    switch (Settings.CurrentFileOperationType)
                    {
                        case FileOperationType.Cut:
                            try
                            {
                                File.Delete(destinationName);
                                fileInfo.MoveTo(destinationName);
                                pastedFileNames.Add(sourceName);
                            }
                            catch (Exception ex)
                            {
                                ShowDialogBox(ex.Message, null, DialogBoxType.Error, DialogBoxButtons.OK);
                            }
                            break;
                        case FileOperationType.Copy:
                            try
                            {
                                fileInfo.CopyTo(destinationName, true);
                                pastedFileNames.Add(sourceName);
                            }
                            catch (Exception ex)
                            {
                                ShowDialogBox(ex.Message, null, DialogBoxType.Error, DialogBoxButtons.OK);
                            }
                            break;
                    }
                }
                else if (Directory.Exists(sourceName))
                {
                    if (sourceName.Equals(destinationName))
                    {
                        switch (Settings.CurrentFileOperationType)
                        {
                            case FileOperationType.Copy:
                                destinationName = FileSystemHelper.GetFreeNumberedDirectoryName(destinationName);
                                break;
                            case FileOperationType.Cut:
                                ShowDialogBox(string.Format("Не удалось переместить папку \"{0}\" - исходное и конечное расположение совпадают.", FileSystemHelper.GetShortDirectoryName(destinationName)), null, DialogBoxType.Error, DialogBoxButtons.OK);
                                continue;
                        }
                    }

                    switch (Settings.CurrentFileOperationType)
                    {
                        case FileOperationType.Cut:
                            try
                            {
                                if (MoveDirectoryTo(sourceName, destinationName))
                                    pastedFileNames.Add(sourceName);
                            }
                            catch (Exception ex)
                            {
                                ShowDialogBox(ex.Message, null, DialogBoxType.Error, DialogBoxButtons.OK);
                            }
                            break;
                        case FileOperationType.Copy:
                            try
                            {
                                if (CopyDirectoryTo(sourceName, destinationName))
                                    pastedFileNames.Add(sourceName);
                            }
                            catch (Exception ex)
                            {
                                ShowDialogBox(ex.Message, null, DialogBoxType.Error, DialogBoxButtons.OK);
                            }
                            break;
                    }
                }
                else
                {
                    ShowDialogBox(string.Format("Объект \"{0}\" не найден в исходной папке.", sourceName), null, DialogBoxType.Error, DialogBoxButtons.OK);
                }
            }

            if (Settings.CurrentFileOperationType is FileOperationType.Cut)
            {
                StringCollection notPastedFileNames = new StringCollection();

                foreach (string fileName in fileNames)
                    if (!pastedFileNames.Contains(fileName))
                        notPastedFileNames.Add(fileName);

                if (notPastedFileNames.Count > 0)
                {
                    Clipboard.SetFileDropList(notPastedFileNames);
                    cutFileNames = notPastedFileNames;
                }
                else
                {
                    Clipboard.Clear();
                    cutFileNames.Clear();
                    Settings.CurrentFileOperationType = FileOperationType.None;
                }
            }

            UpdateCurrentPathForAll();
        }

        void pasteButton_Click(object sender, RoutedEventArgs e)
        {
            Paste();
        }

        void PasteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paste();
        }

        void Rename()
        {
            int selectedItemCount = directoryContentDataGrid.SelectedItems.Count;

            if (selectedItemCount != 1)
                return;

            FileSystemEntry entry = directoryContentDataGrid.SelectedItem as FileSystemEntry;

            switch (entry.EntryType)
            {
                case FileSystemEntryType.Drive:
                    foreach (DriveInfo driveInfo in DriveInfo.GetDrives())
                    {
                        if (driveInfo.Name.Equals(entry.FullName))
                        {
                            RenameDriveWindow renameDriveWindow = new RenameDriveWindow(this, driveInfo);
                            renameDriveWindow.Show();
                            childWindows.Add(renameDriveWindow);
                        }
                    }
                    break;
                case FileSystemEntryType.Directory:
                    RenameDirectoryWindow renameDirectoryWindow = new RenameDirectoryWindow(this, entry.FullName);
                    renameDirectoryWindow.Show();
                    childWindows.Add(renameDirectoryWindow);
                    break;
                case FileSystemEntryType.File:
                    RenameFileWindow renameFileWindow = new RenameFileWindow(this, entry.FullName);
                    renameFileWindow.Show();
                    childWindows.Add(renameFileWindow);
                    break;
                case FileSystemEntryType.Shortcut:
                    RenameShortcutWindow renameShortcutWindow = new RenameShortcutWindow(this, entry.FullName);
                    renameShortcutWindow.Show();
                    childWindows.Add(renameShortcutWindow);
                    break;
                default:
                    ShowDialogBox("Невозможно переименовать выбранный объект.", null, DialogBoxType.Error, DialogBoxButtons.OK);
                    break;
            }
        }

        void renameButton_Click(object sender, RoutedEventArgs e)
        {
            Rename();
        }

        void RenameCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Rename();
        }

        void MoveToRecycleBin(IEnumerable<FileSystemEntry> entries, string objectsToDelete)
        {
            DialogBoxResult dialogBoxResult = ShowDialogBox(string.Format("Вы действительно хотите переместить {0} в корзину?", objectsToDelete), "Удаление", DialogBoxType.Question, DialogBoxButtons.YesNo);
            if (dialogBoxResult is DialogBoxResult.No)
                return;

            foreach (FileSystemEntry entry in entries)
                FileSystemHelper.MoveToRecycleBin(entry.FullName);

            UpdateCurrentPathForAll();
        }

        void DeletePermanently(IEnumerable<FileSystemEntry> entries, string objectsToDelete)
        {
            DialogBoxResult dialogBoxResult = ShowDialogBox(string.Format("Вы действительно хотите безвозвратно удалить {0}?", objectsToDelete), "Удаление", DialogBoxType.Question, DialogBoxButtons.YesNo);
            if (dialogBoxResult is DialogBoxResult.No)
                return;

            foreach (FileSystemEntry entry in entries)
                FileSystemHelper.DeleteCompletelySilent(entry.FullName);

            UpdateCurrentPathForAll();
        }

        void Delete(bool permanently)
        {
            int selectedItemCount = directoryContentDataGrid.SelectedItems.Count;

            if (string.IsNullOrEmpty(GetCurrentPath()) || selectedItemCount == 0)
                return;

            IEnumerable<FileSystemEntry> selectedEntries = directoryContentDataGrid.SelectedItems.Cast<FileSystemEntry>();

            string objectsToDelete = selectedEntries.Count() > 1 ?
                                     string.Format("эти объекты ({0} шт.)", selectedEntries.Count()) :
                                     selectedEntries.First().EntryType is FileSystemEntryType.Directory ?
                                     "эту папку" :
                                     selectedEntries.First().EntryType is FileSystemEntryType.File ?
                                     "этот файл" :
                                     selectedEntries.First().EntryType is FileSystemEntryType.Shortcut ?
                                     "этот ярлык" :
                                     "этот объект";

            if (permanently)
                DeletePermanently(selectedEntries, objectsToDelete);
            else
                MoveToRecycleBin(selectedEntries, objectsToDelete);
        }

        void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            Delete(false);
        }

        void DeleteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Delete(false);
        }

        void DeletePermanentlyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Delete(true);
        }

        void copyPathButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> paths = new List<string>();
            foreach (FileSystemEntry entry in directoryContentDataGrid.SelectedItems)
                paths.Add("\"" + entry.FullName + "\"");

            string toCopy = string.Join(Environment.NewLine, paths);

            Clipboard.SetText(toCopy);
        }

        void SelectAll()
        {
            directoryContentDataGrid.SelectAll();
        }

        void selectAllButton_Click(object sender, RoutedEventArgs e)
        {
            SelectAll();
        }

        void SelectAllCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectAll();
        }

        void RemoveSelection()
        {
            directoryContentDataGrid.UnselectAll();
        }

        void removeSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelection();
        }

        void RemoveSelectionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RemoveSelection();
        }

        void invertSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in directoryContentDataGrid.Items)
                if (directoryContentDataGrid.ItemContainerGenerator.ContainerFromItem(item) is DataGridRow row)
                    row.IsSelected = !row.IsSelected;
        }

        void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            new UserSettingsWindow(this, settings).Show();
        }

        void GoBack()
        {
            if (currentPathHistoryIndex == 0)
                return;

            currentPathHistoryIndex--;
            UpdateCurrentPath();
        }

        void goBackButton_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }

        void GoBackCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GoBack();
        }

        void GoForward()
        {
            if (currentPathHistoryIndex == pathHistory.Count - 1)
                return;

            currentPathHistoryIndex++;
            UpdateCurrentPath();
        }

        void goForwardButton_Click(object sender, RoutedEventArgs e)
        {
            GoForward();
        }

        void GoForwardCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GoForward();
        }

        void GoUp()
        {
            string currentPath = GetCurrentPath();

            if (string.IsNullOrEmpty(currentPath))
                return;

            string currentDirectoryRoot = Directory.GetDirectoryRoot(currentPath);

            if (currentPath.Equals(currentDirectoryRoot))
                GoToDirectory(null);
            else
                GoToDirectory(Directory.GetParent(GetCurrentPath()).FullName);
        }

        void goUpButton_Click(object sender, RoutedEventArgs e)
        {
            GoUp();
        }

        void GoUpCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GoUp();
        }

        void GoHome()
        {
            GoToDirectory(null);
        }

        void goHomeButton_Click(object sender, RoutedEventArgs e)
        {
            GoHome();
        }

        void GoHomeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GoHome();
        }

        void RemoveFocus(FrameworkElement element)
        {
            FrameworkElement parent = (FrameworkElement) element.Parent;
            while (parent != null && parent is IInputElement inputElement && !(inputElement.Focusable))
                parent = (FrameworkElement) parent.Parent;

            DependencyObject scope = FocusManager.GetFocusScope(element);
            FocusManager.SetFocusedElement(scope, parent);
        }

        void TryGoToPath(string path)
        {
            string targetPath;
            string rootPathName = FileSystemHelper.ROOT_PATH_NAME;

            if (path.Equals(rootPathName) ||
                path.Equals(rootPathName + Path.DirectorySeparatorChar) ||
                path.Equals(rootPathName + Path.AltDirectorySeparatorChar))
                targetPath = null;
            else if (path.StartsWith(rootPathName + Path.DirectorySeparatorChar) ||
                     path.StartsWith(rootPathName + Path.AltDirectorySeparatorChar))
                targetPath = path.Substring(rootPathName.Length + 1);
            else
                targetPath = path;

            if (targetPath != null)
            {
                if (!Directory.Exists(targetPath))
                {
                    ShowDialogBox("Указанный путь не найден. Проверьте правильность ввода и повторите попытку.", null, DialogBoxType.Error, DialogBoxButtons.OK);
                    return;
                }
            }

            GoToDirectory(targetPath);
            RemoveFocus(pathTextBox);

            refreshButton.Visibility = Visibility.Visible;
            goButton.Visibility = Visibility.Collapsed;
        }

        void pathTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Escape)
            {
                if (string.IsNullOrEmpty(GetCurrentPath()))
                    pathTextBox.Text = FileSystemHelper.ROOT_PATH_NAME;
                else
                    pathTextBox.Text = GetCurrentPath();

                RemoveFocus(pathTextBox);

                refreshButton.Visibility = Visibility.Visible;
                goButton.Visibility = Visibility.Collapsed;
            }
            else if (e.Key is Key.Enter)
            {
                TryGoToPath(pathTextBox.Text);
            }
            else
            {
                refreshButton.Visibility = Visibility.Collapsed;
                goButton.Visibility = Visibility.Visible;
            }
        }

        void EnterPath()
        {
            pathTextBox.Focus();
            pathTextBox.CaretIndex = pathTextBox.Text.Length;
        }

        void EnterPathCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            EnterPath();
        }

        void Refresh()
        {
            UpdateCurrentPath();
        }

        void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        void RefreshCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Refresh();
        }

        void goButton_Click(object sender, RoutedEventArgs e)
        {
            TryGoToPath(pathTextBox.Text);
        }

        void treeViewGridSplitter_DragDelta(object sender, DragDeltaEventArgs e)
        {
            settings.TreeViewWidth = treeViewColumn.Width.Value;
        }

        void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (treeView.SelectedItem is null)
                return;

            FileSystemEntry entry = treeView.SelectedItem as FileSystemEntry;

            if (entry.EntryType != FileSystemEntryType.Virtual)
                GoToDirectory(entry.FullName);
        }

        void treeView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (treeView.SelectedItem is null)
                return;

            FileSystemEntry entry = treeView.SelectedItem as FileSystemEntry;
            int selectedIndex = treeView.Items.IndexOf(entry);

            if (selectedIndex == -1)
            {
                foreach (var entry1 in treeView.Items)
                {
                    TreeViewItem item = treeView.ItemContainerGenerator.ContainerFromItem(entry1) as TreeViewItem;
                    selectedIndex = item.Items.IndexOf(entry);

                    if (selectedIndex != -1)
                    {
                        TreeViewItem subItem = item.ItemContainerGenerator.ContainerFromIndex(selectedIndex) as TreeViewItem;
                        subItem.IsSelected = false;
                    }
                }
            }
            else
            {
                TreeViewItem item = treeView.ItemContainerGenerator.ContainerFromIndex(selectedIndex) as TreeViewItem;
                item.IsSelected = false;
            }
        }

        void mainPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RemoveSelection();
        }

        void directoryContentDataGrid_ColumnWidthChanged(object sender, EventArgs e)
        {
            DataGridColumn column = sender as DataGridColumn;
            string header = column.Header.ToString();
            double width = column.Width.Value;

            if (header.Equals(nameDataGridColumn.Header))
                settings.NameColumnWidth = width;
            else if (header.Equals(typeDataGridColumn.Header))
                settings.TypeColumnWidth = width;
            else if (header.Equals(dateCreatedDataGridColumn.Header))
                settings.DateCreatedColumnWidth = width;
            else if (header.Equals(dateModifiedDataGridColumn.Header))
                settings.DateModifiedColumnWidth = width;
            else if (header.Equals(sizeDataGridColumn.Header))
                settings.SizeColumnWidth = width;
        }

        void directoryContentDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InitializeMenuPanel();
            InitializeStatusBar();
        }

        void directoryContentDataGrid_DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow clickedRow = sender as DataGridRow;
            FileSystemEntry entry = clickedRow.DataContext as FileSystemEntry;
            OpenFileSystemEntry(entry);
        }

        private void directoryContentDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter)
            {
                e.Handled = true;
                OpenCommand_Executed(sender, null);
            }
            else if (e.Key is Key.C &&
                     e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
            {
                e.Handled = true;
                CopyCommand_Executed(sender, null);
            }
        }
    }
}

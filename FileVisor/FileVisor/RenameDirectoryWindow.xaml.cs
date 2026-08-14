using System;
using System.IO;
using System.Windows;

using static FileVisor.CustomDialogBox.DialogBox;

namespace FileVisor
{
    public partial class RenameDirectoryWindow : Window
    {
        MainWindow mainWindow;
        string initialFullName;

        public RenameDirectoryWindow(MainWindow mainWindow, string initialFullName)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;
            this.initialFullName = initialFullName;

            Title = "Переименование: " + FileSystemHelper.GetShortDirectoryName(initialFullName);
            titleTextBlock.Text = Title;
            titleTextBlock.MaxWidth = Width - 166;

            nameTextBox.Text = FileSystemHelper.GetShortDirectoryName(initialFullName);

            nameTextBox.Focus();
            nameTextBox.CaretIndex = nameTextBox.Text.Length;
        }

        void renameDirectoryWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mainWindow.UpdateCurrentPathForAll();
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

        void RenameDirectory(string fullName)
        {
            if (Directory.Exists(fullName))
            {
                ShowDialogBox(string.Format("Папка {0} уже существует в этом расположении.", FileSystemHelper.GetShortDirectoryName(fullName)), null, DialogBoxType.Error, DialogBoxButtons.OK);
                return;
            }

            Directory.Move(initialFullName, fullName);
            FileSystemEntryFetcher.RemoveFromCache(initialFullName);
            Close();
        }

        void okButton_Click(object sender, RoutedEventArgs e)
        {
            string name = nameTextBox.Text;

            if (string.IsNullOrEmpty(name))
            {
                ShowDialogBox("Имя папки не может быть пустым", null, DialogBoxType.Error, DialogBoxButtons.OK);
                return;
            }

            try
            {
                RenameDirectory(Path.Combine(mainWindow.GetCurrentPath(), name));
            }
            catch (Exception ex)
            {
                ShowDialogBox(ex.Message, null, DialogBoxType.Error, DialogBoxButtons.OK);
            }
        }

        void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

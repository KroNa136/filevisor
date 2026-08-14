using System;
using System.IO;
using System.Windows;

using static FileVisor.CustomDialogBox.DialogBox;

namespace FileVisor
{
    public partial class RenameFileWindow : Window
    {
        MainWindow mainWindow;
        string initialFullName;

        public RenameFileWindow(MainWindow mainWindow, string initialFullName)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;
            this.initialFullName = initialFullName;

            Title = "Переименование: " + FileSystemHelper.GetShortFileName(initialFullName, true);
            titleTextBlock.Text = Title;
            titleTextBlock.MaxWidth = Width - 166;

            nameTextBox.Text = FileSystemHelper.GetShortFileName(initialFullName, false);
            extensionTextBox.Text = FileSystemHelper.GetExtension(initialFullName);

            nameTextBox.Focus();
            nameTextBox.CaretIndex = nameTextBox.Text.Length;
        }

        void renameFileWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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

        void RenameFile(string fullName)
        {
            if (File.Exists(fullName))
            {
                ShowDialogBox(string.Format("Файл {0} уже существует в этом расположении.", FileSystemHelper.GetShortFileName(fullName, true)), null, DialogBoxType.Error, DialogBoxButtons.OK);
                return;
            }

            File.Move(initialFullName, fullName);
            FileSystemEntryFetcher.RemoveFromCache(initialFullName);
            Close();
        }

        void okButton_Click(object sender, RoutedEventArgs e)
        {
            string name = nameTextBox.Text;
            string extension = extensionTextBox.Text;

            if (string.IsNullOrEmpty(name))
            {
                ShowDialogBox("Имя файла не может быть пустым.", null, DialogBoxType.Error, DialogBoxButtons.OK);
                return;
            }

            try
            {
                RenameFile(Path.Combine(mainWindow.GetCurrentPath(), name + "." + extension));
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

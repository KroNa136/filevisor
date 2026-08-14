using System;
using System.IO;
using System.Windows;

using static FileVisor.CustomDialogBox.DialogBox;

namespace FileVisor
{
    public partial class RenameDriveWindow : Window
    {
        MainWindow mainWindow;
        DriveInfo driveInfo;

        public RenameDriveWindow(MainWindow mainWindow, DriveInfo driveInfo)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;
            this.driveInfo = driveInfo;

            Title = "Переименование: " + driveInfo.VolumeLabel;
            titleTextBlock.Text = Title;
            titleTextBlock.MaxWidth = Width - 166;

            labelTextBox.Text = driveInfo.VolumeLabel;

            labelTextBox.Focus();
            labelTextBox.CaretIndex = labelTextBox.Text.Length;
        }

        void renameDriveWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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

        void okButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                driveInfo.VolumeLabel = labelTextBox.Text;
                FileSystemEntryFetcher.RemoveFromCache(driveInfo.Name);
                Close();
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

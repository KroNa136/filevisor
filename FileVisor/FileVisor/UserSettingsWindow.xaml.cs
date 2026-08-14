using FileVisor.Models;
using FileVisor.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace FileVisor
{
    public partial class UserSettingsWindow : Window
    {
        MainWindow mainWindow;

        UserSettingsViewModel viewModel;

        bool updateExtensions = false;

        public UserSettingsWindow(MainWindow mainWindow, Settings settings)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;

            viewModel = new UserSettingsViewModel(settings);
            DataContext = viewModel;

            applyButton.IsEnabled = false;

            titleTextBlock.MaxWidth = Width - 166;
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

        void Apply()
        {
            Settings settings = mainWindow.GetSettings();

            settings.SelectedSortColumn = viewModel.SelectedSortColumnID;
            settings.SelectedSortDirection = viewModel.SelectedSortDirectionID;
            settings.ShowFileExtensions = viewModel.ShowFileExtensions;
            settings.ShowHiddenElements = viewModel.ShowHiddenElements;
            settings.ShowSystemElements = viewModel.ShowSystemElements;
            settings.OldSystemPropertiesView = viewModel.OldSystemPropertiesView;

            if (updateExtensions)
            {
                if (viewModel.ShowFileExtensions)
                    FileSystemEntryFetcher.AddExtensionsToFilesInCache();
                else
                    FileSystemEntryFetcher.RemoveExtensionsFromFilesInCache();
            }

            mainWindow.SetSettings(settings);
            mainWindow.ApplySettings();
            mainWindow.UpdateCurrentPath();

            applyButton.IsEnabled = false;
        }

        void okButton_Click(object sender, RoutedEventArgs e)
        {
            Apply();
            Close();
        }

        void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        void applyButton_Click(object sender, RoutedEventArgs e)
        {
            Apply();
        }

        void sortColumnComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            applyButton.IsEnabled = true;
        }

        void sortDirectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            applyButton.IsEnabled = true;
        }

        void showFileExtensionsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            applyButton.IsEnabled = true;
            updateExtensions = true;
        }

        void showFileExtensionsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            applyButton.IsEnabled = true;
            updateExtensions = true;
        }

        void showHiddenElementsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            applyButton.IsEnabled = true;
        }

        void showHiddenElementsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            applyButton.IsEnabled = true;
        }

        void showSystemElementsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            applyButton.IsEnabled = true;
        }

        void showSystemElementsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            applyButton.IsEnabled = true;
        }

        void oldSystemPropertiesViewCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            applyButton.IsEnabled = true;
        }

        void oldSystemPropertiesViewCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            applyButton.IsEnabled = true;
        }
    }
}

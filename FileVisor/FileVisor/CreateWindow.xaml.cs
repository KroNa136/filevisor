using ElementType = FileVisor.Models.CreatedElementType.ElementType;

using FileVisor.Models;
using FileVisor.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using static FileVisor.CustomDialogBox.DialogBox;

namespace FileVisor
{
    public partial class CreateWindow : Window
    {
        MainWindow mainWindow;

        CreateViewModel viewModel;

        bool isAutoChangingExtension = false;

        public CreateWindow(MainWindow mainWindow)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;

            viewModel = new CreateViewModel();
            DataContext = viewModel;

            titleTextBlock.MaxWidth = Width - 166;

            nameTextBox.Focus();
        }

        void createWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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

        void typeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CreatedElementType selectedElementType = CreateViewModel.CreatedElementTypes.First
            (
                t => t.ID.Equals(viewModel.SelectedID)
            );

            string extension = selectedElementType.Extension;

            isAutoChangingExtension = true;

            if (extension is null)
                extensionTextBox.Text = "";
            else if (!extension.Equals(string.Empty))
                extensionTextBox.Text = extension;

            isAutoChangingExtension = false;
        }

        void extensionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isAutoChangingExtension)
                return;

            if (extensionTextBox.Text.StartsWith("."))
            {
                isAutoChangingExtension = true;

                int caretIndex = extensionTextBox.CaretIndex;
                extensionTextBox.Text = extensionTextBox.Text.Substring(1);
                extensionTextBox.CaretIndex = caretIndex - 1;

                isAutoChangingExtension = false;
            }

            typeComboBox.SelectedIndex = (int) ElementType.Other;

            foreach (CreatedElementType type in CreateViewModel.CreatedElementTypes)
            {
                if (type.Extension is null)
                    continue;

                if (type.Extension.Equals(extensionTextBox.Text))
                    typeComboBox.SelectedIndex = (int) type.ID;
            }
        }

        void CreateFile(string fullName)
        {
            if (File.Exists(fullName))
            {
                ShowDialogBox(string.Format("Файл {0} уже существует в этом расположении.", FileSystemHelper.GetShortFileName(fullName, true)), null, DialogBoxType.Error, DialogBoxButtons.OK);
                return;
            }
            
            File.Create(fullName);
            Close();
        }

        void CreateDirectory(string fullName)
        {
            if (Directory.Exists(fullName))
            {
                ShowDialogBox(string.Format("Папка {0} уже существует в этом расположении.", FileSystemHelper.GetShortDirectoryName(fullName)), null, DialogBoxType.Error, DialogBoxButtons.OK);
                return;
            }
            
            Directory.CreateDirectory(fullName);
            Close();
        }

        void okButton_Click(object sender, RoutedEventArgs e)
        {
            string name = nameTextBox.Text;
            string extension = extensionTextBox.Text;

            if (string.IsNullOrEmpty(name))
            {
                ShowDialogBox("Имя элемента не может быть пустым.", null, DialogBoxType.Error, DialogBoxButtons.OK);
                return;
            }

            if (extension.Equals("lnk"))
            {
                DialogBoxResult dialogBoxResult = ShowDialogBox("Создание ярлыков (файлов с расширением .lnk) не поддерживается в текущей версии программы. Создаваемый ярлык будет неработоспособным.", "Создание ярлыка", DialogBoxType.Warning, DialogBoxButtons.OKCancel);
                if (dialogBoxResult is DialogBoxResult.Cancel)
                    return;
            }

            CreatedElementType selectedElementType = CreateViewModel.CreatedElementTypes.First
            (
                t => t.ID.Equals(viewModel.SelectedID)
            );

            try
            {
                if (selectedElementType.Extension is null)
                    CreateDirectory(Path.Combine(mainWindow.GetCurrentPath(), name));
                else
                    CreateFile(Path.Combine(mainWindow.GetCurrentPath(), name + "." + extension));
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

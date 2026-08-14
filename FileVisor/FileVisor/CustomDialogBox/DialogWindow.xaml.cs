using System.Media;
using System.Windows;

using static FileVisor.CustomDialogBox.DialogBox;

namespace FileVisor.CustomDialogBox
{
    public partial class DialogWindow : Window
    {
        string text;
        string title;
        DialogBoxType type;
        DialogBoxButtons buttons;
        internal DialogBoxResult result = DialogBoxResult.None;

        public DialogWindow(string text, string title, DialogBoxType type, DialogBoxButtons buttons)
        {
            InitializeComponent();

            this.text = text;
            this.title = title;
            this.type = type;
            this.buttons = buttons;

            SetTitle();
            InitializeMainPanel();
            InitializeButtonPanel();
            SetDefaultResult();
        }

        void SetTitle()
        {
            if (title is null)
            {
                switch (type)
                {
                    case DialogBoxType.Message:
                        Title = "Сообщение";
                        break;
                    case DialogBoxType.Question:
                        Title = "Вопрос";
                        break;
                    case DialogBoxType.Warning:
                        Title = "Предупреждение";
                        break;
                    case DialogBoxType.Error:
                        Title = "Ошибка";
                        break;
                }
            }
            else
            {
                Title = title;
            }

            titleTextBlock.Text = Title;
            titleTextBlock.MaxWidth = Width - 166;
        }

        void InitializeMainPanel()
        {
            switch (type)
            {
                case DialogBoxType.Message:
                    messageMainPanel.Visibility = Visibility.Visible;
                    questionMainPanel.Visibility = Visibility.Collapsed;
                    warningMainPanel.Visibility = Visibility.Collapsed;
                    errorMainPanel.Visibility = Visibility.Collapsed;
                    messageMainPanel_textBlock.Text = text;
                    break;
                case DialogBoxType.Question:
                    messageMainPanel.Visibility = Visibility.Collapsed;
                    questionMainPanel.Visibility = Visibility.Visible;
                    warningMainPanel.Visibility = Visibility.Collapsed;
                    errorMainPanel.Visibility = Visibility.Collapsed;
                    questionMainPanel_textBlock.Text = text;
                    break;
                case DialogBoxType.Warning:
                    messageMainPanel.Visibility = Visibility.Collapsed;
                    questionMainPanel.Visibility = Visibility.Collapsed;
                    warningMainPanel.Visibility = Visibility.Visible;
                    errorMainPanel.Visibility = Visibility.Collapsed;
                    warningMainPanel_textBlock.Text = text;
                    break;
                case DialogBoxType.Error:
                    messageMainPanel.Visibility = Visibility.Collapsed;
                    questionMainPanel.Visibility = Visibility.Collapsed;
                    warningMainPanel.Visibility = Visibility.Collapsed;
                    errorMainPanel.Visibility = Visibility.Visible;
                    errorMainPanel_textBlock.Text = text;
                    break;
            }
        }

        void InitializeButtonPanel()
        {
            switch (buttons)
            {
                case DialogBoxButtons.OK:
                    okButtonPanel.Visibility = Visibility.Visible;
                    okCancelButtonPanel.Visibility = Visibility.Collapsed;
                    yesNoButtonPanel.Visibility = Visibility.Collapsed;
                    yesNoCancelButtonPanel.Visibility = Visibility.Collapsed;
                    okButtonPanel_okButton.Focus();
                    break;
                case DialogBoxButtons.OKCancel:
                    okButtonPanel.Visibility = Visibility.Collapsed;
                    okCancelButtonPanel.Visibility = Visibility.Visible;
                    yesNoButtonPanel.Visibility = Visibility.Collapsed;
                    yesNoCancelButtonPanel.Visibility = Visibility.Collapsed;
                    okCancelButtonPanel_okButton.Focus();
                    break;
                case DialogBoxButtons.YesNo:
                    okButtonPanel.Visibility = Visibility.Collapsed;
                    okCancelButtonPanel.Visibility = Visibility.Collapsed;
                    yesNoButtonPanel.Visibility = Visibility.Visible;
                    yesNoCancelButtonPanel.Visibility = Visibility.Collapsed;
                    yesNoButtonPanel_yesButton.Focus();
                    break;
                case DialogBoxButtons.YesNoCancel:
                    okButtonPanel.Visibility = Visibility.Collapsed;
                    okCancelButtonPanel.Visibility = Visibility.Collapsed;
                    yesNoButtonPanel.Visibility = Visibility.Collapsed;
                    yesNoCancelButtonPanel.Visibility = Visibility.Visible;
                    yesNoCancelButtonPanel_yesButton.Focus();
                    break;
            }
        }

        void SetDefaultResult()
        {
            switch (buttons)
            {
                case DialogBoxButtons.OK:
                    result = DialogBoxResult.None;
                    break;
                case DialogBoxButtons.OKCancel:
                    result = DialogBoxResult.Cancel;
                    break;
                case DialogBoxButtons.YesNo:
                    result = DialogBoxResult.No;
                    break;
                case DialogBoxButtons.YesNoCancel:
                    result = DialogBoxResult.Cancel;
                    break;
            }
        }

        void PlaySound()
        {
            switch (type)
            {
                case DialogBoxType.Message:
                    break;
                case DialogBoxType.Question:
                    SystemSounds.Exclamation.Play();
                    break;
                case DialogBoxType.Warning:
                    SystemSounds.Exclamation.Play();
                    break;
                case DialogBoxType.Error:
                    SystemSounds.Hand.Play();
                    break;
            }
        }

        void dialogWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
                PlaySound();
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
            result = DialogBoxResult.OK;
            Close();
        }

        void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            result = DialogBoxResult.Cancel;
            Close();
        }

        void yesButton_Click(object sender, RoutedEventArgs e)
        {
            result = DialogBoxResult.Yes;
            Close();
        }

        void noButton_Click(object sender, RoutedEventArgs e)
        {
            result = DialogBoxResult.No;
            Close();
        }
    }
}

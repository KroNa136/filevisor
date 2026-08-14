using FileVisor.Models;
using System.Windows;

namespace FileVisor
{
    public partial class PropertiesFileWindow : Window
    {
        FileProperties properties;

        public PropertiesFileWindow(Window parent, FileSystemEntry entry)
        {
            InitializeComponent();

            properties = new FileProperties(entry);
            DataContext = properties;

            var transform = PresentationSource.FromVisual(parent).CompositionTarget.TransformFromDevice;
            var mousePosition = System.Windows.Forms.Control.MousePosition;
            var realMousePosition = transform.Transform(new Point(mousePosition.X, mousePosition.Y));

            Left = realMousePosition.X;
            Top = realMousePosition.Y;

            Title = "Свойства: " + entry.Name;
            titleTextBlock.Text = Title;
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

        void okButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

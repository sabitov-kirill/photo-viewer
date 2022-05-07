using System;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

using muxc = Microsoft.UI.Xaml.Controls;

namespace PhotoViewer.Elements
{
    public sealed partial class MenuBar : UserControl
    {
        public string RootFolder = "C:\\Users\\";

        public int MaximizedWidth
        {
            get { return (int)GetValue(MaximizedWidthProperty); }
            set { SetValue(MaximizedWidthProperty, value); }
        }
        public static readonly DependencyProperty MaximizedWidthProperty =
            DependencyProperty.Register(
                "MaximizedWidth",
                typeof(int),
                typeof(MenuBar),
                new PropertyMetadata(300)
            );

        public int MinimizedWidth
        {
            get { return (int)GetValue(MinimizedWidthProperty); }
            set { SetValue(MinimizedWidthProperty, value); }
        }
        public static readonly DependencyProperty MinimizedWidthProperty =
            DependencyProperty.Register(
                "MinimizedWidth",
                typeof(int),
                typeof(MenuBar),
                new PropertyMetadata(58)
            );

        public Windows.UI.Xaml.GridLength GetGridLength(int Length)
        {
            return new Windows.UI.Xaml.GridLength(Length);
        }

        public double GetChengeRootFolderTextBoxSize(int Length)
        {
            return Length - 65;
        }

        public MenuBar()
        {
            this.InitializeComponent();
        }

        public event EventHandler SizeToggleClicked;
        public bool IsMenuBarFullWidth = true;

        public event EventHandler<FilesTreeView.FileTreeEventArgs> FileSelected;

        public event EventHandler<bool> SettingsToggled;

        private async void setPathFromTextBox(TextBox textBox)
        {
            string path = textBox.Text;
            path = path.Trim();
            if (path == "") return;
            path = path.Replace('/', Path.DirectorySeparatorChar);

            bool succes = await filesTreeView.PushRootFolder(path);
            if (!succes)
            {
                fielsTreeSearchError.IsOpen = true;
                return;
            }

            textBox.Text = "";
            fielsTreeSearchError.IsOpen = false;
        }

        private void menuBarTogleButton_Click(object sender, RoutedEventArgs e)
        {
            IsMenuBarFullWidth = !IsMenuBarFullWidth;
            settingToggle.IsChecked = false;
            settingToggle.Visibility = fielsTreeSearch.Visibility = filesTreeView.Visibility =
                IsMenuBarFullWidth ? Visibility.Visible : Visibility.Collapsed;

            Width = IsMenuBarFullWidth ? MaximizedWidth : MinimizedWidth;

            SizeToggleClicked?.Invoke(this, EventArgs.Empty);
        }
        
        private void rootFolderPath_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                setPathFromTextBox(sender as TextBox);
        }
        private void TextBox_LosingFocus(UIElement sender, LosingFocusEventArgs args)
        {
            setPathFromTextBox(sender as TextBox);
        }

        private async void filesTreeView_FilePreviewStarted(object sender, FilesTreeView.FileTreeEventArgs e)
        {
            if (e.FileItem != null && e.FileItem.Type == FilesTreeItem.StorageItemType.Photo)
            {
                StorageFile file = e.FileItem.FileCopy;

                using (Windows.Storage.Streams.IRandomAccessStream fileStream =
                    await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    if (filePreview.Width != double.NaN) 
                        bitmapImage.DecodePixelWidth = (int)filePreview.Width;
                    bitmapImage.SetSource(fileStream);

                    filePreview.Source = bitmapImage;
                }

                filePreview.Visibility = Visibility.Visible;
            }
        }
        private void filesTreeView_FilePreviewEnded(object sender, FilesTreeView.FileTreeEventArgs e)
        {
            filePreview.Visibility = Visibility.Collapsed;
        }
        private void filesTreeBackButton_Click(object sender, RoutedEventArgs e)
        {
            filesTreeView.PopRootFolder();
        }
        private void filesTreeView_FileSelected(object sender, FilesTreeView.FileTreeEventArgs e)
        {
            FileSelected?.Invoke(sender, e);
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            Windows.UI.Xaml.Controls.Primitives.ToggleButton toggleButton = sender as Windows.UI.Xaml.Controls.Primitives.ToggleButton;
            SettingsToggled?.Invoke(sender, (bool)toggleButton.IsChecked);
        }
    }
}

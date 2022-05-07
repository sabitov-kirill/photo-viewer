using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

using muxc = Microsoft.UI.Xaml.Controls;

namespace PhotoViewer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public Windows.UI.Xaml.GridLength GetGridLength(int Length)
        {
            return new Windows.UI.Xaml.GridLength(Length);
        }

        public Common.Binding<int> MaximizedWidth = new Common.Binding<int>();
        private int MinimizedWidth = 58;

        public MainPage()
        {
            this.InitializeComponent();
            MaximizedWidth.Content = 260;
        }

        private void filesMenuBar_OnSizeToggleClicked(object sender, EventArgs e)
        {
            settingsPanel.Visibility = Visibility.Collapsed;
            menuBarColumn.Width = filesMenuBar.IsMenuBarFullWidth ?
                new GridLength(MaximizedWidth.Content) : new GridLength(MinimizedWidth);
        }

        private async void filesMenuBar_FileSelected(object sender, Elements.FilesTreeView.FileTreeEventArgs e)
        {
            if (e.FileItem != null && e.FileItem.Type == Elements.FilesTreeItem.StorageItemType.Photo)
            {
                StorageFile file = e.FileItem.FileCopy;

                using (Windows.Storage.Streams.IRandomAccessStream fileStream =
                    await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    if (fileConent.Width != double.NaN)
                        bitmapImage.DecodePixelWidth = (int)fileConent.Width;
                    bitmapImage.SetSource(fileStream);

                    fileConent.Source = bitmapImage;
                }
            }
        }

        private void filesMenuBar_SettingsToggled(object sender, bool e)
        {
            settingsPanel.Visibility = e ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}

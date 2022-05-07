using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using muxc = Microsoft.UI.Xaml.Controls;

namespace PhotoViewer.Elements
{
    public class FilesTreeItem : Common.BindableBase
    {
        // Unchangable members
        public enum StorageItemType { Folder, Photo, Unknown }
        public StorageItemType Type { get; }
        public string Name { get; }
        public string Path { get; }

        public StorageFolder FolderCopy { get; }
        public StorageFile FileCopy { get; }

        // Bindable properties
        private ObservableCollection<FilesTreeItem> children;
        public ObservableCollection<FilesTreeItem> Children
        {
            get { return children == null ? (children = new ObservableCollection<FilesTreeItem>()) : children; }
            set { SetProperty(ref children, value); }
        }

        private StorageItemType GetTypeByMIME(string MIME)
        {
            string[] mimeSplited = MIME.Split('/');

            switch (mimeSplited[0])
            {
                case "image": return StorageItemType.Photo; ;
                default: return StorageItemType.Unknown;
            }
        }

        public FilesTreeItem()
        {
            Type = StorageItemType.Unknown;
            Name = "";
            Path = "";
            FolderCopy = null;
        }
        public FilesTreeItem(StorageFolder Folder, bool IsRoot = false)
        {
            Type = StorageItemType.Folder;
            Name = IsRoot ? Folder.Path + $" ({Folder.DisplayName})": Folder.DisplayName;
            Path = Folder.Path;
            FolderCopy = Folder;
        }
        public FilesTreeItem(StorageFile File)
        {
            Type = GetTypeByMIME(File.ContentType);
            Name = File.Name;
            Path = File.Path;
            FileCopy = File;
        }

        private bool WasExpanded;
        private bool isExpanded;
        public bool IsExpanded
        {
            get { return isExpanded; }
            set { SetProperty(ref isExpanded, value); }
        }
        public async Task ExpandFolder()
        {
            if (WasExpanded) return;

            IReadOnlyList<StorageFolder> folders = await FolderCopy.GetFoldersAsync();
            IReadOnlyList<StorageFile> files = await FolderCopy.GetFilesAsync();

            Children.Clear();
            foreach (StorageFolder folder in folders)
                Children.Add(new FilesTreeItem(folder));

            foreach (StorageFile file in files)
                Children.Add(new FilesTreeItem(file));

            WasExpanded = true;
        }
    }

    public class FilesTreeItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TreeViewFolderDataTemplate { get; set; }
        public DataTemplate TreeViewPhotoDataTemplate { get; set; }
        public DataTemplate TreeViewUnknownDataTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            FilesTreeItem filesTreeItem = (FilesTreeItem)item;
            switch (filesTreeItem.Type)
            {
                case FilesTreeItem.StorageItemType.Folder: return TreeViewFolderDataTemplate;
                case FilesTreeItem.StorageItemType.Photo: return TreeViewPhotoDataTemplate;
                case FilesTreeItem.StorageItemType.Unknown:
                default: return TreeViewUnknownDataTemplate;
            }
        }
    }

    public sealed partial class FilesTreeView : UserControl
    {
        private Stack<string> RootFilderPathHistory;

        private ObservableCollection<FilesTreeItem> FileTreeDataSource;
        public static readonly DependencyProperty RootFolderPathProperty =
            DependencyProperty.Register(
                "RootFolderPath",
                typeof(string),
                typeof(FilesTreeView),
                new PropertyMetadata("")
            );
        public string RootFolderPath
        {
            get { return (string)GetValue(RootFolderPathProperty); }
            set
            {
                SetValue(RootFolderPathProperty, value);
                _ = PushRootFolder(value);
            }
        }

        public FilesTreeView()
        {
            this.InitializeComponent();
            FileTreeDataSource = new ObservableCollection<FilesTreeItem>();
            RootFilderPathHistory = new Stack<string>();
        }

        private async Task<bool> SetRootFolder(string RootFolderPath)
        {
            if (RootFolderPath is null) return true;

            try
            {
                StorageFolder rootStorageFolder = await StorageFolder.GetFolderFromPathAsync(RootFolderPath);
                FilesTreeItem rootFolder = new FilesTreeItem(rootStorageFolder, true);
                rootFolder.IsExpanded = true;

                FileTreeDataSource.Clear();
                FileTreeDataSource.Add(rootFolder);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PushRootFolder(string RootFolderPath)
        {
            if (await SetRootFolder(RootFolderPath))
            {
                RootFilderPathHistory.Push(RootFolderPath);
                return true;
            }
            return false;
        }
        public void PopRootFolder()
        {
            if (RootFilderPathHistory.Count < 2) return;

            RootFilderPathHistory.Pop();
            _ = SetRootFolder(RootFilderPathHistory.First());
        }

        private async void filesTreeView_Expanding(muxc.TreeView sender, muxc.TreeViewExpandingEventArgs args)
        {
            FilesTreeItem filesTreeItem = args.Item as FilesTreeItem;
            await filesTreeItem.ExpandFolder();
        }

        public class FileTreeEventArgs : EventArgs
        {
            public FilesTreeItem FileItem;

            public FileTreeEventArgs(FilesTreeItem previewingItem)
            {
                FileItem = previewingItem;
            }
        }
        public event EventHandler<FileTreeEventArgs> FilePreviewStarted;
        public event EventHandler<FileTreeEventArgs> FilePreviewEnded;
        public event EventHandler<FileTreeEventArgs> FileSelected;

        private void filesTreeViewItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            muxc.TreeViewItem treeViewNode = (muxc.TreeViewItem)sender;
            FilePreviewStarted?.Invoke(this, new FileTreeEventArgs(treeViewNode.DataContext as FilesTreeItem));
        }

        private void filesTreeViewItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            muxc.TreeViewItem treeViewNode = (muxc.TreeViewItem)sender;
            FilePreviewEnded?.Invoke(this, new FileTreeEventArgs(treeViewNode.DataContext as FilesTreeItem));
        }

        private void filesTreeView_ItemInvoked(muxc.TreeView sender, muxc.TreeViewItemInvokedEventArgs args)
        {
            FilesTreeItem item = (FilesTreeItem)args.InvokedItem;
            this.FileSelected?.Invoke(this, new FileTreeEventArgs(item));
        }
    }
}
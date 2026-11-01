using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HD6Browser
{
    public partial class Browser : Form
    {
        private sealed record ParentNavigation(TreeNode Node);

        private List<HD6Entry>? _loadedEntries;
        private string? _datFilePath;
        private ImageList? _folderImageList;

        private const uint ShgfiIcon = 0x000000100;
        private const uint ShgfiSmallIcon = 0x000000001;
        private const uint ShgfiUseFileAttributes = 0x000000010;
        private const uint FileAttributeDirectory = 0x000000010;

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SHGetFileInfo(
            string pszPath,
            uint dwFileAttributes,
            out SHFILEINFO psfi,
            uint cbFileInfo,
            uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        public Browser()
        {
            InitializeComponent();
            SetupListViewColumns();
            SetupSystemIcons();
            UpdateExtractButtonState();

            // Wire up tree selection event if not set in Designer
            treeViewFiles.AfterSelect += treeViewFiles_AfterSelect;
            listViewFiles.DoubleClick += listViewFiles_DoubleClick;
            listViewFiles.KeyDown += listViewFiles_KeyDown;
        }

        private void SetupSystemIcons()
        {
            var folderIcon = GetSystemFolderIcon();
            var fileIcon = GetSystemFileIcon();
            if (folderIcon == null || fileIcon == null)
                return;

            var imageList = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(16, 16)
            };
            imageList.Images.Add(folderIcon);
            imageList.Images.Add(fileIcon);
            _folderImageList = imageList;
            treeViewFiles.ImageList = imageList;
            listViewFiles.SmallImageList = imageList;
        }

        private static Icon? GetSystemFolderIcon()
        {
            const uint flags = ShgfiIcon | ShgfiSmallIcon | ShgfiUseFileAttributes;
            IntPtr result = SHGetFileInfo("folder", FileAttributeDirectory, out SHFILEINFO fileInfo,
                (uint)Marshal.SizeOf<SHFILEINFO>(), flags);

            if (result == IntPtr.Zero || fileInfo.hIcon == IntPtr.Zero)
                return null;

            try
            {
                return (Icon)Icon.FromHandle(fileInfo.hIcon).Clone();
            }
            finally
            {
                DestroyIcon(fileInfo.hIcon);
            }
        }

        private static Icon? GetSystemFileIcon()
        {
            const uint flags = ShgfiIcon | ShgfiSmallIcon | ShgfiUseFileAttributes;
            IntPtr result = SHGetFileInfo("file", 0, out SHFILEINFO fileInfo,
                (uint)Marshal.SizeOf<SHFILEINFO>(), flags);

            if (result == IntPtr.Zero || fileInfo.hIcon == IntPtr.Zero)
                return null;

            try
            {
                return (Icon)Icon.FromHandle(fileInfo.hIcon).Clone();
            }
            finally
            {
                DestroyIcon(fileInfo.hIcon);
            }
        }

        private void SetupListViewColumns()
        {
            listViewFiles.View = View.Details;
            listViewFiles.FullRowSelect = true;
            listViewFiles.Columns.Clear();
            listViewFiles.Columns.Add("Filename", 300);
            listViewFiles.Columns.Add("Size", 100);
            listViewFiles.Columns.Add("Offset", 120);
        }

        private void ButtonOpen_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "HD6 Index (*.HD6)|*.HD6";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string hd6Path = ofd.FileName;
                    _datFilePath = Path.ChangeExtension(hd6Path, ".DAT");

                    if (!File.Exists(_datFilePath))
                    {
                        MessageBox.Show($"Matching .DAT file not found at:\n{_datFilePath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Parse .HD6
                    _loadedEntries = HD6Parser.Parse(hd6Path);

                    // Populate UI
                    PopulateTreeView(_loadedEntries);
                    MessageBox.Show($"Successfully loaded {_loadedEntries.Count} files!", "Success");
                }
            }
        }

        private void PopulateTreeView(List<HD6Entry> entries)
        {
            treeViewFiles.Nodes.Clear();
            listViewFiles.Items.Clear();

            string rootName = Path.GetFileNameWithoutExtension(_datFilePath) ?? "Archive";
            TreeNode rootNode = treeViewFiles.Nodes.Add("archive", rootName);
            rootNode.Tag = new List<HD6Entry>();
            rootNode.ImageIndex = 0;
            rootNode.SelectedImageIndex = 0;

            foreach (var entry in entries)
            {
                string[] parts = (entry.Path ?? string.Empty).Split(
                    new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
                TreeNodeCollection currentNodes = rootNode.Nodes;
                TreeNode currentFolderNode = rootNode;

                for (int i = 0; i < Math.Max(0, parts.Length - 1); i++)
                {
                    string folderName = parts[i];
                    if (!currentNodes.ContainsKey(folderName))
                    {
                        currentFolderNode = currentNodes.Add(folderName, folderName);
                        currentFolderNode.Tag = new List<HD6Entry>(); // Initialize file list for this folder
                        currentFolderNode.ImageIndex = 0;
                        currentFolderNode.SelectedImageIndex = 0;
                        currentNodes = currentFolderNode.Nodes;
                    }
                    else
                    {
                        currentFolderNode = currentNodes[folderName]!;
                        currentNodes = currentFolderNode.Nodes;
                    }
                }

                if (currentFolderNode.Tag is List<HD6Entry> folderFiles)
                {
                    folderFiles.Add(entry);
                }
            }

            rootNode.Expand();
            treeViewFiles.SelectedNode = rootNode;
        }

        // Triggered whenever a node/folder is selected in the TreeView
        private void treeViewFiles_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            listViewFiles.Items.Clear();
            TreeNode node = e.Node!;

            if (node.Parent is not null)
            {
                var parentItem = new ListViewItem("...")
                {
                    ImageIndex = _folderImageList == null ? -1 : 0,
                    Tag = new ParentNavigation(node.Parent)
                };
                parentItem.SubItems.Add("Parent folder");
                parentItem.SubItems.Add(string.Empty);
                listViewFiles.Items.Add(parentItem);
            }

            foreach (TreeNode childNode in node.Nodes.Cast<TreeNode>().Where(ContainsFiles).OrderBy(
                node => node.Text, StringComparer.OrdinalIgnoreCase))
            {
                AddFolderToListView(childNode);
            }

            if (node.Tag is List<HD6Entry> folderFiles)
            {
                foreach (var entry in folderFiles.OrderBy(entry => Path.GetFileName(entry.Path ?? string.Empty), StringComparer.OrdinalIgnoreCase))
                {
                    AddEntryToListView(entry);
                }
            }
        }

        private bool ContainsFiles(TreeNode folderNode)
        {
            if (folderNode.Tag is List<HD6Entry> files && files.Count > 0)
                return true;

            foreach (TreeNode childNode in folderNode.Nodes)
            {
                if (ContainsFiles(childNode))
                    return true;
            }

            return false;
        }

        private void AddFolderToListView(TreeNode folderNode)
        {
            ListViewItem item = new ListViewItem(folderNode.Text)
            {
                ImageIndex = _folderImageList == null ? -1 : 0,
                Tag = folderNode
            };
            item.SubItems.Add("<DIR>");
            item.SubItems.Add(string.Empty);
            listViewFiles.Items.Add(item);
        }

        private void AddEntryToListView(HD6Entry entry)
        {
            string fileName = Path.GetFileName(entry.Path ?? string.Empty);
            ListViewItem item = new ListViewItem(fileName);
            item.ImageIndex = _folderImageList == null ? -1 : 1;
            item.SubItems.Add(FormatSize(entry.FileSize));
            item.SubItems.Add($"0x{entry.DatOffset:X}"); // Hexadecimal offset in .DAT
            item.Tag = entry; // Attach HD6Entry metadata for double-click extraction

            listViewFiles.Items.Add(item);
        }

        // Formats raw bytes into readable B, KB, or MB units
        private string FormatSize(long bytes)
        {
            if (bytes >= 1024 * 1024)
                return $"{((double)bytes / (1024 * 1024)):F2} MB";
            if (bytes >= 1024)
                return $"{((double)bytes / 1024):F2} KB";
            return $"{bytes} B";
        }

        // Extract file when double-clicking an item in the ListView
        private void listViewFiles_DoubleClick(object? sender, EventArgs e)
        {
            if (listViewFiles.SelectedItems.Count > 0 && listViewFiles.SelectedItems[0].Tag is ParentNavigation parentNavigation)
            {
                treeViewFiles.SelectedNode = parentNavigation.Node;
                parentNavigation.Node.EnsureVisible();
                return;
            }

            if (listViewFiles.SelectedItems.Count > 0 && listViewFiles.SelectedItems[0].Tag is TreeNode folderNode)
            {
                treeViewFiles.SelectedNode = folderNode;
                folderNode.EnsureVisible();
                return;
            }

            ExtractSelectedItems(useCurrentPath: false);
        }

        private void listViewFiles_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && listViewFiles.SelectedItems.Count == 1 &&
                listViewFiles.SelectedItems[0].Tag is ParentNavigation parentNavigation)
            {
                treeViewFiles.SelectedNode = parentNavigation.Node;
                parentNavigation.Node.EnsureVisible();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Enter && listViewFiles.SelectedItems.Count == 1 &&
                listViewFiles.SelectedItems[0].Tag is TreeNode folderNode)
            {
                treeViewFiles.SelectedNode = folderNode;
                folderNode.EnsureVisible();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Back && treeViewFiles.SelectedNode?.Parent is TreeNode parentNode)
            {
                treeViewFiles.SelectedNode = parentNode;
                parentNode.EnsureVisible();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void ButtonExtract_Click(object? sender, EventArgs e)
        {
            ExtractSelectedItems(useCurrentPath: false);
        }

        private void ButtonExtractCurrent_Click(object? sender, EventArgs e)
        {
            ExtractSelectedItems(useCurrentPath: true);
        }

        private void listViewFiles_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateExtractButtonState();
        }

        private void UpdateExtractButtonState()
        {
            var enabled = File.Exists(_datFilePath) &&
                listViewFiles.SelectedItems.Cast<ListViewItem>().Any(item => item.Tag is HD6Entry or TreeNode);
            buttonExtract.Enabled = enabled;
            buttonExtractCurrent.Enabled = enabled;
        }

        private void ExtractSelectedItems(bool useCurrentPath)
        {
            var selectedEntries = GetSelectedEntries().ToList();
            if (selectedEntries.Count == 0 || string.IsNullOrWhiteSpace(_datFilePath))
            {
                MessageBox.Show("Select one or more files or folders to extract.", "No selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Choose an extraction destination";
                if (folderDialog.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    foreach (var entry in selectedEntries)
                    {
                        var relativePath = useCurrentPath
                            ? GetPathRelativeToCurrentFolder(entry)
                            : GetArchivePath(entry);
                        ExtractSingleFile(_datFilePath, entry, folderDialog.SelectedPath, relativePath);
                    }

                    var mode = useCurrentPath ? "current path" : "archive root path";
                    MessageBox.Show($"Extracted {selectedEntries.Count} file(s) using the {mode}.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidDataException)
                {
                    MessageBox.Show($"Extraction failed:\n{exception.Message}", "Extraction error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private IEnumerable<HD6Entry> GetSelectedEntries()
        {
            var entries = new HashSet<HD6Entry>();
            foreach (ListViewItem item in listViewFiles.SelectedItems)
            {
                if (item.Tag is HD6Entry entry)
                {
                    entries.Add(entry);
                }
                else if (item.Tag is TreeNode folderNode)
                {
                    AddFolderEntries(folderNode, entries);
                }
            }

            return entries;
        }

        private static void AddFolderEntries(TreeNode folderNode, ISet<HD6Entry> entries)
        {
            if (folderNode.Tag is List<HD6Entry> folderEntries)
            {
                foreach (var entry in folderEntries)
                {
                    entries.Add(entry);
                }
            }

            foreach (TreeNode childNode in folderNode.Nodes)
            {
                AddFolderEntries(childNode, entries);
            }
        }

        private static IReadOnlyList<string> GetArchivePath(HD6Entry entry)
        {
            return SplitArchivePath(entry.Path);
        }

        private IReadOnlyList<string> GetPathRelativeToCurrentFolder(HD6Entry entry)
        {
            var entryParts = SplitArchivePath(entry.Path);
            var currentParts = GetCurrentFolderPath();
            if (entryParts.Count < currentParts.Count || !entryParts.Take(currentParts.Count).SequenceEqual(currentParts, StringComparer.OrdinalIgnoreCase))
                throw new InvalidDataException($"The selected entry is not inside the current list-view path: {entry.Path}");

            var relativeParts = entryParts.Skip(currentParts.Count).ToArray();
            if (relativeParts.Length == 0)
                throw new InvalidDataException($"The selected entry has no file name: {entry.Path}");

            return relativeParts;
        }

        private IReadOnlyList<string> GetCurrentFolderPath()
        {
            var node = treeViewFiles.SelectedNode;
            if (node is null || node.Parent is null)
                return Array.Empty<string>();

            var parts = new Stack<string>();
            while (node.Parent is not null)
            {
                parts.Push(node.Text);
                node = node.Parent;
            }

            return parts.ToArray();
        }

        private static IReadOnlyList<string> SplitArchivePath(string? archivePath)
        {
            var parts = (archivePath ?? string.Empty).Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0 || parts.Any(part => part is "." or ".." || Path.IsPathRooted(part) || part.Contains(':')))
                throw new InvalidDataException($"Invalid archive path: {archivePath}");

            return parts;
        }

        private static void ExtractSingleFile(string datPath, HD6Entry entry, string outputDirectory, IReadOnlyList<string> relativePath)
        {
            var outputPath = Path.GetFullPath(Path.Combine(new[] { outputDirectory }.Concat(relativePath).ToArray()));
            var fullOutputDirectory = Path.GetFullPath(outputDirectory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!outputPath.StartsWith(fullOutputDirectory, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Archive path escapes the extraction directory: {entry.Path}");

            var parentDirectory = Path.GetDirectoryName(outputPath);
            if (parentDirectory is not null)
                Directory.CreateDirectory(parentDirectory);

            using (FileStream datStream = new FileStream(datPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (FileStream outStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            {
                if (entry.DatOffset < 0 || entry.FileSize < 0 || entry.DatOffset > datStream.Length || entry.FileSize > datStream.Length - entry.DatOffset)
                    throw new InvalidDataException($"Invalid DAT range for archive entry: {entry.Path}");

                datStream.Seek(entry.DatOffset, SeekOrigin.Begin);
                byte[] buffer = new byte[8192];
                long bytesLeft = entry.FileSize;

                while (bytesLeft > 0)
                {
                    int read = datStream.Read(buffer, 0, (int)Math.Min(buffer.Length, bytesLeft));
                    if (read == 0)
                        throw new EndOfStreamException($"The DAT file ended before '{entry.Path}' was completely read.");
                    outStream.Write(buffer, 0, read);
                    bytesLeft -= read;
                }
            }
        }
    }
}
using System.Drawing;
using System.Text.Json;
using System.Windows.Forms;

namespace ClipTray;

internal sealed class EditorForm : Form
{
    private readonly string path;
    private readonly Action onSaved;
    private List<ClipEntry> entries;
    private readonly TreeView tree = new() { Dock = DockStyle.Fill, HideSelection = false };
    private readonly TextBox title = new() { Dock = DockStyle.Top };
    private readonly TextBox content = new() { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Both, AcceptsReturn = true, AcceptsTab = true, WordWrap = true };
    private readonly Label textLabel = new() { Text = "Clipboard text", Dock = DockStyle.Top, Height = 24 };
    private readonly Button save = new() { Text = "Save", Width = 100 };
    private readonly Button cancel = new() { Text = "Close", Width = 100 };
    private bool loading;
    private bool dirty;

    public EditorForm(string configPath, Action onSavedCallback)
    {
        path = configPath;
        onSaved = onSavedCallback;
        entries = JsonSerializer.Deserialize<List<ClipEntry>>(File.ReadAllText(path)) ?? new();
        Text = "ClipTray — Clip Editor";
        Width = 920; Height = 580; MinimumSize = new Size(680, 400);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9);

       var split = new SplitContainer
{
    Dock = DockStyle.Fill,
    Orientation = Orientation.Vertical
};

Shown += (s, e) =>
{
    split.Panel1MinSize = 180;
    split.Panel2MinSize = 280;
    split.SplitterDistance = 285;
};
        Shown += (s, e) => split.SplitterDistance = 285;
        Controls.Add(split);
        var left = new Panel { Dock = DockStyle.Fill };
        split.Panel1.Controls.Add(left);
        left.Controls.Add(tree);
        var leftButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 88, Padding = new Padding(4), AutoSize = false };
        left.Controls.Add(leftButtons);
        AddButton(leftButtons, "+ Clip", () => AddEntry(false));
        AddButton(leftButtons, "+ Category", () => AddEntry(true));
        AddButton(leftButtons, "Delete", DeleteSelected);
        AddButton(leftButtons, "Move Up", () => MoveSelected(-1));
        AddButton(leftButtons, "Move Down", () => MoveSelected(1));

        var right = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
        split.Panel2.Controls.Add(right);
        var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 50, FlowDirection = FlowDirection.RightToLeft };
        right.Controls.Add(footer);
        footer.Controls.Add(cancel);
        footer.Controls.Add(save);
        var editArea = new Panel { Dock = DockStyle.Fill };
        right.Controls.Add(editArea);
        editArea.Controls.Add(content);
        editArea.Controls.Add(textLabel);
        var titlePanel = new Panel { Dock = DockStyle.Top, Height = 55 };
        editArea.Controls.Add(titlePanel);
        titlePanel.Controls.Add(title);
        titlePanel.Controls.Add(new Label { Text = "Title / category name", Dock = DockStyle.Top, Height = 24 });
        title.TextChanged += (_, _) => { if (loading || tree.SelectedNode?.Tag is not ClipEntry e) return; e.Label = title.Text; tree.SelectedNode.Text = title.Text; MarkDirty(); };
        content.TextChanged += (_, _) => { if (loading || tree.SelectedNode?.Tag is not ClipEntry e || e.Children != null) return; e.Text = content.Text; MarkDirty(); };
        tree.AfterSelect += (_, _) => ShowSelected();
        save.Click += (_, _) => Save();
        cancel.Click += (_, _) => Close();
        FormClosing += OnClosing;
        Populate();
    }

    private static void AddButton(FlowLayoutPanel panel, string caption, Action action)
    {
        var button = new Button { Text = caption, AutoSize = true, Height = 28, Margin = new Padding(2) };
        button.Click += (_, _) => action();
        panel.Controls.Add(button);
    }

    private void Populate(ClipEntry? select = null)
    {
        tree.BeginUpdate();
        tree.Nodes.Clear();
        AddNodes(tree.Nodes, entries, 0);
        tree.ExpandAll();
        tree.EndUpdate();
        if (select != null) tree.SelectedNode = FindNode(tree.Nodes, select);
        else if (tree.Nodes.Count > 0) tree.SelectedNode = tree.Nodes[0];
        else ShowSelected();
    }

    private static void AddNodes(TreeNodeCollection nodes, List<ClipEntry> list, int depth)
    {
        if (depth > 12) return;
        foreach (var item in list)
        {
            var node = nodes.Add(item.Label);
            node.Tag = item;
            if (item.Children != null) AddNodes(node.Nodes, item.Children, depth + 1);
        }
    }

    private static TreeNode? FindNode(TreeNodeCollection nodes, ClipEntry item)
    {
        foreach (TreeNode node in nodes)
        {
            if (ReferenceEquals(node.Tag, item)) return node;
            var child = FindNode(node.Nodes, item);
            if (child != null) return child;
        }
        return null;
    }

    private void ShowSelected()
    {
        loading = true;
        var item = tree.SelectedNode?.Tag as ClipEntry;
        title.Enabled = content.Enabled = item != null;
        title.Text = item?.Label ?? "";
        bool isCategory = item?.Children != null;
        content.Visible = textLabel.Visible = !isCategory;
        content.Text = isCategory ? "" : item?.Text ?? "";
        loading = false;
    }

    private void AddEntry(bool category)
    {
        var selected = tree.SelectedNode;
        var parent = selected?.Tag as ClipEntry;
        var list = parent?.Children ?? (selected?.Parent?.Tag as ClipEntry)?.Children ?? entries;
        if (list == null) list = entries;
        // A new category beside the selected item; a new clip inside a selected category.
        if (category && parent?.Children != null) list = (selected?.Parent?.Tag as ClipEntry)?.Children ?? entries;
        if (category && selected?.Parent != null && selected.Parent.Level >= 12) { MessageBox.Show("Maximum category depth reached."); return; }
        var item = category
            ? new ClipEntry { Label = "New Category", Children = new List<ClipEntry>() }
            : new ClipEntry { Label = "New Clip", Text = "" };
        list.Add(item);
        MarkDirty();
        Populate(item);
        title.Focus(); title.SelectAll();
    }

    private List<ClipEntry> Siblings(TreeNode node) => (node.Parent?.Tag as ClipEntry)?.Children ?? entries;

    private void DeleteSelected()
    {
        if (tree.SelectedNode is not { Tag: ClipEntry item } node) return;
        var prompt = item.Children != null ? "Delete this category and all its clips?" : "Delete this clip?";
        if (MessageBox.Show(prompt, "ClipTray", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        Siblings(node).Remove(item);
        MarkDirty(); Populate();
    }

    private void MoveSelected(int direction)
    {
        if (tree.SelectedNode is not { Tag: ClipEntry item } node) return;
        var siblings = Siblings(node);
        int index = siblings.IndexOf(item), next = index + direction;
        if (index < 0 || next < 0 || next >= siblings.Count) return;
        siblings.RemoveAt(index); siblings.Insert(next, item);
        MarkDirty(); Populate(item);
    }

    private void MarkDirty() { dirty = true; Text = "ClipTray — Clip Editor *"; }

    private bool Save()
    {
        try
        {
            // Never overwrite the user's file with an invalid or partially written JSON document.
            string json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
            string temp = path + ".tmp";
            File.WriteAllText(temp, json);
            File.Move(temp, path, true);
            dirty = false;
            Text = "ClipTray — Clip Editor";
            onSaved();
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Unable to save clips", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private void OnClosing(object? sender, FormClosingEventArgs e)
    {
        if (!dirty) return;
        var answer = MessageBox.Show("Save your changes before closing?", "ClipTray", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        if (answer == DialogResult.Cancel) e.Cancel = true;
        else if (answer == DialogResult.Yes && !Save()) e.Cancel = true;
    }
}

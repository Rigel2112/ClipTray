using System.Diagnostics;
using System.Drawing;
using System.Text.Json;
using System.Windows.Forms;

namespace ClipTray;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new TrayApp());
    }
}

internal sealed class ClipEntry
{
    public string Label { get; set; } = "";
    public string? Text { get; set; }
    public List<ClipEntry>? Children { get; set; }
}

internal sealed class TrayApp : ApplicationContext
{
    private readonly NotifyIcon icon;
    private readonly ContextMenuStrip menu = new();
    private readonly string configPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClipTray", "clips.json");

    public TrayApp()
    {
        icon = new NotifyIcon
        {
            Icon = SystemIcons.Information,
            Text = "ClipTray - right-click for snippets",
            ContextMenuStrip = menu,
            Visible = true
        };
        menu.Opening += (_, _) => RebuildMenu();
        icon.DoubleClick += (_, _) => OpenConfig();
        EnsureConfig();
        RebuildMenu();
    }

    private void EnsureConfig()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
        if (File.Exists(configPath)) return;
        var examples = new List<ClipEntry>
        {
            new() { Label = "Hello", Text = "Hello, thank you for contacting support." },
            new() { Label = "Follow-up", Text = "I'm following up on your request. Please let me know if you need anything else." },
            new() { Label = "Support", Children = new()
                {
                    new() { Label = "Restart instructions", Text = "Please restart your computer and try again.\r\nIf the issue continues, let us know." },
                    new() { Label = "Ticket closed", Text = "Your support ticket has been resolved and closed. Thank you!" }
                }
            }
        };
        File.WriteAllText(configPath, JsonSerializer.Serialize(examples, new JsonSerializerOptions { WriteIndented = true }));
    }

    private void RebuildMenu()
    {
        menu.Items.Clear();
        try
        {
            var entries = JsonSerializer.Deserialize<List<ClipEntry>>(File.ReadAllText(configPath)) ?? new();
            AddEntries(menu.Items, entries, 0);
        }
        catch (Exception ex)
        {
            menu.Items.Add(new ToolStripMenuItem("Configuration error (click for details)", null,
                (_, _) => MessageBox.Show(ex.Message, "ClipTray", MessageBoxButtons.OK, MessageBoxIcon.Error)));
        }
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Edit clips.json", null, (_, _) => OpenConfig());
        menu.Items.Add("Reload", null, (_, _) => RebuildMenu());
        menu.Items.Add("Exit", null, (_, _) => ExitThread());
    }

    private static void AddEntries(ToolStripItemCollection destination, IEnumerable<ClipEntry> entries, int depth)
    {
        if (depth > 12) return;
        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Label)) continue;
            var item = new ToolStripMenuItem(entry.Label.Replace("&", "&&"));
            if (entry.Children is { Count: > 0 })
                AddEntries(item.DropDownItems, entry.Children, depth + 1);
            else if (!string.IsNullOrEmpty(entry.Text))
            {
                string value = entry.Text;
                item.Click += (_, _) => Copy(value);
            }
            else item.Enabled = false;
            destination.Add(item);
        }
    }

    private static void Copy(string value)
    {
        for (int attempt = 0; attempt < 5; attempt++)
        {
            try { Clipboard.SetText(value); return; }
            catch (System.Runtime.InteropServices.ExternalException) when (attempt < 4)
            {
                Thread.Sleep(75);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Clipboard error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
        MessageBox.Show("The clipboard is currently busy. Please try again.", "ClipTray");
    }

    private void OpenConfig()
    {
        try
        {
            Process.Start(new ProcessStartInfo("notepad.exe", $"\"{configPath}\"") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "ClipTray", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    protected override void ExitThreadCore()
    {
        icon.Visible = false;
        icon.Dispose();
        menu.Dispose();
        base.ExitThreadCore();
    }
}

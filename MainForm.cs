using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MNGLauncher;

public sealed class MainForm : Form
{
    private readonly LauncherSettings settings = LauncherSettings.Load();
    private readonly TextBox serverPath = new();
    private readonly TextBox fifaPath = new();
    private readonly TextBox email = new();
    private readonly TextBox persona = new();
    private readonly RichTextBox log = new();
    private readonly Label serverState = StatusLabel("● Serveur hors ligne");
    private readonly Label lsxState = StatusLabel("● LSX hors ligne");
    private readonly Label blazeState = StatusLabel("● Blaze hors ligne");
    private readonly Label fifaState = StatusLabel("● FIFA arrêté");
    private readonly Button playButton = MakeButton("JOUER À FIFA 17", Color.FromArgb(255, 221, 0), 210);
    private Process? serverProcess;
    private Process? launcherProcess;
    private readonly string dailyLog;
    private static readonly Regex Ansi = new(@"\x1B(?:[@-Z\\-_]|\[[0-?]*[ -/]*[@-~])", RegexOptions.Compiled);

    public MainForm()
    {
        Text = "MNG Launcher - FIFA 17 Local";
        Size = new Size(1180, 760);
        MinimumSize = new Size(1050, 680);
        BackColor = Color.FromArgb(238, 239, 240);
        ForeColor = Color.FromArgb(20, 23, 27);
        Font = new Font("Segoe UI", 10);
        StartPosition = FormStartPosition.CenterScreen;
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "mnglauncher.ico");
        if (File.Exists(iconPath)) Icon = new Icon(iconPath);

        var logsDir = Path.Combine(LauncherSettings.DataDirectory, "logs");
        Directory.CreateDirectory(logsDir);
        dailyLog = Path.Combine(logsDir, $"launcher-{DateTime.Now:yyyy-MM-dd}.log");

        var content = BuildContent();
        var header = BuildHeader();
        Controls.Add(content);
        Controls.Add(header);
        header.BringToFront();
        Load += async (_, _) => { LoadFields(); WriteLog("MNG Launcher prêt."); await RefreshStatusAsync(); };
        FormClosing += OnClosing;
    }

    private Control BuildHeader()
    {
        var panel = new FifaHeaderPanel { Dock = DockStyle.Top, Height = 124 };
        panel.Controls.Add(new Label { Text = "MNG", Font = new Font("Arial", 31, FontStyle.Bold), ForeColor = Color.FromArgb(20, 22, 25), BackColor = Color.Transparent, AutoSize = true, Location = new Point(28, 18) });
        panel.Controls.Add(new Label { Text = "LAUNCHER", Font = new Font("Arial", 17, FontStyle.Bold), ForeColor = Color.FromArgb(20, 22, 25), BackColor = Color.Transparent, AutoSize = true, Location = new Point(31, 66) });
        panel.Controls.Add(new Label { Text = "FIFA 17  /  LOCAL REVIVAL", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(20, 22, 25), BackColor = Color.Transparent, AutoSize = true, Location = new Point(224, 79) });
        panel.Controls.Add(new Label { Text = "17", Font = new Font("Arial", 49, FontStyle.Bold), ForeColor = Color.FromArgb(20, 22, 25), BackColor = Color.Transparent, AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(1070, 20) });
        var logoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "logo.png");
        if (File.Exists(logoPath))
        {
            panel.Controls.Add(new PictureBox
            {
                Image = Image.FromFile(logoPath),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Size = new Size(108, 108),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(930, 8)
            });
        }
        return panel;
    }

    private Control BuildContent()
    {
        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = BackColor, Padding = new Padding(20, 144, 20, 18) };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        var leftCard = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(22), Margin = new Padding(0, 0, 10, 0) };
        leftCard.Controls.Add(BuildControls());
        grid.Controls.Add(leftCard, 0, 0);
        grid.Controls.Add(BuildLog(), 1, 0);
        return grid;
    }

    private Control BuildControls()
    {
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 11, Padding = new Padding(2), AutoScroll = true, BackColor = Color.White };
        table.Controls.Add(Section("Compte local"));
        table.Controls.Add(FieldRow("E-mail", email));
        table.Controls.Add(FieldRow("Persona", persona));
        table.Controls.Add(Section("Installation"));
        table.Controls.Add(PathRow("Serveur", serverPath, true));
        table.Controls.Add(PathRow("FIFA17.exe", fifaPath, false));
        var states = new FlowLayoutPanel { Height = 58, Dock = DockStyle.Top, WrapContents = true, BackColor = Color.FromArgb(245, 246, 247), Padding = new Padding(8) };
        states.Controls.AddRange([serverState, lsxState, blazeState, fifaState]);
        table.Controls.Add(states);
        var actions = new FlowLayoutPanel { Height = 88, Dock = DockStyle.Top, WrapContents = true, Padding = new Padding(0, 5, 0, 0) };
        var save = MakeButton("ENREGISTRER", Color.FromArgb(30, 34, 40)); save.Click += (_, _) => SaveFields();
        var session = MakeButton("CRÉER SESSION", Color.FromArgb(30, 34, 40)); session.Click += (_, _) => CreateSession();
        var start = MakeButton("DÉMARRER SERVEUR", Color.FromArgb(30, 34, 40), 150); start.Click += async (_, _) => await StartServerAsync();
        var stopServer = MakeButton("FERMER SERVEUR", Color.FromArgb(175, 112, 18), 145); stopServer.Click += async (_, _) => await StopServerAsync();
        var stop = MakeButton("TOUT ARRÊTER", Color.FromArgb(30, 34, 40)); stop.Click += (_, _) => StopAll();
        var test = MakeButton("TESTER SERVICES", Color.FromArgb(30, 34, 40), 145); test.Click += async (_, _) => await RefreshStatusAsync(true);
        actions.Controls.AddRange([save, session, start, stopServer, test, stop]);
        table.Controls.Add(actions);
        playButton.Height = 58; playButton.ForeColor = Color.FromArgb(15, 18, 22); playButton.Font = new Font("Arial", 14, FontStyle.Bold); playButton.Click += async (_, _) => await PlayAsync();
        table.Controls.Add(playButton);
        table.Controls.Add(new Label { Text = "SESSION UNIQUE  •  SERVEUR LOCAL  •  FIFA 17", ForeColor = Color.FromArgb(100, 104, 110), Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, Padding = new Padding(3, 8, 0, 0) });
        return table;
    }

    private Control BuildLog()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(25, 28, 33), Padding = new Padding(16), Margin = new Padding(10, 0, 0, 0) };
        var title = Section("Journal"); title.Dock = DockStyle.Top;
        log.Dock = DockStyle.Fill; log.ReadOnly = true; log.BackColor = Color.FromArgb(13, 15, 19); log.ForeColor = Color.FromArgb(224, 226, 229); log.Font = new Font("Cascadia Mono", 9); log.BorderStyle = BorderStyle.None;
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42 };
        var clear = MakeButton("EFFACER", Color.FromArgb(255, 221, 0), 85); clear.ForeColor = Color.Black; clear.Click += (_, _) => log.Clear();
        var copy = MakeButton("COPIER", Color.FromArgb(48, 53, 61), 85); copy.Click += (_, _) => { if (log.TextLength > 0) Clipboard.SetText(log.Text); };
        var folder = MakeButton("DOSSIER LOGS", Color.FromArgb(48, 53, 61), 120); folder.Click += (_, _) => Process.Start("explorer.exe", Path.GetDirectoryName(dailyLog)!);
        buttons.Controls.AddRange([clear, copy, folder]);
        panel.Controls.Add(log); panel.Controls.Add(buttons); panel.Controls.Add(title);
        return panel;
    }

    private async Task PlayAsync()
    {
        try
        {
            playButton.Enabled = false;
            SaveFields();
            ValidatePaths();
            CreateSession();
            if (!await ServicesReadyAsync()) await StartServerAsync();
            if (!await WaitServicesAsync(TimeSpan.FromSeconds(25))) throw new InvalidOperationException("Les services FIFA ne sont pas prêts.");
            if (Process.GetProcessesByName("FIFA17").Length > 0) throw new InvalidOperationException("FIFA 17 est déjà lancé.");
            var reliableScript = Path.Combine(settings.ServerDirectory, "tools", "run-fifa17-reliable-test.ps1");
            var script = File.Exists(reliableScript)
                ? reliableScript
                : Path.Combine(settings.ServerDirectory, "tools", "run-combined-blaze-10041.ps1");
            launcherProcess = StartPowerShell(script, settings.ServerDirectory, "JEU");
            WriteLog("Lancement FIFA 17 demandé via la session unique.");
            fifaState.Text = "● FIFA en démarrage"; fifaState.ForeColor = Color.Orange;
        }
        catch (Exception ex) { WriteLog("ERREUR JEU : " + ex.Message, true); MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { playButton.Enabled = true; }
    }

    private async Task StartServerAsync()
    {
        ValidatePaths();
        if (await ServicesReadyAsync()) { WriteLog("Le serveur répond déjà."); return; }
        var script = Path.Combine(settings.ServerDirectory, "tools", "start-current-server.ps1");
        serverProcess = StartPowerShell(script, settings.ServerDirectory, "SERVEUR");
        serverState.Text = "● Serveur en démarrage"; serverState.ForeColor = Color.Orange;
        WriteLog("Démarrage du serveur local.");
        await WaitServicesAsync(TimeSpan.FromSeconds(25));
    }

    private async Task StopServerAsync()
    {
        try
        {
            var script = Path.Combine(settings.ServerDirectory, "tools", "stop-current-server.ps1");
            var process = StartPowerShell(script, settings.ServerDirectory, "ARRÊT SERVEUR");
            await process.WaitForExitAsync();
            serverProcess = null;
            WriteLog("Services locaux arrêtés. FIFA n'a pas été fermé.");
            await RefreshStatusAsync(true);
        }
        catch (Exception ex)
        {
            WriteLog("ERREUR ARRÊT SERVEUR : " + ex.Message, true);
            MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private Process StartPowerShell(string script, string workingDirectory, string prefix)
    {
        if (!File.Exists(script)) throw new FileNotFoundException("Script introuvable", script);
        var psi = new ProcessStartInfo("powershell.exe") { WorkingDirectory = workingDirectory, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
        psi.ArgumentList.Add("-NoProfile"); psi.ArgumentList.Add("-ExecutionPolicy"); psi.ArgumentList.Add("Bypass"); psi.ArgumentList.Add("-File"); psi.ArgumentList.Add(script);
        var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
        process.OutputDataReceived += (_, e) => { if (e.Data is not null) WriteLog($"[{prefix}] {e.Data}"); };
        process.ErrorDataReceived += (_, e) => { if (e.Data is not null) WriteLog($"[{prefix}] {e.Data}", true); };
        process.Exited += (_, _) => WriteLog($"{prefix} terminé (code {process.ExitCode}).", process.ExitCode != 0);
        process.Start(); process.BeginOutputReadLine(); process.BeginErrorReadLine();
        return process;
    }

    private void CreateSession()
    {
        SaveFields();
        var session = new Dictionary<string, object> { ["Email"] = settings.Email, ["PersonaName"] = settings.PersonaName, ["Uid"] = settings.Uid, ["PersonaId"] = settings.PersonaId, ["AuthCode"] = "LOCAL-FIFA17-AUTH", ["Pctk"] = $"LOCAL-PCTK-{settings.Uid}", ["Skey"] = $"LOCAL-SKEY-{settings.Uid}", ["Online"] = true, ["LoggedIn"] = true, ["LoginAtUtc"] = DateTime.UtcNow };
        var json = JsonSerializer.Serialize(session, new JsonSerializerOptions { WriteIndented = true });
        var local = Path.Combine(LauncherSettings.DataDirectory, "active-session.json");
        File.WriteAllText(local, json);
        File.WriteAllText(Path.Combine(settings.ServerDirectory, "active-session.json"), json);
        WriteLog($"Session créée : {settings.PersonaName} / UID {settings.Uid}.");
    }

    private async Task<bool> WaitServicesAsync(TimeSpan timeout)
    {
        var until = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < until) { if (await RefreshStatusAsync()) return true; await Task.Delay(500); }
        return false;
    }

    private Task<bool> ServicesReadyAsync() => RefreshStatusAsync();

    private async Task<bool> RefreshStatusAsync(bool verbose = false)
    {
        var lsx = await PortOpenAsync(4216); var blaze = await PortOpenAsync(10041); var redir = await PortOpenAsync(42230); var nucleus = await PortOpenAsync(4433); var fut = await PortOpenAsync(8000);
        SetStatus(lsxState, lsx, "LSX"); SetStatus(blazeState, blaze, "Blaze"); SetStatus(serverState, blaze && redir && nucleus && fut, "Serveur");
        var fifa = Process.GetProcessesByName("FIFA17").Length > 0; SetStatus(fifaState, fifa, "FIFA");
        if (verbose) WriteLog($"Services : Redirector={(redir ? "OK" : "NON")}, Blaze={(blaze ? "OK" : "NON")}, Nucleus={(nucleus ? "OK" : "NON")}, FUT={(fut ? "OK" : "NON")}, LSX={(lsx ? "OK" : "NON")}.");
        return lsx && blaze && redir && nucleus && fut;
    }

    private static async Task<bool> PortOpenAsync(int port)
    {
        try { using var tcp = new TcpClient(); using var cts = new CancellationTokenSource(700); await tcp.ConnectAsync("127.0.0.1", port, cts.Token); return true; } catch { return false; }
    }

    private void StopAll()
    {
        TryStop(launcherProcess); TryStop(serverProcess);
        foreach (var p in Process.GetProcessesByName("FIFA17")) TryStop(p);
        WriteLog("Processus du launcher arrêtés.");
        _ = RefreshStatusAsync();
    }

    private static void TryStop(Process? p) { try { if (p is { HasExited: false }) p.Kill(true); } catch { } }
    private void ValidatePaths()
    {
        if (!Directory.Exists(settings.ServerDirectory) || !File.Exists(Path.Combine(settings.ServerDirectory, "package.json"))) throw new InvalidOperationException("Le dossier serveur est invalide.");
        if (!File.Exists(settings.FifaExecutable)) throw new InvalidOperationException("FIFA17.exe est introuvable.");
    }
    private void SaveFields() { settings.ServerDirectory = serverPath.Text.Trim(); settings.FifaExecutable = fifaPath.Text.Trim(); settings.Email = email.Text.Trim(); settings.PersonaName = persona.Text.Trim(); settings.Save(); WriteLog("Configuration enregistrée."); }
    private void LoadFields() { serverPath.Text = settings.ServerDirectory; fifaPath.Text = settings.FifaExecutable; email.Text = settings.Email; persona.Text = settings.PersonaName; }
    private void WriteLog(string text, bool error = false)
    {
        text = Ansi.Replace(text, ""); var line = $"[{DateTime.Now:HH:mm:ss}] {text}";
        void append() { log.SelectionColor = error ? Color.Salmon : Color.Gainsboro; log.AppendText(line + Environment.NewLine); log.ScrollToCaret(); try { File.AppendAllText(dailyLog, line + Environment.NewLine, Encoding.UTF8); } catch { } }
        if (InvokeRequired) BeginInvoke(append); else append();
    }
    private void OnClosing(object? sender, FormClosingEventArgs e) { settings.Save(); if ((serverProcess is { HasExited: false }) || Process.GetProcessesByName("FIFA17").Length > 0) { if (MessageBox.Show("FIFA ou le serveur tourne encore. Tout arrêter ?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) StopAll(); } }
    private static Label StatusLabel(string text) => new() { Text = text, AutoSize = true, ForeColor = Color.FromArgb(100, 104, 110), Margin = new Padding(7, 8, 10, 4), BackColor = Color.Transparent };
    private static void SetStatus(Label label, bool online, string name) { label.Text = $"● {name} {(online ? "en ligne" : "hors ligne")}"; label.ForeColor = online ? Color.LightGreen : Color.Gray; }
    private static Label Section(string text) => new() { Text = text.ToUpperInvariant(), Font = new Font("Arial", 11, FontStyle.Bold), ForeColor = Color.FromArgb(22, 25, 29), AutoSize = true, Padding = new Padding(0, 9, 0, 5), BackColor = Color.Transparent };
    private static Control FieldRow(string label, TextBox box) { var p = new TableLayoutPanel { Height = 58, Dock = DockStyle.Top, ColumnCount = 1, BackColor = Color.White }; p.Controls.Add(new Label { Text = label, AutoSize = true, ForeColor = Color.FromArgb(70, 73, 78) }); box.Dock = DockStyle.Top; box.BackColor = Color.FromArgb(245, 246, 247); box.ForeColor = Color.FromArgb(20, 23, 27); box.BorderStyle = BorderStyle.FixedSingle; p.Controls.Add(box); return p; }
    private Control PathRow(string label, TextBox box, bool folder) { var p = new TableLayoutPanel { Height = 62, Dock = DockStyle.Top, ColumnCount = 2 }; p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); p.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 45)); var inner = FieldRow(label, box); p.Controls.Add(inner, 0, 0); var browse = MakeButton("…", Color.FromArgb(55, 65, 78), 38); browse.Click += (_, _) => { if (folder) { using var d = new FolderBrowserDialog { InitialDirectory = box.Text }; if (d.ShowDialog() == DialogResult.OK) box.Text = d.SelectedPath; } else { using var f = new OpenFileDialog { Filter = "FIFA17.exe|FIFA17.exe|Exécutables|*.exe", InitialDirectory = Path.GetDirectoryName(box.Text) }; if (f.ShowDialog() == DialogResult.OK) box.Text = f.FileName; } }; p.Controls.Add(browse, 1, 0); return p; }
    private static Button MakeButton(string text, Color color, int width = 130) => new() { Text = text, Width = width, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = color, ForeColor = Color.White, FlatAppearance = { BorderSize = 0 }, Margin = new Padding(4) };
}

internal sealed class FifaHeaderPanel : Panel
{
    public FifaHeaderPanel() { DoubleBuffered = true; BackColor = Color.FromArgb(255, 221, 0); }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var dark = new SolidBrush(Color.FromArgb(22, 25, 29));
        using var shade = new SolidBrush(Color.FromArgb(42, 0, 0, 0));
        var w = Width;
        e.Graphics.FillPolygon(shade, new Point[] { new(w - 470, 0), new(w - 260, 0), new(w - 350, Height), new(w - 560, Height) });
        e.Graphics.FillPolygon(dark, new Point[] { new(w - 185, 0), new(w - 145, 0), new(w - 245, Height), new(w - 285, Height) });
        e.Graphics.FillPolygon(dark, new Point[] { new(w - 95, 0), new(w - 72, 0), new(w - 168, Height), new(w - 191, Height) });
        using var pen = new Pen(Color.FromArgb(80, 22, 25, 29), 2);
        for (var x = w - 540; x < w - 310; x += 18) e.Graphics.DrawLine(pen, x, 13, x - 65, Height - 12);
    }
}

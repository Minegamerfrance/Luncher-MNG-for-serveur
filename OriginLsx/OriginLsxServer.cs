# MNG Launcher — Origin LSX Emulator (first slice)
# Hosts/starts the Node LSX listener on 127.0.0.1:3216 before FIFA.

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace MNGLauncher.OriginLsx;

public sealed class OriginLsxServer : IDisposable
{
    public const int DefaultPort = 4216;
    public string Host { get; }
    public int Port { get; }
    public string? SessionFile { get; set; }
    public string? FifaServeurRoot { get; set; }

    public bool IsRunning => _process is { HasExited: false };
    public bool IsPortListening { get; private set; }

    private Process? _process;

    public OriginLsxServer(string host = "127.0.0.1", int port = DefaultPort)
    {
        Host = host;
        Port = port;
    }

    public static bool IsPortBusy(int port = DefaultPort)
    {
        try
        {
            var listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            listener.Stop();
            return false;
        }
        catch (SocketException)
        {
            return true;
        }
    }

    /// <summary>
    /// Start Node LSX standalone (fifa serveur). Throws if :3216 is busy.
    /// </summary>
    public async Task StartAsync(CancellationToken ct = default)
    {
        if (IsPortBusy(Port))
        {
            throw new InvalidOperationException(
                $"LSX_PORT_BUSY {Host}:{Port}. Close Origin.exe then retry.");
        }

        var root = FifaServeurRoot
            ?? FindFifaServeurRoot()
            ?? throw new DirectoryNotFoundException("fifa serveur root not found");

        var psi = new ProcessStartInfo
        {
            FileName = "npm",
            Arguments = "run start:lsx",
            WorkingDirectory = root,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        psi.Environment["LSX_HOST"] = Host;
        psi.Environment["LSX_PORT"] = Port.ToString();
        if (!string.IsNullOrWhiteSpace(SessionFile))
            psi.Environment["MNG_SESSION_FILE"] = SessionFile!;

        _process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start npm run start:lsx");

        _process.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                Console.WriteLine(e.Data);
        };
        _process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                Console.Error.WriteLine(e.Data);
        };
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        // Wait until port accepts or process dies
        for (var i = 0; i < 50; i++)
        {
            ct.ThrowIfCancellationRequested();
            if (_process.HasExited)
                throw new InvalidOperationException("LSX process exited early");
            if (!IsPortBusy(Port))
            {
                await Task.Delay(100, ct);
                continue;
            }
            // IsPortBusy true means something listens — our server is up
            IsPortListening = true;
            Console.WriteLine($"LSX_LISTENING {Host}:{Port} (via Node)");
            return;
        }
        throw new TimeoutException("LSX did not bind in time");
    }

    public async Task StopAsync()
    {
        if (_process is null) return;
        try
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
                await _process.WaitForExitAsync();
            }
        }
        catch { /* ignore */ }
        finally
        {
            _process.Dispose();
            _process = null;
            IsPortListening = false;
        }
    }

    public void Dispose() => StopAsync().GetAwaiter().GetResult();

    private static string? FindFifaServeurRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 8 && dir != null; i++, dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "fifa serveur");
            if (File.Exists(Path.Combine(candidate, "package.json")))
                return candidate;
            if (dir.Name.Equals("fifa serveur", StringComparison.OrdinalIgnoreCase)
                && File.Exists(Path.Combine(dir.FullName, "package.json")))
                return dir.FullName;
        }
        var desktop = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            "serveur fifa 17",
            "fifa serveur");
        return File.Exists(Path.Combine(desktop, "package.json")) ? desktop : null;
    }
}

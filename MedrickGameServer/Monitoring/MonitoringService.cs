using System.Diagnostics;
using MedrickGameServer.Network.Application;

namespace MedrickGameServer.Monitoring;

public class MonitoringService : IDisposable
{
    private readonly NetworkServer networkServer;
    private readonly TimeSpan interval;
    private readonly Timer timer;
    private readonly Process process;
    private TimeSpan previousCpuTime;

    public MonitoringService(NetworkServer networkServer, TimeSpan? interval = null)
    {
        this.networkServer = networkServer;
        this.interval = interval ?? TimeSpan.FromSeconds(5);
        timer = new Timer(LogMetrics, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        process = Process.GetCurrentProcess();
        previousCpuTime = process.TotalProcessorTime;
    }

    public void Start() => timer.Change(TimeSpan.Zero, interval);

    public void Stop() => timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

    private void LogMetrics(object? state)
    {
        process.Refresh();
        var currentCpuTime = process.TotalProcessorTime;
        var cpuUsage = (currentCpuTime - previousCpuTime).TotalMilliseconds /
                       (Environment.ProcessorCount * interval.TotalMilliseconds) * 100;
        previousCpuTime = currentCpuTime;

        var memoryUsageMb = process.WorkingSet64 / 1024 / 1024;
        var connectionCount = networkServer.ConnectedClients.Count;

        Console.WriteLine($"[Metrics] CPU: {cpuUsage:F2}% | RAM: {memoryUsageMb} MB | Connections: {connectionCount}");
    }

    public void Dispose() => timer.Dispose();
}

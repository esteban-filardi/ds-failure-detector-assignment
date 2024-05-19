using DistributedSystemsFailureDetectorAssignment.Model;
using System.Collections.Concurrent;
using System.Text;
using System.Xml.Linq;

namespace DistributedSystemsFailureDetectorAssignment.Application;

/// <summary>
/// Periodically checks the content of nodes to check whether heartbeat messages have been received 
/// before the threshold period and check for the code it has not .
/// 
/// </summary>
public class FailureDetectorBackgroundService : BackgroundService
{
    private readonly TimeSpan FailureDetectorCheckingInterval;
    private readonly ILogger<FailureDetectorBackgroundService> _logger;
    private readonly Cluster _cluster;
    private readonly IConfiguration _config;

    public FailureDetectorBackgroundService(ILogger<FailureDetectorBackgroundService> logger, Cluster cluster, IConfiguration config)
    {
        _logger = logger;
        _cluster = cluster;
        _config = config;
        var FailureDetectorCheckingIntervalMs = config.GetValue<int>("FailureDetectorCheckingIntervalMs");
        FailureDetectorCheckingInterval = TimeSpan.FromMilliseconds(FailureDetectorCheckingIntervalMs);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scopeDisposable = _logger.BeginScope("Failure detector background service");

        _logger.LogInformation("Failure detector service running");

        while (!stoppingToken.IsCancellationRequested)
        {
            // Wait first to allow  for incoming heartbeats to arrive when node first start
            await Task.Delay((int) FailureDetectorCheckingInterval.TotalMilliseconds, stoppingToken);

            CheckForSuspectedNodes();
            RenderCurrentClusterInformation();
        }
    }

    private void CheckForSuspectedNodes()
    {
        _logger.LogInformation("Checking for suspected nodes...");
        var currentDateTime = DateTime.UtcNow;
        
        foreach (var node in _cluster.Nodes.Values)
        {
            if (node.IsNotSuspected() && !node.HasReceivedHeartbeatBeforeThreshold(currentDateTime))
            {
                _logger.LogWarning("Heartbeat of node with ID {NodeId} has not been received before node time limit {T} ms." +
                    "{NewLine}Marking node as suspected", node.NodeId, node.ElapsedTimeThreshold.TotalMilliseconds, Environment.NewLine);

                _cluster.SetNodeAsSuspected(node.NodeUrl);
            }
        }
    }

    private void RenderCurrentClusterInformation()
    {
        StringBuilder sb = new();
        sb.AppendLine("Current cluster information:");
        string tableHeader = string.Format("|{0,-25}|{1,-25}|{2,-22}|{3,-14}|{4,-25}|", "NodeId", "NodeUrl", "LastReceivedHeartbeat", "Status", "HeartbeatThreshold (ms)");
        sb.AppendLine(tableHeader);
        sb.AppendLine(new string('-', 102));
        foreach (var node in _cluster.Nodes.Values)
        {
            var row = string.Format("|{0,-25}|{1,-25}|{2,-22}|{3,-14}|{4,-25}|", node.NodeId, node.NodeUrl, node.LastReceivedHeartbeatTimestamp, node.Status, (int) node.ElapsedTimeThreshold.TotalMilliseconds);

            sb.AppendLine(row);            
        }

        Console.WriteLine(sb);
    }

}

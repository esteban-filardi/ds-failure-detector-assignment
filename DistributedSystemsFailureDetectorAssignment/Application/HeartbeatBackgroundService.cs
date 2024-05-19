using DistributedSystemsFailureDetectorAssignment.Model;
using Grpc.Net.Client;

namespace DistributedSystemsFailureDetectorAssignment.Application;

/// <summary>
/// Periodically sends the heartbeat message to all clusters in the node
/// </summary>
public class HeartbeatBackgroundService : BackgroundService
{
    private const string UnableToSendHeartbeatErrorMessageTemplate = "Unable send heartbeat to node with ID {NodeId}.";
    private readonly TimeSpan HeartbeatPropagationInterval;
    private readonly ILogger<FailureDetectorBackgroundService> _logger;
    private readonly Cluster _cluster;
    private readonly string _currentNodeOwnUrl;

    public HeartbeatBackgroundService(ILogger<FailureDetectorBackgroundService> logger, 
        IConfiguration appConfig,
        Cluster cluster)
    {
        _logger = logger;
        _cluster = cluster;
        _currentNodeOwnUrl = RetrieveCurrentNodeUrlFromConfig(appConfig);
        var heartbeatPropagationIntervalMs = appConfig.GetValue<int>("HeartbeatPropagationIntervalMs");
        HeartbeatPropagationInterval = TimeSpan.FromMilliseconds(heartbeatPropagationIntervalMs);
    }

    private static string RetrieveCurrentNodeUrlFromConfig(IConfiguration appConfig)
    {
        return appConfig.GetValue<string>("urls") ?? appConfig.GetValue<string>("ASPNETCORE_URLS") ?? throw new Exception("Cannot determine current node's own URL");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Running heartbeat background service");

        await SendHeartbeatToClusterNodes(stoppingToken);
    }

    private Task SendHeartbeatToClusterNodes(CancellationToken stoppingToken)
    {
        var tasks = new LinkedList<Task>();
        foreach (var nodeId in _cluster.Nodes.Keys) 
        {            
            var task = Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    Node node = _cluster.Nodes[nodeId];
                    try
                    {
                        node = _cluster.Nodes[nodeId];
                        await SendHeartbeatToNode(node, stoppingToken);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError("Unable to send heartbeat to node with ID { NodeId}", node.NodeId);
                        // Because I'm using gRPC, I could already suspect the node at this point,
                        // but for the purposes of the assignment leaving it to failure detector service because it would also work with some other async message protocol
                    }

                    await Task.Delay((int)HeartbeatPropagationInterval.TotalMilliseconds, stoppingToken);
                }                
            }, CancellationToken.None);

            tasks.AddLast(task);
        }

        return Task.WhenAll(tasks);
    }

    private async Task SendHeartbeatToNode(Node node, CancellationToken stoppingToken)
    {
        using var channel = GrpcChannel.ForAddress(node.NodeUrl);
        var client = new Heartbeat.HeartbeatClient(channel);
        // For the purposes of this assignment, I'm using this node own URL as its NodeId
        await client.ReceiveHeartbeatAsync(new HeartbeatRequest { NodeId = _currentNodeOwnUrl }, cancellationToken: stoppingToken);
    }    
}


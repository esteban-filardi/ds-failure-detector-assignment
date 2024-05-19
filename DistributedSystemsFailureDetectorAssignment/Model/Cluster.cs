using DistributedSystemsFailureDetectorAssignment.Application;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Xml.Linq;

namespace DistributedSystemsFailureDetectorAssignment.Model;

public class Cluster
{
    private readonly TimeSpan _defaultNodeElapsedTimeThreshold;

    public Cluster(IOptions<ClusterOptions> clusterOptionsParam)
    {        
        var clusterOptions = clusterOptionsParam.Value;
        foreach (var nodeOptions in clusterOptions.Nodes)
        {
            var location = nodeOptions.Location;

            _defaultNodeElapsedTimeThreshold = TimeSpan.FromMilliseconds(clusterOptions.DefaultNodeElapsedTimeThresholdMilliseconds);
            Nodes[location] = new Node
            {
                NodeId = location,
                Location = location,
                ElapsedTimeThreshold = _defaultNodeElapsedTimeThreshold,
                Status = NodeStatus.Suspected
            };
        }
    }

    public ConcurrentDictionary<string, Node> Nodes { get; set; } = new ();

    public void SetNodeAsSuspected(string nodeId)
    {
        var node = Nodes[nodeId];

        var copyOfNode = new Node(node)
        {
            Status = NodeStatus.Suspected
        };

        Nodes[nodeId] = copyOfNode;
    }

    public void SetNodeAsUnsuspected(string nodeId)
    {
        var node = Nodes[nodeId];
        NodeStatus previousNodeStatus = node.Status;
        DateTime? previousHeartbeatTimestamp = node.LastReceivedHeartbeatTimestamp;

        var currentDateTime = DateTime.UtcNow;

        var copyOfNode = new Node(node)
        {
            Status = NodeStatus.NotSuspected,
            LastReceivedHeartbeatTimestamp = currentDateTime
        };
        
        if (previousNodeStatus == NodeStatus.Suspected)
        {
            UpdateNodeElapsedTimeThreshold(node, previousHeartbeatTimestamp);
        }

        Nodes[nodeId] = copyOfNode;
    }

    public void UpdateNodeLastReceivedHeartbeatDateTime(string nodeId)
    {
        var node = Nodes[nodeId];

        var copyOfNode = new Node(node)
        {
            LastReceivedHeartbeatTimestamp = DateTime.UtcNow
        };

        Nodes[nodeId] = copyOfNode;
    }

    private void UpdateNodeElapsedTimeThreshold(Node node, DateTime? previousHeartbeatTimestamp)
    {
        if (previousHeartbeatTimestamp.HasValue && node.LastReceivedHeartbeatTimestamp.HasValue)
        {
            node.ElapsedTimeThreshold = node.LastReceivedHeartbeatTimestamp.Value - previousHeartbeatTimestamp.Value;
        }
        else if (node.LastReceivedHeartbeatTimestamp.HasValue)
        {
            var timeElapsedSinceInitialization = node.LastReceivedHeartbeatTimestamp.Value - node.InitiatedTimestamp;
            // Only update the threshold if is higher than the default value
            node.ElapsedTimeThreshold = timeElapsedSinceInitialization > node.ElapsedTimeThreshold ? timeElapsedSinceInitialization : _defaultNodeElapsedTimeThreshold;
        }
    }   
}

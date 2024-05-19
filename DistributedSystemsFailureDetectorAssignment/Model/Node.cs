using Microsoft.AspNetCore.Components.Routing;
using System.Xml.Linq;

namespace DistributedSystemsFailureDetectorAssignment.Model;

public class Node
{
    public string NodeId { get; internal set; } = "";
    /// <summary>
    /// URL of the node
    /// </summary>
    public string NodeUrl { get; set; } = "";
    public NodeStatus Status { get; set; } = NodeStatus.Suspected;
    /// <summary>
    /// Timestamp indicating when the node was first instantiated
    /// </summary>
    public DateTime InitiatedTimestamp = DateTime.UtcNow;
    public DateTime? LastReceivedHeartbeatTimestamp { get; set; }
    /// <summary>
    /// The time that must elapse before it can consider this node as suspect
    /// </summary>
    public TimeSpan ElapsedTimeThreshold { get; set; } = TimeSpan.FromMilliseconds(5000);    

    public Node() { }

    /// <summary>
    /// Clone a node
    /// </summary>
    /// <param name="node"></param>
    public Node(Node node)
    {
        NodeId = node.NodeId;
        NodeUrl = node.NodeUrl;
        Status = node.Status;
        InitiatedTimestamp = node.InitiatedTimestamp;
        LastReceivedHeartbeatTimestamp = node.LastReceivedHeartbeatTimestamp;
        ElapsedTimeThreshold = node.ElapsedTimeThreshold;
    }

    public bool HasReceivedHeartbeatBeforeThreshold(DateTime currentDateTime)
    {
        var elapsedTimeSinceHeartbeat = (currentDateTime - LastReceivedHeartbeatTimestamp);

        return elapsedTimeSinceHeartbeat < ElapsedTimeThreshold;

    }

    public bool IsSuspected()
    {
        return Status == NodeStatus.Suspected;
    }

    public bool IsNotSuspected()
    {
        return Status == NodeStatus.NotSuspected;
    }
}

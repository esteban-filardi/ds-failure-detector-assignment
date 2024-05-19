namespace DistributedSystemsFailureDetectorAssignment.Application;

/// <summary>
/// Class to represent cluster options
/// </summary>
public class ClusterOptions
{
    public const string Cluster = "Cluster";

    public uint DefaultNodeElapsedTimeThresholdMilliseconds { get; set; } = 1000u;
    public IList<NodeOptions> Nodes { get; set; } = [];
}

public class NodeOptions
{
    public string NodeUrl { get; set; } = string.Empty;
}

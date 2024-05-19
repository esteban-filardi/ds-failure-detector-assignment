namespace DistributedSystemsFailureDetectorAssignment.Application;

public class ClusterOptions
{
    public const string Cluster = "Cluster";

    public uint DefaultNodeElapsedTimeThresholdMilliseconds { get; set; } = 1000u;
    public IList<NodeOptions> Nodes { get; set; } = [];
}

public class NodeOptions
{
    public string Location { get; set; } = string.Empty;
}

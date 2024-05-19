using DistributedSystemsFailureDetectorAssignment.Model;
using Grpc.Core;

namespace DistributedSystemsFailureDetectorAssignment.gRpcServices
{
    /// <summary>
    /// Exposes an gRPC endpoint to receive heartbeat messages from nodes in the cluster
    /// </summary>
    public class HeartbeatService : Heartbeat.HeartbeatBase
    {
        private readonly ILogger<HeartbeatService> _logger;
        private readonly Cluster _cluster;

        public HeartbeatService(ILogger<HeartbeatService> logger, Cluster cluster)
        {
            _logger = logger;
            _cluster = cluster;
        }
        /// <summary>
        /// Receives the heartbeat from other nodes
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<HeartbeatReply> ReceiveHeartbeat(HeartbeatRequest request, ServerCallContext context)
        {
            Task.Run(() =>
            {
                if (_cluster.Nodes.ContainsKey(request.NodeId))
                {
                    _logger.LogInformation("Received heartbeat of node with ID: {NodeId}", request.NodeId);
                    var node = _cluster.Nodes[request.NodeId];

                    if (node.IsSuspected())
                    {
                        _logger.LogInformation("Node was currently suspected, marking as unsuspected again.");
                        _cluster.SetNodeAsUnsuspected(node.NodeId);
                    }
                    
                    _cluster.UpdateNodeLastReceivedHeartbeatDateTime(node.NodeId);                    
                } else
                {
                    _logger.LogWarning("Received heartbeat of unknown node with node ID: {NodeId}", request.NodeId);
                }
            });

            return Task.FromResult(new HeartbeatReply
            {
                Message = "Hello " + request.NodeId
            });
        }
    }
}

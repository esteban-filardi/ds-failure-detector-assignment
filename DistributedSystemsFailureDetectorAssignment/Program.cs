using DistributedSystemsFailureDetectorAssignment.Application;
using DistributedSystemsFailureDetectorAssignment.Model;
using DistributedSystemsFailureDetectorAssignment.gRpcServices;

namespace DistributedSystemsFailureDetectorAssignment
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            // Add services to the container.
            builder.Services.Configure<ClusterOptions>(builder.Configuration.GetSection(ClusterOptions.Cluster));
            builder.Services.AddSingleton<Cluster>();

            builder.Services.AddHostedService<HeartbeatBackgroundService>();
            builder.Services.AddHostedService<FailureDetectorBackgroundService>();
            builder.Services.AddGrpc();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.MapGrpcService<HeartbeatService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }
    }
}
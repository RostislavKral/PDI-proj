using System.Collections.Concurrent;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using PDI.Contracts;
using PDI.Hubs;

namespace PDI.Consumers
{public class TaskStatus
    {
        public int PendingTasks;
        public List<int> Results = new List<int>(); 

        public TaskStatus(int pendingTasks)
        {
            PendingTasks = pendingTasks;
        }
    }
    public class PrimeResultConsumer : IConsumer<PrimeResult>
    {
        private readonly IHubContext<PrimeHub> _hubContext;
        private readonly ILogger<PrimeResultConsumer> _logger;
        private static ConcurrentDictionary<string, TaskStatus> Tasks = new();

        public PrimeResultConsumer(IHubContext<PrimeHub> hubContext, ILogger<PrimeResultConsumer> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PrimeResult> context)
        {
            var result = context.Message;
            if (!Tasks.ContainsKey(result.BundleId))
            {
                Console.WriteLine($"Error: BundleId {result.BundleId} not found in TaskTracker.");
                return;
            }
          await _hubContext.Clients.Group(result.BundleId).SendAsync("ReceivePartialResult", new {result.TaskId, result.Primes});
          Console.WriteLine("HELLO");

          if (Tasks.TryGetValue(result.BundleId, out var status))
          {
             
              lock (status.Results)
              {
                  status.Results.AddRange(result.Primes);
              }

              var remainingTasks = Interlocked.Decrement(ref status.PendingTasks);

              Console.WriteLine("PENDING TASK: " + remainingTasks);

              if (status.PendingTasks <= 0)
              {
                  Console.WriteLine($"All tasks for BundleId {result.BundleId} completed. Sending final result.");
                  List<int> finalResults;
                  lock (status.Results)
                  {
                      finalResults = status.Results.OrderBy(x => x).ToList();
                  }

                  await _hubContext.Clients.Group(result.BundleId).SendAsync("ReceiveFinalResult", finalResults);

                     Tasks.TryRemove(result.BundleId, out _);
              }
          }
          

        }
        public static void InitializeBundle(string bundleId, int totalTasks)
        {
            Tasks[bundleId] = new TaskStatus(totalTasks);
        }
    }
}


using System.Diagnostics;
using MassTransit;
using PDI.Contracts;

namespace PDI_node.Consumers
{
    public class PrimeTaskConsumer : IConsumer<PrimeTask>
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public PrimeTaskConsumer(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }
        public async Task Consume(ConsumeContext<PrimeTask> context)
        {
            var task = context.Message;
            var primes = new List<int>();
            var stopwatch = Stopwatch.StartNew();
            var rand = new Random();
            await Task.Delay(rand.Next(100,1500));  // Delay 
            for (int i = task.RangeStart; i <= task.RangeEnd; i++)
            {
                if (IsPrime(i))
                    primes.Add(i);
                
            }
            var taskResult = new { TaskId = task.TaskId, Result = primes, Status="Partial Result"};
            for(int i=0; i< primes.Count; i++) Console.WriteLine($"{i}: {primes[i]}");
            Console.WriteLine($"Task {task.TaskId} has been processed in {stopwatch.Elapsed}");
            if (_publishEndpoint == null)
            {
                Console.WriteLine("Publish endpoint is null");
                return;
            }
            var res = new PrimeResult
            {
                TaskId = task.TaskId,
                Primes = primes,
                UnprocessedStart = null,
                BundleId = task.BundleId,
                RangeEnd = task.RangeEnd,
            };
            Console.WriteLine("TaskId" + res.TaskId);
            await _publishEndpoint.Publish(res);
        }

        private bool IsPrime(int number)
        {
            if (number < 2) return false;
            for (int i = 2; i <= Math.Sqrt(number); i++)
            {
                if (number % i == 0) return false;
            }

            return true;
        }
    }
}
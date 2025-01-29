
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using PDI.Consumers;
using PDI.Contracts;

namespace PDI.Controllers;

[ApiController]
[Route("api/primes")]
public class PrimeController : ControllerBase
{
    private readonly IBusControl _publishEndpoint;

    public PrimeController(IBusControl publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }
    [HttpPost]
    public async Task<IActionResult> DistributeTasks([FromBody] PrimeRequest request)
    {
        // Validate the request
        if (request == null)
        {
            return BadRequest("Invalid request data.");
        }


        var bundleId = request.BundleId;

       int totalRange = request.RangeEnd - request.RangeStart + 1; // Total number range
       int chunkSize =  (int)(request.MaxProcessingTime / 0.001);
       chunkSize = chunkSize > 0 ? chunkSize : 1; // Ensure chunk size is at least 1
       chunkSize = chunkSize > totalRange ? totalRange : chunkSize;
        Console.WriteLine("Chunk size:" + chunkSize);
        Console.WriteLine(bundleId);
            Console.WriteLine("Number of tasks:" + (totalRange/chunkSize));
            PrimeResultConsumer.InitializeBundle(bundleId, (totalRange/chunkSize));
       for (int i = request.RangeStart; i <= request.RangeEnd; i += chunkSize)
       {
           var task = new PrimeTask
           {
               BundleId = bundleId,
               RangeStart = i,
               RangeEnd = Math.Min(i + chunkSize - 1, request.RangeEnd),
               MaxProcessingTime = request.MaxProcessingTime,
               TaskId = Guid.NewGuid().ToString(),
           };
           Console.WriteLine(task.TaskId);
        
           await _publishEndpoint.Publish(task);
           Console.WriteLine("SENT");
           Console.WriteLine("-------------------------------------------------------------------------");


       }
       

        return Ok(new { bundleId });
    }

}

public class PrimeRequest
{
    public int RangeStart { get; set; }
    public int RangeEnd { get; set; }
    public int MaxProcessingTime { get; set; }
    
    public string BundleId { get; set; }
}
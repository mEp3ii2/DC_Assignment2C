using Microsoft.AspNetCore.Mvc;
using WebServer.Models;
using System.Linq;
using WebServer.Data;
using Microsoft.EntityFrameworkCore;

namespace WebServer.Controllers
{
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public ClientController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterClient([FromBody] Client client)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Client Data");
            }
            try
            {
                await _dbContext.Clients.AddAsync(client);
                await _dbContext.SaveChangesAsync();
                return Ok("Client registerd successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500,"Internal server error: "+ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetClients()
        {
            try
            {
                var clients = await _dbContext.Clients.ToListAsync();
                if(clients == null || !clients.Any())
                {
                    return NoContent();
                }
                return Ok(clients);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateJobStatus(int jobId, string status, string assignedClientIP, int assignedClientPort)
        {
            try
            {
                var job = await _dbContext.Jobs.FirstOrDefaultAsync(j => j.JobId == jobId);

                if (job == null)
                {
                    return NotFound("Job not found");
                }

                job.Status = status;
                job.AssignedClientIP = assignedClientIP;
                job.AssignedClientPort = assignedClientPort;
                job.LastUpdated = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                return Ok("Job status updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }


        [HttpDelete]
        public async Task<IActionResult> RemoveClient(string ipAddr, int port)
        {
            try
            {
                var client = await _dbContext.Clients
                    .FirstOrDefaultAsync(c => c.IPAddr == ipAddr && c.Port == port);

                if (client == null)
                {
                    return NotFound("Client not found");
                }

                _dbContext.Clients.Remove(client);
                await _dbContext.SaveChangesAsync();

                return Ok("Client removed successfully");
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckForJobs()
        {
            try
            {
                var availableJobs = await _dbContext.Jobs
                    .Where(j => j.Status == "Pending")
                    .ToListAsync();

                if (availableJobs == null || !availableJobs.Any())
                {
                    return NoContent();  // No jobs available
                }

                return Ok(availableJobs);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitJobResults(int jobId, string result)
        {
            try
            {
                var job = await _dbContext.Jobs.FirstOrDefaultAsync(j => j.JobId == jobId);

                if (job == null)
                {
                    return NotFound("Job not found");
                }

                job.Status = "Completed";
                job.Result = result;
                job.CompletedAt = DateTime.Now;
                job.LastUpdated = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                return Ok("Job results submitted successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegisterJob([FromBody] Job newJob)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid job data");
            }

            try
            {
                newJob.Status = "Pending";
                newJob.LastUpdated = DateTime.Now;

                await _dbContext.Jobs.AddAsync(newJob);
                await _dbContext.SaveChangesAsync();

                return Ok("Job registered successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }






    }
}

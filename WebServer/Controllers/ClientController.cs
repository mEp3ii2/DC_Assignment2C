using Microsoft.AspNetCore.Mvc;
using WebServer.Models;
using System.Linq;
using WebServer.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;
using APIClasses;
using System.Runtime.Remoting;



namespace WebServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public ClientController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        [Route("RegisterClient")]
        public async Task<IActionResult> RegisterClient([FromBody] Models.Client client)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Client Data");
            }
            try
            {
                client.LastUpdated = DateTime.Now;
                await _dbContext.Clients.AddAsync(client);
                await _dbContext.SaveChangesAsync();
                return Ok(new { ClientID = client.ClientID });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetClients")]
        public async Task<IActionResult> GetClients()
        {
            try
            {
                var clients = await _dbContext.Clients.ToListAsync();
                if (clients == null || !clients.Any())
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




        [HttpDelete]
        [Route("RemoveClient")]
        public async Task<IActionResult> RemoveClient(string ipAddr, string port)
        {
            try
            {
                //var client = await _dbContext.Clients
                //   .FirstOrDefaultAsync(c => c.IPAddr.Equals(ipAddr) && c.Port.Equals(port));
                var client = await _dbContext.Clients
                .FromSqlRaw("SELECT * FROM Clients WHERE IPAddr = {0} AND Port = {1}", ipAddr, port)
                .FirstOrDefaultAsync();

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
                
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost]
        [Route("UpdateActivity")]
        public async Task<IActionResult> UpdateActivity(int clientId)
        {
            try
            {
                var client = await _dbContext.Clients.FirstOrDefaultAsync(c => c.ClientID == clientId);
                if (client == null)
                {
                    return NotFound("Client Not Found");
                }

                client.LastUpdated = DateTime.Now;
                _dbContext.Clients.Update(client);
                await _dbContext.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Intenal server error: " + ex.Message);
            }
        }

        [HttpPost]
        [Route("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus([FromBody] CurrentStatus status)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Data");
            }
            try
            {
                var client = await _dbContext.Clients
                                    .FirstOrDefaultAsync(c => c.ClientID == status.ClientID);

                if (client == null)
                {
                    return NotFound("Client not found.");
                }

                client.LastUpdated = DateTime.Now;
                client.JobsCompleted = status.jobCompleted;
                client.JobsPosted = status.jobsPosted;
                
                _dbContext.Clients.Update(client);
                await _dbContext.SaveChangesAsync();

                return Ok("Status updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

    }
}

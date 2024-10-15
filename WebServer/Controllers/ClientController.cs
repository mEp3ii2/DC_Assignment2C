using Microsoft.AspNetCore.Mvc;
using WebServer.Models;
using System.Linq;
using WebServer.Data;
using Microsoft.EntityFrameworkCore;

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
        [Route("GetClients")]
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

        


        [HttpDelete]
        [Route("RemoveClient")]
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
    }
}

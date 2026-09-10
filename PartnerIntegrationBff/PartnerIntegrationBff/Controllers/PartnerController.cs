using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PartnerIntegrationBff.Controllers
{
    [Route("api/partner")]
    [ApiController]
    public class PartnerController : ControllerBase
    {
        private static readonly Random _random = new Random();

        public PartnerController()
        {
        }

        [HttpGet("verify/{partnerId}")]
        public async Task<IActionResult> PartnerVerificationAPI(string partnerId)
        {
            await Task.Delay(_random.Next(100, 2000)); 

            var isSuccess = _random.Next(0, 100) >= 30;
            if (isSuccess)
            {
                return Ok(new
                {
                    partnerId = partnerId,
                    isValid = isSuccess,
                    VerifiedAt = DateTime.UtcNow
                });
            }
            else
            {
                throw new TimeoutException("Simulated timeout exception for partner verification.");
            }
        }
    }
}

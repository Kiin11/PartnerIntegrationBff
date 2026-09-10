using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PartnerIntegrationBff.Constant;
using PartnerIntegrationBff.Interfact;
using PartnerIntegrationBff.Models.Request;
using System.Text.Json.Serialization;
using System.Threading;

namespace PartnerIntegrationBff.Controllers
{
    [Route("api/v1/partner/transactions")]
    [ApiController]
    [Produces("application/json")]
    public class PartnerTransactionController : ControllerBase
    {
        private readonly IValidator<PartnerTransactionRequest> _validator;
        private readonly IPartnerVerificationClient _partnerClient;

        public PartnerTransactionController(IValidator<PartnerTransactionRequest> validator,
            IPartnerVerificationClient partnerVerificationClient)
        {
            _validator = validator;
            _partnerClient = partnerVerificationClient;
        }

        [HttpPost]
        public async Task<IActionResult> PostTransaction([FromBody] PartnerTransactionRequest request)
        {
            // Add Validate request body
            var validdationResult = await _validator.ValidateAsync(request);

            if(!validdationResult.IsValid)
            {
                var errors = validdationResult.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                return ValidationProblem(new ValidationProblemDetails(errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Failed",
                    Detail = "One or more validation errors occurred."
                });
            }

            // Verification API - Resilience Retry 
            var isVerified = await _partnerClient.VerifyPartnerAsync(request.PartnerId);

            if (!isVerified)
            {
                return UnprocessableEntity(new ProblemDetails
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Title = "Partner Verification Failed",
                    Detail = $"Partner '{request.PartnerId}' could not be verified or external verification service is unavailable."
                });
            }

            if (request == null)
            {
                return BadRequest(new ProblemDetails()
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid payload",
                    Detail = "Request body cannot be null."
                });
            }
           
            return Accepted(new
            {
                Message = "Transaction accepted for processing",
                TransactionReference = request.TransactionReference,
                Status = "Queued"
            });
        }
    }
}

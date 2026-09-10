using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PartnerIntegrationBff.Constant;
using PartnerIntegrationBff.Models.Request;
using PartnerIntegrationBff.Models.Response;
using System.Text.Json.Serialization;

namespace PartnerIntegrationBff.Controllers
{
    [Route("api/v1/partner/transactions")]
    [ApiController]
    [Produces("application/json")]
    public class PartnerTransactionController : ControllerBase
    {
        private readonly IValidator<PartnerTransactionRequest> _validator;

        public PartnerTransactionController(IValidator<PartnerTransactionRequest> validator)
        {
            _validator = validator;
        }

        [HttpPost]
        public async Task<IActionResult> PostTransaction([FromBody] PartnerTransactionRequest request)
        {
            var result = new BaseResponse<string>();
            if (request == null)
            {
                return BadRequest(new ProblemDetails()
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid payload",
                    Detail = "Request body cannot be null."
                });
            }

            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());    
                return ValidationProblem(new ValidationProblemDetails(errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Failed",
                    Detail = "One or more validation errors occurred."
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

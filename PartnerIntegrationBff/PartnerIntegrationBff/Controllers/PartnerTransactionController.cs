using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PartnerIntegrationBff.Constant;
using PartnerIntegrationBff.Interfact;
using PartnerIntegrationBff.Models;
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
        private readonly IMessageProducer _messageProducer;
        private readonly IConfiguration _configuration;

        public PartnerTransactionController(IValidator<PartnerTransactionRequest> validator,
            IPartnerVerificationClient partnerVerificationClient,
            IConfiguration configuration,
            IMessageProducer messageProducer)
        {
            _validator = validator;
            _partnerClient = partnerVerificationClient;
            _configuration = configuration;
            _messageProducer = messageProducer;
        }

        [HttpPost]
        public async Task<IActionResult> PostTransaction([FromBody] PartnerTransactionRequest request)
        {
            if (request == null)
            {
                return BadRequest(new ProblemDetails()
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid payload",
                    Detail = "Request body cannot be null."
                });
            }

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

            // Partner Verify : Verification API - Resilience Retry 
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

            var queueName = _configuration["RabbitMQ:TransactionsQueue"] ?? "partner_transactions_queue";
            var enrichedMessage = new TransactionEnrichedMessage()
            {
                PartnerId = request.PartnerId,
                TransactionReference = request.TransactionReference,
                Amount = request.Amount,
                Currency = request.Currency,
                Timestamp = request.Timestamp,
                IngestedAt = DateTime.UtcNow,
                Status = "QUEUED"
            };

            await _messageProducer.PublishMessageAsync(queueName, enrichedMessage);

            return Accepted(new
            {
                Message = "Transaction accepted for processing",
                TransactionReference = request.TransactionReference,
                Status = "Queued"
            });
        }
    }
}

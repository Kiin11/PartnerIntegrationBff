using FluentAssertions;
using PartnerIntegrationBff.Models;
using PartnerIntegrationBff.Models.Request;
using PartnerIntegrationBff.Models.Validator;
using Xunit;

namespace PartnerIntegration.UnitTests
{
    [TestClass]
    public sealed class PartnerTransactionValidatorTests
    {
        private readonly PartnerTransactionValidator _validator = new();

        private PartnerTransactionRequest CreateValidRequest()
        {
            return new PartnerTransactionRequest
            {
                PartnerId = "P-1001",
                TransactionReference = "TXN-99823",
                Amount = 250.00m,
                Currency = "USD",
                Timestamp = DateTime.UtcNow.AddMinutes(-1)
            };
        }

        [Fact]
        public void Validate_WhenPayloadIsValid()
        {
            var model = CreateValidRequest();

            var result = _validator.Validate(model);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task Validate_WhenPartnerIdIsEmptyOrNull(string? invalidPartnerId)
        {
            var model = CreateValidRequest();
            model.PartnerId = invalidPartnerId;

            var result = await _validator.ValidateAsync(model);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(PartnerTransactionRequest.PartnerId));
        }


        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100.50)]
        public async Task Validate_WhenAmountIsZeroOrNegative(decimal invalidAmount)
        {
            var model = CreateValidRequest();
            model.Amount = invalidAmount;

            var result = await _validator.ValidateAsync(model);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(PartnerTransactionRequest.Amount)
                                               && e.ErrorMessage.Contains("greater than zero"));
        }

        [Theory]
        [InlineData("XYZ")]
        [InlineData("US")]
        [InlineData("123")]
        [InlineData("")]
        [InlineData(null)]
        public async Task Validate_WhenCurrencyIsUnsupportedOrInvalid(string? invalidCurrency)
        {
            var model = CreateValidRequest();
            model.Currency = invalidCurrency;

            var result = await _validator.ValidateAsync(model);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(PartnerTransactionRequest.Currency));
        }

        [Theory]
        [InlineData("usd")]
        [InlineData("VND")]
        [InlineData("Eur")]
        public async Task Validate_WhenCurrencyIsValidCaseInsensitive(string validCurrency)
        {
            var model = CreateValidRequest();
            model.Currency = validCurrency;

            var result = await _validator.ValidateAsync(model);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Validate_WhenTimestampIsInFarFuture()
        {
            var model = CreateValidRequest();
            model.Timestamp = DateTime.UtcNow.AddDays(1); 

            var result = await _validator.ValidateAsync(model);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(PartnerTransactionRequest.Timestamp));
        }
    }
}

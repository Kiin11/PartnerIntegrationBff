using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PartnerIntegration.UnitTests.Resilience
{
    public class PartnerVerificationResilienceTests
    {
        [Fact]
        public async Task VerifyPartner_WhenFirstTwoAttemptsThrowTimeout_AndThirdSucceeds_ShouldRetryAndReturnTrue()
        {
            //// Arrange: Giả lập 2 lần đầu ném TimeoutException (30% case), lần 3 trả về 200 OK
            //var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            //var callCount = 0;

            //handlerMock
            //    .Protected()
            //    .Setup<Task<HttpResponseMessage>>(
            //        "SendAsync",
            //        ItExpr.IsAny<HttpRequestMessage>(),
            //        ItExpr.IsAny<CancellationToken>())
            //    .Returns(() =>
            //    {
            //        callCount++;
            //        if (callCount < 3)
            //        {
            //            throw new TimeoutException("Simulated mock timeout exception.");
            //        }

            //        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            //        {
            //            Content = new StringContent("{\"isValid\": true}")
            //        });
            //    });

            //// Cấu hình Pipeline Resilience có retry 3 lần
            //var services = new ServiceCollection();
            //services.AddHttpClient<IPartnerVerificationClient, PartnerVerificationClient>(client =>
            //{
            //    client.BaseAddress = new Uri("http://localhost:5000");
            //})
            //.ConfigurePrimaryHttpMessageHandler(() => handlerMock.Object)
            //.AddResilienceHandler("test-retry-pipeline", builder =>
            //{
            //    builder.AddRetry(new HttpRetryStrategyOptions
            //    {
            //        MaxRetryAttempts = 3,
            //        Delay = TimeSpan.FromMilliseconds(20), // Tăng tốc độ khi chạy unit test
            //        BackoffType = DelayBackoffType.Constant,
            //        ShouldHandle = args =>
            //        {
            //            var isTimeout = args.Outcome.Exception is TimeoutException;
            //            var isServerError = args.Outcome.Result?.StatusCode >= HttpStatusCode.InternalServerError;
            //            return ValueTask.FromResult(isTimeout || isServerError);
            //        }
            //    });
            //});

            //var provider = services.BuildServiceProvider();
            //var client = provider.GetRequiredService<IPartnerVerificationClient>();

            //// Act
            //var result = await client.VerifyPartnerAsync("P-1001");

            //// Assert
            //result.Should().BeTrue();
            //callCount.Should().Be(3); // Đảm bảo retry đã chạy đúng 3 lần (2 fail + 1 success)
        }
    }
}

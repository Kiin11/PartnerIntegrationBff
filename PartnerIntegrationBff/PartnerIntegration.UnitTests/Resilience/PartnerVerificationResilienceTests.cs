using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using PartnerIntegrationBff.Business;
using PartnerIntegrationBff.Interfact;
using Polly;
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
        public async Task VerifyPartnerAsync_CallApiSuccess()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"partnerId\":\"P-1001\",\"isValid\":true}")
            });

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:5000")
            };

            var client = new PartnerVerificationClient(httpClient);

            // Act
            var result = await client.VerifyPartnerAsync("P-1001");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task VerifyPartnerAsync_CallApiFail()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:5000")
            };
            var client = new PartnerVerificationClient(httpClient);
            // Act
            var result = await client.VerifyPartnerAsync("P-1001");
            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task VerifyPartnerAsync_CallFailforOneAndTwo_WhenCallThirdSuccess()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var callCount = 0;

            handlerMock.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(() =>
            {
                callCount++;
                if (callCount < 3)
                {
                    return Task.FromException<HttpResponseMessage>(new HttpRequestException("Server TimeOut", new TimeoutException("Server TimeOut"), System.Net.HttpStatusCode.GatewayTimeout));
                }
                else
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("{\"partnerId\":\"P-1001\",\"isValid\":true}")
                    });
                }
            });

            // Registry the HttpClient with Polly retry policy
            var services = new ServiceCollection();
            services.AddHttpClient<IPartnerVerificationClient, PartnerVerificationClient>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:5000");
            }).ConfigurePrimaryHttpMessageHandler(() => handlerMock.Object)
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromMilliseconds(10);
                options.Retry.BackoffType = DelayBackoffType.Constant;
                options.Retry.UseJitter = true;

                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
            });

            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IPartnerVerificationClient>();

            // Act
            var result = await client.VerifyPartnerAsync("P-1001");

            // Assert
            result.Should().BeTrue();
            callCount.Should().Be(3); // 2 lần fail + 1 lần retry thành công
        }

        [Fact]
        public async Task VerifyPartnerAsync_CallFailforAllAttempts()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var callCount = 0;
            handlerMock.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(() =>
            {
                callCount++;
                return Task.FromException<HttpResponseMessage>(new HttpRequestException("Server TimeOut", new TimeoutException("Server TimeOut"), System.Net.HttpStatusCode.GatewayTimeout));
            });
            // Registry the HttpClient with Polly retry policy
            var services = new ServiceCollection();
            services.AddHttpClient<IPartnerVerificationClient, PartnerVerificationClient>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:5000");
            }).ConfigurePrimaryHttpMessageHandler(() => handlerMock.Object)
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromMilliseconds(10);
                options.Retry.BackoffType = DelayBackoffType.Constant;
                options.Retry.UseJitter = true;
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
            });
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IPartnerVerificationClient>();
            // Act
            var result = await client.VerifyPartnerAsync("P-1001");
            // Assert
            result.Should().BeFalse();
            callCount.Should().Be(4); // 3  fail + 1 retry final
        }
    }
}

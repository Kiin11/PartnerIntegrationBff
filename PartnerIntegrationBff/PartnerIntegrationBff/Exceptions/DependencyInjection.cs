using PartnerIntegrationBff.Models.Validator;
using FluentValidation;
using PartnerIntegrationBff.Interfact;
using PartnerIntegrationBff.Business;
using Polly;

namespace PartnerIntegrationBff.Exceptions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddDistributedMemoryCache();

            services.AddValidatorsFromAssemblyContaining<PartnerTransactionValidator>();

            services.AddHttpClient<IPartnerVerificationClient, PartnerVerificationClient>(client =>
            {
                client.BaseAddress = new Uri(configuration["PartnerService:BaseUrl"] ?? "http://localhost:5000");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
              .AddStandardResilienceHandler(options =>
              {
                  options.Retry.MaxRetryAttempts = 3;
                  options.Retry.Delay = TimeSpan.FromMilliseconds(200);
                  options.Retry.BackoffType = DelayBackoffType.Exponential;
                  options.Retry.UseJitter = true;

                  options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
              });

            return services;
        }
    }
}

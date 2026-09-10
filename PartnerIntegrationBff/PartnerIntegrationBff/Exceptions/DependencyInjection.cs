using PartnerIntegrationBff.Models.Validator;
using FluentValidation;

namespace PartnerIntegrationBff.Exceptions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddDistributedMemoryCache();

            services.AddValidatorsFromAssemblyContaining<PartnerTransactionValidator>();

            return services;
        }
    }
}

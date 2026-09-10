using PartnerIntegrationBff.Interfact;

namespace PartnerIntegrationBff.Business
{
    public class PartnerVerificationClient : IPartnerVerificationClient
    {
        private readonly HttpClient _httpClient;

        public PartnerVerificationClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> VerifyPartnerAsync(string partnerId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/partner/verify/{partnerId}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

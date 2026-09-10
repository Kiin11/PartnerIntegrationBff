namespace PartnerIntegrationBff.Interfact
{
    public interface IPartnerVerificationClient
    {
        Task<bool> VerifyPartnerAsync(string partnerId, CancellationToken cancellationToken = default);
    }
}

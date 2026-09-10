namespace PartnerIntegrationBff.Interfact
{
    public interface IMessageProducer
    {
        Task PublishMessageAsync<T>(string queueName, T message, CancellationToken cancellationToken = default);
    }
}

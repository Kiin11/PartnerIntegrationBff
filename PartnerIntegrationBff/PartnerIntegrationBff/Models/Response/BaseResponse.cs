using PartnerIntegrationBff.Constant;

namespace PartnerIntegrationBff.Models.Response
{
    public class BaseResponse<T>
    {
        public StatusCodeResponseEnum Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public T Result { get; set; }
    }
}

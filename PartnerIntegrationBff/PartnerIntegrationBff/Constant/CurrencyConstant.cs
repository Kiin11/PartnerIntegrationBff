namespace PartnerIntegrationBff.Constant
{
    public class CurrencyConstant
    {
        public const string USD = "USD";
        public const string EUR = "EUR";
        public const string GBP = "GBP";
        public const string JPY = "JPY";
        public const string AUD = "AUD";
        public const string CAD = "CAD";
        public const string VND = "VND";

        public static readonly HashSet<string> SupportedCurrencies = new(StringComparer.OrdinalIgnoreCase)
        {
            USD, EUR, GBP, JPY, AUD, CAD, VND
        };
    }

}

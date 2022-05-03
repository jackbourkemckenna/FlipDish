
namespace Flipdish.Recruiting.WebHookReceiver.Helpers
{
    static class CurrencyFilter
    {
        public static string currency(decimal input)
        {
            return input.ToString("N2");
        }
    }
}

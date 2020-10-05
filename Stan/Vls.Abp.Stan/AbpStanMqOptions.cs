namespace Vls.Abp.Stan
{
    public class AbpStanMqOptions
    {
        public string ClusterId { get; set; }
        public string ClientId { get; set; }
        public string Url { get; set; }
        public int ConnectionTimeout { get; set; }
    }
}

namespace Vls.Abp.Nats.Client
{
    public class ProxyOptions
    {
        public static ProxyOptions Default => new ProxyOptions()
        {
            ServiceUid = "default"
        };

        /// <summary>
        /// Remote service uid
        /// </summary>
        public string ServiceUid { get; set; }
    }
}

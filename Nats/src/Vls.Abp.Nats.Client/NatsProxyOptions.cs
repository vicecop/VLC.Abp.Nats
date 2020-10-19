namespace Vls.Abp.Examples.Client
{
    public class NatsProxyOptions
    {
        public static NatsProxyOptions Default => new NatsProxyOptions()
        {
            ServiceUid = "default"
        };

        /// <summary>
        /// Remote service uid
        /// </summary>
        public string ServiceUid { get; set; }
    }
}

namespace Vls.Abp.Nats.Hubs
{
    public class HubServiceOptions
    {
        public static HubServiceOptions Default => new HubServiceOptions()
        {
            ServiceUid = "default"
        };

        public string ServiceUid { get; set; }
    }
}

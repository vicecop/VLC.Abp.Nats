namespace Vls.Abp.Nats.Hubs
{
    public class HubServiceOptions
    {
        public static HubServiceOptions Default => new HubServiceOptions()
        {
            ServiceUid = "default",
            ConnectionString = "nats://localhost:4222"
        };

        public string ServiceUid { get; set; }
        public string ConnectionString { get; set; }
    }
}

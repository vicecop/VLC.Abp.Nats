namespace Vls.Abp.Examples.Hubs
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

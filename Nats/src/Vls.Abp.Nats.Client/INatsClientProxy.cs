namespace Vls.Abp.Examples.Client
{

    public interface INatsClientProxy<out TRemoteService>
    {
        TRemoteService Service { get; }
    }
}

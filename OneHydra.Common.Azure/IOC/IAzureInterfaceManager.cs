namespace OneHydra.Common.Azure.IOC
{
    public interface IAzureInterfaceManager
    {
        T GetInstanceOf<T>();
    }
}

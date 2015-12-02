using Microsoft.Practices.Unity;
using OneHydra.Common.Azure.Storage;
using OneHydra.Common.Azure.Storage.Interfaces;
using OneHydra.Common.Utilities.Configuration;

namespace OneHydra.Common.Azure.IOC
{
    public class AzureInterfaceManager : IAzureInterfaceManager
    {

        private static readonly IUnityContainer InstanceContainer;

        static AzureInterfaceManager()
        {
            InstanceContainer = new UnityContainer();
            InstanceContainer.RegisterInstance<IBlobManager>(new BlobManager(new ConfigManagerHelper()));
            InstanceContainer.RegisterInstance<ITableManager>(new TableManager(new ConfigManagerHelper()));
        }

        public T GetInstanceOf<T>()
        {
            return InstanceContainer.Resolve<T>();
        }
    }
}

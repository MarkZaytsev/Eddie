using FrostLib.Containers;

namespace FrostLib.Services
{
    public class ServiceGroup : DisposableGroup, IProvider
    {
        private readonly IProvider _servicer;

        public ServiceGroup(IProvider servicer) => _servicer = servicer;

        public void Provide<T>(T service, string tag = "")
        {
            _servicer.Provide(service, tag);
            Add(() => { _servicer.Remove<T>(); });
        }

        void IProvider.Remove<T>(string tag) => throw new System.NotImplementedException();
    }
}
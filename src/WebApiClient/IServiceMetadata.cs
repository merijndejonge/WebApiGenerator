using Microsoft.Extensions.DependencyInjection;

namespace OpenSoftware.WebApiClient
{
    public interface IServiceMetadata
    {
        void ConfigureServices(IServiceCollection services);
    }
}
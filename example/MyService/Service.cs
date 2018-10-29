using Microsoft.Extensions.DependencyInjection;
using OpenSoftware.WebApiClient;

namespace MyService
{
    public class Service : IServiceMetadata
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IStringUtils, StringUtils>();
        }
    }
}
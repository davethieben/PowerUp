using Microsoft.Extensions.DependencyInjection;

namespace PowerUp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<MainWindow>();

        }

    }
}

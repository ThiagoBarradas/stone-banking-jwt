using Microsoft.Extensions.Configuration;
using StoneBanking.Jwt;
using StoneBanking.Jwt.Models;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class StoneBankingServiceExtension
    {
        public static void AddStoneBankingJwt(this IServiceCollection services, IConfigurationRoot configurationRoot)
        {
            if (services == null)
            {
                throw new ArgumentNullException("ServiceCollection cannot be null");
            }

            if (configurationRoot == null)
            {
                throw new ArgumentNullException("ConfigurationRoot cannot be null. Try add 'services.AddSingleton(Configuration);' before 'services.AddStoneBankingJwt();'");
            }

            var section = configurationRoot.GetSection("StoneBankingJwt");
            if (!section.Exists())
            {
                throw new ArgumentException("ConfigurationRoot must have a section with name StoneBankingJwt.");
            }

            var stoneBankingSettings = new StoneBankingSettings();
            configurationRoot.GetSection("StoneBankingJwt").Bind(stoneBankingSettings);

            AddStoneBankingJwt(services, stoneBankingSettings);
        }

        public static void AddStoneBankingJwt(this IServiceCollection services, Action<StoneBankingSettings> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException("ServiceCollection cannot be null");
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException("SetupAction cannot be null.");
            }

            var stoneBankingSettings = new StoneBankingSettings();
            setupAction(stoneBankingSettings);

            AddStoneBankingJwt(services, stoneBankingSettings);
        }

        public static void AddStoneBankingJwt(this IServiceCollection services, StoneBankingSettings stoneBankingSettings)
        {
            if (services == null)
            {
                throw new ArgumentNullException("ServiceCollection cannot be null");
            }

            if (stoneBankingSettings == null)
            {
                throw new ArgumentNullException("StoneBankingSettings cannot be null.");
            }

            services.AddSingleton(provider => stoneBankingSettings);
            services.AddSingleton<IStoneBankingJwt, StoneBankingJwt>();
        }

        public static void AddStoneBankingJwt(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException("ServiceCollection cannot be null");
            }

            var configuration = services.BuildServiceProvider().GetService<IConfigurationRoot>();

            AddStoneBankingJwt(services, configuration);
        }
    }
}

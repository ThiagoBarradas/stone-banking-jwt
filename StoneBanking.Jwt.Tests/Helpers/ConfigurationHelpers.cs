using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace StoneBanking.Jwt.Tests.Helpers
{
    public class ConfigurationHelpers
    {
        public static IConfigurationRoot LoadConfigurations(string customJsonName = null, string envVarsPrefix = null)
        {
            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{customJsonName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables(envVarsPrefix);

            return builder.Build();
        }
    }
}

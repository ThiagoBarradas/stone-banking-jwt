using Microsoft.Extensions.DependencyInjection;
using StoneBanking.Jwt.Models;
using StoneBanking.Jwt.Tests.Helpers;
using System;
using Xunit;

namespace StoneBanking.Jwt.Tests
{
    public static class ServiceColletionTests
    {
        [Fact]
        public static void LoadConfig_AppSettingsWithRsaFile_AutoGettingConfiguration()
        {
            // arrange
            var configuration = ConfigurationHelpers.LoadConfigurations("RsaByFile");
            var services = new ServiceCollection();
            services.AddSingleton(configuration);

            // act
            services.AddStoneBankingJwt();
            var provider = services.BuildServiceProvider();

            var settings = provider.GetService<StoneBankingSettings>();
            var jwtSign = provider.GetService<IStoneBankingJwt>();

            // assert
            Assert.Equal(settings.ClientId, "123123123");
            Assert.Equal(settings.ConsentExpiresInSeconds, 300);
            Assert.Equal(settings.AuthenticationExpiresInSeconds, 900);
            Assert.Equal(settings.Environment, StoneBankingEnvironment.Sandbox);
            Assert.Equal(settings.ConsentDefaultRedirectUrl, "https://mysite.com/stonebanking/success");
            Assert.Equal(settings.PublicKey, "rsa-publickey-file.pub");
            Assert.Equal(settings.PrivateKey, "rsa-privatekey-file.pem");

            var jwtSignObj = ((StoneBankingJwt)jwtSign);
            Assert.Equal(jwtSignObj.PublicKey, "-----BEGIN PUBLIC KEY-----\nMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEA+VDuOiFW4qYiivOE+9sG\n7XAU2DqkGXSBRkNDyGooQEVAR2fGPn8PHxTND0TQFmkRLUxuOCkGnLTMCVRfO7L9\nv1SFllzCl3KC9zQeTlrmEDcpnSsEMxQ+W9UISIhr4pBhv9Afj0WOlRYb9uaOpyic\nqzCPLia+KQkMsZlG7m9sK6VtdeugRezLbNQ/NbmU74Q+i63BeYdgJGVA8vZrQ+4g\nxei9n5vA97iuxVDn8BK9B2UAskPY4nZ4i/4d+5Mrp++hAHnusFLcscJ2GrsXDbuw\nqSCbqhgRuEpzJPibhWYdF8y71pjwFVoGiVnWV7xqe4PXX3MzQI+Wp/jMwiY6HOJJ\ngkoYjU4dVO5n7iC7Cdm6OIcropuoHfp2GicLOxnKya+z1JvkpFU09NII8+CuMN79\nxDm+04Xh4J0/OmmAWg+NtZabKslmH0/So6JY6xp13lBvbACvqDpf5QQllZyyUamc\nYouUIAZqYYWQIHcr5+dFGgkf+UtW8BX2/Dh65P6mDjnnfw2jvTL8PgUSbowuUDlx\njoevrSiZ6QOkNKTKLXzncgWTvNXp5/JvYdP9JzRhE6jjXE3gYCfty3CenvQs6sGE\nZyT+l48lkFn4F/HoSwsNo3QbkXYrmyeyvwvC6yvXnHNDesuDoKm/9eTQRD7YUDqO\nZhsvGlk73ak8USX94qSpOacCAwEAAQ==\n-----END PUBLIC KEY-----\n");
            Assert.Equal(jwtSignObj.PrivateKey, "-----BEGIN RSA PRIVATE KEY-----\n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\n-----END RSA PRIVATE KEY-----\n");
        }

        [Fact]
        public static void LoadConfig_AppSettingsWithRsaFile()
        {
            // arrange
            var configuration = ConfigurationHelpers.LoadConfigurations("RsaByFile");
            var services = new ServiceCollection();

            // act
            services.AddStoneBankingJwt(configuration);
            var provider = services.BuildServiceProvider();

            var settings = provider.GetService<StoneBankingSettings>();
            var jwtSign = provider.GetService<IStoneBankingJwt>();

            // assert
            Assert.Equal(settings.ClientId, "123123123");
            Assert.Equal(settings.ConsentExpiresInSeconds, 300);
            Assert.Equal(settings.AuthenticationExpiresInSeconds, 900);
            Assert.Equal(settings.Environment, StoneBankingEnvironment.Sandbox);
            Assert.Equal(settings.ConsentDefaultRedirectUrl, "https://mysite.com/stonebanking/success");
            Assert.Equal(settings.PublicKey, "rsa-publickey-file.pub");
            Assert.Equal(settings.PrivateKey, "rsa-privatekey-file.pem");

            var jwtSignObj = ((StoneBankingJwt)jwtSign);
            Assert.Equal(jwtSignObj.PublicKey, "-----BEGIN PUBLIC KEY-----\nMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEA+VDuOiFW4qYiivOE+9sG\n7XAU2DqkGXSBRkNDyGooQEVAR2fGPn8PHxTND0TQFmkRLUxuOCkGnLTMCVRfO7L9\nv1SFllzCl3KC9zQeTlrmEDcpnSsEMxQ+W9UISIhr4pBhv9Afj0WOlRYb9uaOpyic\nqzCPLia+KQkMsZlG7m9sK6VtdeugRezLbNQ/NbmU74Q+i63BeYdgJGVA8vZrQ+4g\nxei9n5vA97iuxVDn8BK9B2UAskPY4nZ4i/4d+5Mrp++hAHnusFLcscJ2GrsXDbuw\nqSCbqhgRuEpzJPibhWYdF8y71pjwFVoGiVnWV7xqe4PXX3MzQI+Wp/jMwiY6HOJJ\ngkoYjU4dVO5n7iC7Cdm6OIcropuoHfp2GicLOxnKya+z1JvkpFU09NII8+CuMN79\nxDm+04Xh4J0/OmmAWg+NtZabKslmH0/So6JY6xp13lBvbACvqDpf5QQllZyyUamc\nYouUIAZqYYWQIHcr5+dFGgkf+UtW8BX2/Dh65P6mDjnnfw2jvTL8PgUSbowuUDlx\njoevrSiZ6QOkNKTKLXzncgWTvNXp5/JvYdP9JzRhE6jjXE3gYCfty3CenvQs6sGE\nZyT+l48lkFn4F/HoSwsNo3QbkXYrmyeyvwvC6yvXnHNDesuDoKm/9eTQRD7YUDqO\nZhsvGlk73ak8USX94qSpOacCAwEAAQ==\n-----END PUBLIC KEY-----\n");
            Assert.Equal(jwtSignObj.PrivateKey, "-----BEGIN RSA PRIVATE KEY-----\n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\n-----END RSA PRIVATE KEY-----\n");
        }

        [Fact]
        public static void LoadConfig_AppSettingsWithRsaString()
        {
            // arrange
            var configuration = ConfigurationHelpers.LoadConfigurations("RsaByString");
            var services = new ServiceCollection();
            var privateKey = "-----BEGIN RSA PRIVATE KEY-----\n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\n-----END RSA PRIVATE KEY-----\n";
            var publicKey = "-----BEGIN PUBLIC KEY-----\nMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAr49YBXfHZkPxXAQ+uHWc\nHNMLt2PY0t1G5TvAV6+BQBvv64BASjVe0uBHw8LOeQ1Gu5aQDyxUm5PDzORrkK+y\n9YaK+2X24XkRVuR9v4ZD5eQXKLanNm/6EGPmeHfsmhI3ecgjePs+M7kEkcVcloO6\nEPEZdhf32NLV+tY6D4FKD3sJ+RhV7CmKoBi6Befwlmpo64kStdzlaq2s3jMNvxWM\nH4rjuYglC3LKNsTzbKBqyS+aqFOfE5MRCCyAlusN/vQ6cD3/AHzTbVpNgohk6T0c\np/7iZSAhD3T6/HmXEALrenNoMZrzQES5Qbf2XgozQvh0j/UONYVWA6cfkY7+gtte\nwcENe1h+Rm+5inWCc7aEKAlldJrZsI5jH90yewoFtcgvsKl18OQC//H/Hix1g8x5\ndNaje9H9WtKYwtPtu9xzUeO0sFtFra1qknGKvCXP+H1t4mX9VnmUMx+2iTZfss4V\nrJvEaw/mNbJx1w0sjQioJpsik8PqV4B//ZVK8xMcY4P/H8ly5eTHJB91NBcZZiy8\nnUGMbybV8SJH4FmqnFfIIMJODQao6XhbkQ0IFkqWmWR4gyLaIKQ2QIKW/uzB5ClL\nTLq6Pkc0pkP9zhmrBKw5ejJRzyIRAksodp/lXsyBMOhw2cAK30SjYeAWKH0BmTfV\nd2GzlXHVfGLqk5wWZl3lzoUCAwEAAQ==\n-----END PUBLIC KEY-----\n";

            // act
            services.AddStoneBankingJwt(configuration);
            var provider = services.BuildServiceProvider();

            var settings = provider.GetService<StoneBankingSettings>();
            var jwtSign = provider.GetService<IStoneBankingJwt>();

            // assert
            Assert.Equal(settings.ClientId, "321321321");
            Assert.Equal(settings.ConsentExpiresInSeconds, 600);
            Assert.Equal(settings.AuthenticationExpiresInSeconds, 1800);
            Assert.Equal(settings.Environment, StoneBankingEnvironment.Production);
            Assert.Equal(settings.ConsentDefaultRedirectUrl, "https://mysite.com/stonebanking/success2");
            Assert.Equal(settings.PublicKey, publicKey);
            Assert.Equal(settings.PrivateKey, privateKey);

            var jwtSignObj = ((StoneBankingJwt)jwtSign);
            Assert.Equal(jwtSignObj.PublicKey, publicKey);
            Assert.Equal(jwtSignObj.PrivateKey, privateKey);
        }

        [Fact]
        public static void LoadConfig_WithObject()
        {
            // arrange
            var services = new ServiceCollection();
            var stoneBankingSettings = new StoneBankingSettings
            {
                ClientId = "1231234",
                PrivateKey = "-----BEGIN RSA PRIVATE KEY-----\n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\n-----END RSA PRIVATE KEY-----\n",
                PublicKey = "-----BEGIN PUBLIC KEY-----\nMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAr49YBXfHZkPxXAQ+uHWc\nHNMLt2PY0t1G5TvAV6+BQBvv64BASjVe0uBHw8LOeQ1Gu5aQDyxUm5PDzORrkK+y\n9YaK+2X24XkRVuR9v4ZD5eQXKLanNm/6EGPmeHfsmhI3ecgjePs+M7kEkcVcloO6\nEPEZdhf32NLV+tY6D4FKD3sJ+RhV7CmKoBi6Befwlmpo64kStdzlaq2s3jMNvxWM\nH4rjuYglC3LKNsTzbKBqyS+aqFOfE5MRCCyAlusN/vQ6cD3/AHzTbVpNgohk6T0c\np/7iZSAhD3T6/HmXEALrenNoMZrzQES5Qbf2XgozQvh0j/UONYVWA6cfkY7+gtte\nwcENe1h+Rm+5inWCc7aEKAlldJrZsI5jH90yewoFtcgvsKl18OQC//H/Hix1g8x5\ndNaje9H9WtKYwtPtu9xzUeO0sFtFra1qknGKvCXP+H1t4mX9VnmUMx+2iTZfss4V\nrJvEaw/mNbJx1w0sjQioJpsik8PqV4B//ZVK8xMcY4P/H8ly5eTHJB91NBcZZiy8\nnUGMbybV8SJH4FmqnFfIIMJODQao6XhbkQ0IFkqWmWR4gyLaIKQ2QIKW/uzB5ClL\nTLq6Pkc0pkP9zhmrBKw5ejJRzyIRAksodp/lXsyBMOhw2cAK30SjYeAWKH0BmTfV\nd2GzlXHVfGLqk5wWZl3lzoUCAwEAAQ==\n-----END PUBLIC KEY-----\n",
            };

            // act
            services.AddStoneBankingJwt(stoneBankingSettings);
            var provider = services.BuildServiceProvider();

            var settings = provider.GetService<StoneBankingSettings>();
            var jwtSign = provider.GetService<IStoneBankingJwt>();

            // assert
            Assert.Equal(settings.ClientId, "1231234");
            Assert.Equal(settings.ConsentExpiresInSeconds, 300);
            Assert.Equal(settings.AuthenticationExpiresInSeconds, 900);
            Assert.Equal(settings.Environment, StoneBankingEnvironment.Sandbox);
            Assert.Null(settings.ConsentDefaultRedirectUrl);
            Assert.Equal(settings.PublicKey, stoneBankingSettings.PublicKey);
            Assert.Equal(settings.PrivateKey, stoneBankingSettings.PrivateKey);

            var jwtSignObj = ((StoneBankingJwt)jwtSign);
            Assert.Equal(jwtSignObj.PublicKey, stoneBankingSettings.PublicKey);
            Assert.Equal(jwtSignObj.PrivateKey, stoneBankingSettings.PrivateKey);
        }

        [Fact]
        public static void LoadConfig_WithAction()
        {
            // arrange
            var services = new ServiceCollection();

            // act
            services.AddStoneBankingJwt(options =>
            {
                options.PrivateKey = "rsa-privatekey-file.pem";
                options.PublicKey = "rsa-publickey-file.pub";
                options.ClientId = "123321";
                options.Environment = StoneBankingEnvironment.Sandbox;
            });
            
            var provider = services.BuildServiceProvider();

            var settings = provider.GetService<StoneBankingSettings>();
            var jwtSign = provider.GetService<IStoneBankingJwt>();

            // assert
            Assert.Equal(settings.ClientId, "123321");
            Assert.Equal(settings.ConsentExpiresInSeconds, 300);
            Assert.Equal(settings.AuthenticationExpiresInSeconds, 900);
            Assert.Equal(settings.Environment, StoneBankingEnvironment.Sandbox);
            Assert.Null(settings.ConsentDefaultRedirectUrl);
            Assert.Equal(settings.PublicKey, "rsa-publickey-file.pub");
            Assert.Equal(settings.PrivateKey, "rsa-privatekey-file.pem");

            var jwtSignObj = ((StoneBankingJwt)jwtSign);
            Assert.Equal(jwtSignObj.PublicKey, "-----BEGIN PUBLIC KEY-----\nMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEA+VDuOiFW4qYiivOE+9sG\n7XAU2DqkGXSBRkNDyGooQEVAR2fGPn8PHxTND0TQFmkRLUxuOCkGnLTMCVRfO7L9\nv1SFllzCl3KC9zQeTlrmEDcpnSsEMxQ+W9UISIhr4pBhv9Afj0WOlRYb9uaOpyic\nqzCPLia+KQkMsZlG7m9sK6VtdeugRezLbNQ/NbmU74Q+i63BeYdgJGVA8vZrQ+4g\nxei9n5vA97iuxVDn8BK9B2UAskPY4nZ4i/4d+5Mrp++hAHnusFLcscJ2GrsXDbuw\nqSCbqhgRuEpzJPibhWYdF8y71pjwFVoGiVnWV7xqe4PXX3MzQI+Wp/jMwiY6HOJJ\ngkoYjU4dVO5n7iC7Cdm6OIcropuoHfp2GicLOxnKya+z1JvkpFU09NII8+CuMN79\nxDm+04Xh4J0/OmmAWg+NtZabKslmH0/So6JY6xp13lBvbACvqDpf5QQllZyyUamc\nYouUIAZqYYWQIHcr5+dFGgkf+UtW8BX2/Dh65P6mDjnnfw2jvTL8PgUSbowuUDlx\njoevrSiZ6QOkNKTKLXzncgWTvNXp5/JvYdP9JzRhE6jjXE3gYCfty3CenvQs6sGE\nZyT+l48lkFn4F/HoSwsNo3QbkXYrmyeyvwvC6yvXnHNDesuDoKm/9eTQRD7YUDqO\nZhsvGlk73ak8USX94qSpOacCAwEAAQ==\n-----END PUBLIC KEY-----\n");
            Assert.Equal(jwtSignObj.PrivateKey, "-----BEGIN RSA PRIVATE KEY-----\n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\n-----END RSA PRIVATE KEY-----\n");
        }

        [Fact]
        public static void LoadConfig_AppSettingsReplacedByEnvVars()
        {
            // arrange
            Environment.SetEnvironmentVariable("TestEnv_StoneBankingJwt__ClientId", "123env");
            Environment.SetEnvironmentVariable("TestEnv_StoneBankingJwt__PublicKey", "rsa-publickey-file.pub");
            Environment.SetEnvironmentVariable("TestEnv_StoneBankingJwt__PrivateKey", "rsa-privatekey-file.pem");
            Environment.SetEnvironmentVariable("TestEnv_StoneBankingJwt__ConsentExpiresInSeconds", "200");
            Environment.SetEnvironmentVariable("TestEnv_StoneBankingJwt__AuthenticationExpiresInSeconds", "400");
            Environment.SetEnvironmentVariable("TestEnv_StoneBankingJwt__ConsentDefaultRedirectUrl", "https://mysite.com/stonebanking/success-env");
            Environment.SetEnvironmentVariable("TestEnv_StoneBankingJwt__Environment", "Production");

            var configuration = ConfigurationHelpers.LoadConfigurations(null, "TestEnv_");
            var services = new ServiceCollection();

            // act
            services.AddStoneBankingJwt(configuration);
            var provider = services.BuildServiceProvider();

            var settings = provider.GetService<StoneBankingSettings>();
            var jwtSign = provider.GetService<IStoneBankingJwt>();

            // assert
            Assert.Equal(settings.ClientId, "123env");
            Assert.Equal(settings.ConsentExpiresInSeconds, 200);
            Assert.Equal(settings.AuthenticationExpiresInSeconds, 400);
            Assert.Equal(settings.Environment, StoneBankingEnvironment.Production);
            Assert.Equal(settings.ConsentDefaultRedirectUrl, "https://mysite.com/stonebanking/success-env");
            Assert.Equal(settings.PublicKey, "rsa-publickey-file.pub");
            Assert.Equal(settings.PrivateKey, "rsa-privatekey-file.pem");

            var jwtSignObj = ((StoneBankingJwt)jwtSign);
            Assert.Equal(jwtSignObj.PublicKey, "-----BEGIN PUBLIC KEY-----\nMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEA+VDuOiFW4qYiivOE+9sG\n7XAU2DqkGXSBRkNDyGooQEVAR2fGPn8PHxTND0TQFmkRLUxuOCkGnLTMCVRfO7L9\nv1SFllzCl3KC9zQeTlrmEDcpnSsEMxQ+W9UISIhr4pBhv9Afj0WOlRYb9uaOpyic\nqzCPLia+KQkMsZlG7m9sK6VtdeugRezLbNQ/NbmU74Q+i63BeYdgJGVA8vZrQ+4g\nxei9n5vA97iuxVDn8BK9B2UAskPY4nZ4i/4d+5Mrp++hAHnusFLcscJ2GrsXDbuw\nqSCbqhgRuEpzJPibhWYdF8y71pjwFVoGiVnWV7xqe4PXX3MzQI+Wp/jMwiY6HOJJ\ngkoYjU4dVO5n7iC7Cdm6OIcropuoHfp2GicLOxnKya+z1JvkpFU09NII8+CuMN79\nxDm+04Xh4J0/OmmAWg+NtZabKslmH0/So6JY6xp13lBvbACvqDpf5QQllZyyUamc\nYouUIAZqYYWQIHcr5+dFGgkf+UtW8BX2/Dh65P6mDjnnfw2jvTL8PgUSbowuUDlx\njoevrSiZ6QOkNKTKLXzncgWTvNXp5/JvYdP9JzRhE6jjXE3gYCfty3CenvQs6sGE\nZyT+l48lkFn4F/HoSwsNo3QbkXYrmyeyvwvC6yvXnHNDesuDoKm/9eTQRD7YUDqO\nZhsvGlk73ak8USX94qSpOacCAwEAAQ==\n-----END PUBLIC KEY-----\n");
            Assert.Equal(jwtSignObj.PrivateKey, "-----BEGIN RSA PRIVATE KEY-----\n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\n-----END RSA PRIVATE KEY-----\n");
        }
    }
}

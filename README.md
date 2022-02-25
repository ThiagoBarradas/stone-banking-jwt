[![NuGet Downloads](https://img.shields.io/nuget/dt/StoneBanking.Jwt.svg)](https://www.nuget.org/packages/StoneBanking.Jwt/)
[![NuGet Version](https://img.shields.io/nuget/v/StoneBanking.Jwt.svg)](https://www.nuget.org/packages/StoneBanking.Jwt/)

# StoneBanking.Jwt

Creates Jwt Authentication Token and Consent URL by RSA key pair.

## Install via NuGet

```
PM> Install-Package StoneBanking.Jwt
```

## Stone Banking Application

See more here: https://docs.openbank.stone.com.br/docs/guias/token-de-acesso/

## Creating you RSA 4096 key pair

```
openssl genrsa -out myprivatekey.pem 4096
openssl rsa -in myprivatekey.pem -pubout > mypublickey.pub
```

## How to use

### Configuring with your application credentials

Add in appsettings.json

```
{
  "StoneBankingJwt": {
    "PrivateKey": "myprivatekey.pem",
    "PublicKey": "mypublickey.pub",  
    "ClientId": "f7158598-fcbe-4810-98f5-08d512f42977",
    "Environment": "Sandbox",
    "AuthenticationExpiresInSeconds": 900,
    "ConsentExpiresInSeconds": 300,
    "ConsentDefaultRedirectUrl": "https://mysite.com/stonebanking/success"
  }
}
```

Notes:
- *PrivateKey*: you can change this var with key content. e.g. "-----BEGIN RSA PRIVATE KEY-----\nMIIJKAIBAAKCAgEA..."
- *PublicKey*: you can change this var with key content. e.g. "-----BEGIN PUBLIC KEY-----\nMIICIjANBgkqhkiG9w0BA..."
- *Environment*: Possible values `Sandbox` `Production`

Add in Startup.cs

```
services.AddSingleton(configuration); // register IConfigurationRoot before
services.AddStoneBankingJwt();
```

or 

```
services.AddStoneBankingJwt(configuration);
```

or ignoring appsettings.json

```
var settings = new StoneBankingSettings
{
    ClientId = "xxxx",
    PrivateKey = "key.pem",
    PublicKey = "key.pub",
    ConsentDefaultRedirectUrl = "https://mysite.com/redirect"
};
services.AddStoneBankingJwt(settings);
```

without IoC:

```
var settings = new StoneBankingSettings
{
    ClientId = "xxxx",
    PrivateKey = "key.pem",
    PublicKey = "key.pub",
    ConsentDefaultRedirectUrl = "https://mysite.com/redirect"
};
IStoneBankingJwt stoneBanking = new StoneBankingJwt(settings);
```

### Creating Authentication Token

```
string authenticationToken = stoneBanking.CreateAuthenticationToken();
```

### Creating Access Token with Authentication Token

```
string authenticationToken = stoneBanking.CreateAuthenticationToken();
string accessToken = stoneBanking.CreateAccessToken(authenticationToken);
```

### Creating Access Token with Auto Managed Authentication Token Creation [BEST USAGE]

```
// this will create a token and reuse it during token expiration time
// 60 seconds before expiration, next call will generate a new access token

string accessToken = stoneBanking.CreateAccessToken();
```

### Creating Consent Url

```
string consentUrl = stoneBanking.CreateConsentUrl();
```

### Creating Consent Url With Metadata

```
var metadata = new Dictionary<string, string>
{
    { "user_id", "user123" },
    { "other_data", "123" }
};

string consentUrl = stoneBanking.CreateConsentUrl(metadata);
```

### Decoding Jwt Token (created with your RSA key pair)

```
Dictionary<string, object> decodedToken = stoneBankingJwt.DecodeToken(token);
```

## How can I contribute?
Please, refer to [CONTRIBUTING](.github/CONTRIBUTING.md)

## Found something strange or need a new feature?
Open a new Issue following our issue template [ISSUE TEMPLATE](.github/ISSUE_TEMPLATE.md)

## Changelog
See in [nuget version history](https://www.nuget.org/packages/StoneBanking.Jwt)

## Did you like it? Please, make a donate :)

if you liked this project, please make a contribution and help to keep this and other initiatives, send me some Satochis.

BTC Wallet: `1G535x1rYdMo9CNdTGK3eG6XJddBHdaqfX`

![1G535x1rYdMo9CNdTGK3eG6XJddBHdaqfX](https://i.imgur.com/mN7ueoE.png)

[![Codacy Badge](https://api.codacy.com/project/badge/Grade/5bb3ad40445a4d5e9aad8469de06e808)](https://app.codacy.com/app/ThiagoBarradas/StoneBanking.Jwt?utm_source=github.com&utm_medium=referral&utm_content=ThiagoBarradas/StoneBanking.Jwt&utm_campaign=badger)
[![Build status](https://ci.appveyor.com/api/projects/status/e50c386k68c9dy7j/branch/master?svg=true)](https://ci.appveyor.com/project/ThiagoBarradas/StoneBanking.Jwt/branch/master)
[![codecov](https://codecov.io/gh/ThiagoBarradas/StoneBanking.Jwt/branch/master/graph/badge.svg)](https://codecov.io/gh/ThiagoBarradas/StoneBanking.Jwt)
[![NuGet Downloads](https://img.shields.io/nuget/dt/StoneBanking.Jwt.svg)](https://www.nuget.org/packages/StoneBanking.Jwt/)
[![NuGet Version](https://img.shields.io/nuget/v/StoneBanking.Jwt.svg)](https://www.nuget.org/packages/StoneBanking.Jwt/)

# StoneBanking.Jwt

Creates Jwt Authentication Token and Consent URL by RSA key pair.

## Install via NuGet

```
PM> Install-Package StoneBanking.Jwt
```

## Stone Banking Application

See more here: https://docs.openbank.stone.com.br/docs/cadastro-da-aplicacao-guides

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
    PublicKey = "key.pub"
};
services.AddStoneBankingJwt(settings);
```

without IoC:

```
var settings = new StoneBankingSettings
{
    ClientId = "xxxx",
    PrivateKey = "key.pem",
    PublicKey = "key.pub"
};
IStoneBankingJwt stoneBankingJwt = new StoneBankingJwt(settings);
```

### Creating Authentication Tokens

```
string accessToken = stoneBankingJwt.CreateAuthenticationToken();
```

### Creating Consent Url

```
string consentUrl = stoneBankingJwt.CreateConsentUrl();
```

### Creating Consent Url With Metadatas

```
var metadata = new Dictionary<string, string>
{
    { "user_id", "user123" },
    { "other_data", "123" }
};

string consentUrl = stoneBankingJwt.CreateConsentUrl(metadata);
```

### Creating Consent Url Replacing Default Redirect Url

```
string newUrl = "https://mysite.com/user123/stone-banking-success";
string consentUrl = stoneBankingJwt.CreateConsentUrl(newUrl);
```

### Decoding Jwt Token (created with your RSA key pair)

```
Dictionary<string, object> decodedToken = stoneBankingJwt.DecodeToken(token);
```

### Configuring with your application credentials

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

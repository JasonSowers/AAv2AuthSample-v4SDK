# AAv2AuthSample-v4SDK

1. Clone this repository.
2. Add your MicrosoftAppId and MicrosoftAppPassword to the appsettings.json file
3. Add your connection name to the AADv2Bot.cs file on line 17
4. In your package manager console install the following packages:  
**Note: These packages will change in the future. for not I know this projects works with these exact packages**  
[`Microsoft.Bot.Builder 4.0.0.38511`](https://botbuilder.myget.org/feed/botbuilder-v4-dotnet-daily/package/nuget/Microsoft.Bot.Builder)
```
Install-Package Microsoft.Bot.Builder -Version 4.0.0.38511 -Source https://botbuilder.myget.org/F/botbuilder-v4-dotnet-daily/api/v3/index.json
```
[`Microsoft.Bot.Builder.Dialogs 4.0.0.38511`](https://botbuilder.myget.org/feed/botbuilder-v4-dotnet-daily/package/nuget/Microsoft.Bot.Builder.Dialogs)
```
Install-Package Microsoft.Bot.Builder.Dialogs -Version 4.0.0.38511 -Source https://botbuilder.myget.org/F/botbuilder-v4-dotnet-daily/api/v3/index.json
```
[`Microsoft.Bot.Builder.Integration.AspNet.Core 4.0.0.38511`](https://botbuilder.myget.org/feed/botbuilder-v4-dotnet-daily/package/nuget/Microsoft.Bot.Builder.Integration.AspNet.Core)
```
Install-Package Microsoft.Bot.Builder.Integration.AspNet.Core -Version 4.0.0.38511 -Source https://botbuilder.myget.org/F/botbuilder-v4-dotnet-daily/api/v3/index.json
```

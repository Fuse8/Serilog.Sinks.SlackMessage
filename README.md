# Serilog.Sinks.SlackMessage

Writes Serilog events to the Slack

## Geting started

[Create](https://api.slack.com/bot-users#getting-started) a Slack Bot and give it at least two scopes: [chat:write](https://api.slack.com/scopes/chat:write) and [files:write](https://api.slack.com/scopes/files:write)

Register the sink in your C# code

```csharp
Log.Logger = new LoggerConfiguration()
        .WriteTo.Slack(
          "Enter your token here",
          new SlackLogOptions
          {
            Channel = "Enter your slack channel here",
            Mentions = new Dictionary<LogEventLevel, SlackLogMention>()
            {
              {
                LogEventLevel.Error,
                new SlackLogMention
                {
                  Users = new[] { "slack_username" },
                  Channel = SlackMention.Here,
                }
              },
            },
            MaxMessageLineCount = 5,
            SendExceptionAsFile = false,
          },
          LogEventLevel.Debug
        )
        .CreateLogger();
```

Also you can configure the sink in appsettings.json.

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "Slack",
        "Args": {
          "Token": "Enter your token here",
          "MinimumLevel": "Debug",
          "Options": {
            "Channel": "Enter your channel name here",
            "SendExceptionAsFile": false,
            "MaxMessageLineCount": 10,
            "Mentions": {
              "Debug": {
                "Users": ["some_user_name"],
                "Channel": "Here"
              }
            }
          }
        }
      }
    ]
  }
}
```

To do this, install the [Serilog.Settings.Configuration](https://www.nuget.org/packages/Serilog.Settings.Configuration) package and register the sink in your C# code

```csharp
Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
```

Remember to register SelfLog before the sink registration. Otherwise you will not see errors that occurred in the sink

For example:

```csharp
SelfLog.Enable(Console.WriteLine);
```

# Serilog.Sinks.SlackMessage

Writes Serilog events to the Slack using bot api

[![NuGet Version](https://img.shields.io/nuget/v/Serilog.Sinks.SlackMessage.svg?style=flat)](https://www.nuget.org/packages/Serilog.Sinks.SlackMessage/)
## Geting started

[Create](https://api.slack.com/bot-users#getting-started) a Slack Bot and give it at least two scopes: [chat:write](https://api.slack.com/scopes/chat:write) and [files:write](https://api.slack.com/scopes/files:write). Than add this bot to the channel where log events will be written.

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

## Samples

![Simple Message](/assets/sample1.bmp)

Set the `MaxMessageLineCount` option to `null` if you wanna receive all event information in one message. 
```csharp
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Slack(
          "<TOKEN>",
          new SlackLogOptions { Channel = "<CHANNEL>", MaxMessageLineCount = null }
        )
        .CreateLogger();
    
      Log.Information(longMessage);
```

![Long Message](/assets/long_message.bmp)


But it's better to set a value between 5 to 10. Long message will be cutted and full message will be sent to the thread.

```csharp
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Slack(
          "<TOKEN>",
          new SlackLogOptions { Channel = "<CHANNEL>", MaxMessageLineCount = 5 }
        )
        .CreateLogger();
```

![Short Message](/assets/short_message.bmp)


You can notify usergroup, channel or specific users. Just set the `Mentions` option
```csharp

      Log.Logger = new LoggerConfiguration()
        .WriteTo.Slack(
          "<TOKEN>",
          new SlackLogOptions
          {
            Channel = "<CHANNEL>", 
            Mentions = new Dictionary<LogEventLevel, SlackLogMention>()
            {
              { LogEventLevel.Error, new SlackLogMention { Channel = SlackMention.Here } },
              { LogEventLevel.Fatal, new SlackLogMention { Channel = SlackMention.Channel } },
              { LogEventLevel.Debug, new SlackLogMention { Users = new[] { "<USERNAME>" }, UserGroups = new[] { "<USER_GROUP1>" }} },
              { LogEventLevel.Warning, new SlackLogMention { UserGroups = new[] { "<USER_GROUP2>" }} },
            }
          }
        )
        .CreateLogger();
```

If an event contains an exception, it always puts to a thread. Set the `SendExceptionAsFile` optiont to `false` to receive exception in message body.
 
![Exception in Message](/assets/exception_in_code_block.bmp)

If you set the `SendExceptionAsFile` optiont to `true`, an exception will be sent as text file (looks prettier in our opinion)

```csharp
      Log.Logger = new LoggerConfiguration()
        .WriteTo.Slack(
          "<TOKEN>",
          new SlackLogOptions { Channel = "<CHANNEL>", SendExceptionAsFile = true }
        )
        .CreateLogger();
```

![Exception in File](/assets/exception_in_file.bmp)
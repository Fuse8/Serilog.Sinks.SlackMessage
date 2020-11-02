using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using Serilog.Sinks.SlackMessage.Extensions;
using Serilog.Sinks.SlackMessage.Helpers;
using Serilog.Sinks.SlackMessage.Options;
using SlackBot.Api;
using SlackBot.Api.Helpers;

namespace Serilog.Sinks.SlackMessage
{
	/// <summary>
	/// Implements <see cref="PeriodicBatchingSink"/> and provides means needed for sending Serilog log events to Slack.
	/// </summary>
	public class SlackSink : PeriodicBatchingSink
	{
		private readonly string _slackBotToken;
		private readonly SlackLogOptions _logOptions;

		public SlackSink(SlackLogOptions logOptions, string slackBotToken)
			: base(1000, TimeSpan.FromSeconds(2))
		{
			_logOptions = logOptions;
			_slackBotToken = slackBotToken;
		}

		/// <summary>
		/// Overrides <see cref="PeriodicBatchingSink.EmitBatchAsync"/> method and uses <see cref="HttpClient"/> to post <see cref="LogEvent"/> to Slack.
		/// </summary>
		/// <param name="events">Collection of <see cref="LogEvent"/>.</param>
		/// <returns>Awaitable task.</returns>
		protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
		{
			using (var slackClient = SlackClientFactory.CreateSlackClient(_slackBotToken))
			{
				foreach (var logEvent in events)
				{
					try
					{
						var logMessage = logEvent.RenderMessage();
						var logProperties = logEvent.Properties.ToDictionary(p => p.Key, p => p.Value.ToString());

						if (_logOptions.MaxMessageLineCount.HasValue)
						{
							await PostShortMessageAndReplyFullLogEventAsync(
								slackClient,
								logMessage, 
								_logOptions.MaxMessageLineCount.Value, 
								logEvent,
								logProperties);
						}
						else
						{
							await PostLogEventMessageAsync(slackClient, logEvent, logMessage, logProperties);
						}
					}
					catch (Exception e)
					{
						SelfLog.WriteLine("Can't post a message '{0}' to the Slack:  {1}", logEvent.MessageTemplate, e.ToString());
					}
				}
			}
		}

		private async Task PostLogEventMessageAsync(SlackClient slackClient, LogEvent logEvent, string logMessage, Dictionary<string, string> logProperties)
		{
			var mainMessageResponse = await PostMessageToSlackAsync(slackClient, logEvent, logMessage, logProperties);
			if (logEvent.Exception != null)
			{
				await PostMessageToThreadAsync(slackClient, logEvent, mainMessageResponse.Timestamp);
			}
		}

		private async Task PostShortMessageAndReplyFullLogEventAsync(
			SlackClient slackClient,
			string logMessage,
			int maxMessageLineCount,
			LogEvent logEvent,
			Dictionary<string, string> logProperties)
		{
			var messageLines = MessageHelper.GetLineCount(logMessage);

			var messageLineCount = messageLines.Sum(p => p.LineCount);
			if (messageLineCount > maxMessageLineCount)
			{
				var shortMessage = MessageHelper.GetShortMessage(messageLines, maxMessageLineCount);

				var mainMessageResponse = await PostMessageToSlackAsync(slackClient, logEvent, shortMessage);
				await PostMessageToThreadAsync(
					slackClient,
					logEvent,
					mainMessageResponse.Timestamp,
					logProperties,
					logMessage);
			}
			else
			{
				var propertyLineCount = logProperties.SelectMany(p => MessageHelper.GetLineCount(p.Key + p.Value)).Sum(p => p.LineCount);

				if (messageLineCount + propertyLineCount > maxMessageLineCount)
				{
					var mainMessageResponse = await PostMessageToSlackAsync(slackClient, logEvent, logMessage);
					await PostMessageToThreadAsync(
						slackClient,
						logEvent,
						mainMessageResponse.Timestamp,
						logProperties);
				}
				else
				{
					await PostLogEventMessageAsync(slackClient, logEvent, logMessage, logProperties);
				}
			}
		}

		private Task<SendMessageResponse> PostMessageToSlackAsync(
			SlackClient slackClient,
			LogEvent logEvent,
			string message,
			Dictionary<string, string> properties = null)
		{
			var messageBuilder = MessageBuilderExtensions
				.CreateMessageBuilder(_logOptions.Channel, logEvent, message);

			if (_logOptions.Mentions.TryGetValue(logEvent.Level, out var mention))
			{
				messageBuilder.MentionBlock(mention);
			}

			var messageRequest = messageBuilder
				.AttachmentProperties(logEvent, properties, message)
				.CreateMessage();

			return slackClient.Chat.PostMessageAsync(messageRequest);
		}

		private async Task PostMessageToThreadAsync(
			SlackClient slackClient,
			LogEvent logEvent,
			string replayTimespan,
			Dictionary<string, string> properties = null,
			string message = null)
		{
			var threadMessageBuilder = MessageBuilderExtensions
				.CreateMessageBuilder(_logOptions.Channel, logEvent, message, replayTimespan)
				.MessageBlock(message)
				.AttachmentProperties(logEvent, properties);

			if (logEvent.Exception != null)
			{
				await AttachExceptionAsync(slackClient, threadMessageBuilder, logEvent.Exception);
			}

			var slackMessage = threadMessageBuilder.CreateMessage();
			await slackClient.Chat.PostMessageAsync(slackMessage);
		}

		private async Task AttachExceptionAsync(SlackClient slackClient, SlackMessageBuilder threadMessageBuilder, Exception exception)
		{
			if (_logOptions.SendExceptionAsFile)
			{
				var exceptionType = exception.GetType().Name;
				var fileObjectResponse = await slackClient.Files.UploadContentAsync(
					exception.ToString(),
					title: exceptionType,
					filename: $"{exceptionType}.cs");

				threadMessageBuilder.Text(fileObjectResponse.File.Permalink.ToString());
			}
			else
			{
				threadMessageBuilder
					.MessageBlock(SlackMessageTextHelper.CodeBlock(exception.ToString()));
			}
		}
	}
}
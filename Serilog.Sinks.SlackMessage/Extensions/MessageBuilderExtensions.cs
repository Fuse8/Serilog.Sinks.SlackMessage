using System.Collections.Generic;
using System.Linq;
using Serilog.Events;
using Serilog.Sinks.SlackMessage.Options;
using SlackBot.Api;
using SlackBot.Api.Helpers;

namespace Serilog.Sinks.SlackMessage.Extensions
{
	internal static class MessageBuilderExtensions
	{
		public static SlackMessageBuilder CreateMessageBuilder(string channel, LogEvent logEvent, string message, string replayTimespan = null)
			=> SlackMessageBuilder.CreateBuilder(channel)
				.Text(message)
				.LineBreak()
				.Reply(replayTimespan)
				.Blocks(
					new SectionBlock
					{
						Text = (MarkdownTextObject) SlackMessageTextHelper.Date(logEvent.Timestamp.ToUnixTimeSeconds(), "{date_num} {time_secs}", "LogDate"),
					});
		
		public static SlackMessageBuilder MessageBlock(this SlackMessageBuilder builder, params string[] messages)
		{
			var notEmptyMessages = messages.Where(message => !string.IsNullOrWhiteSpace(message)).ToArray();
			if (notEmptyMessages.Any())
			{
				builder.Blocks(CreateMessageBlock(notEmptyMessages));
			}

			return builder;
		}
		
		public static SlackMessageBuilder AttachmentProperties(this SlackMessageBuilder builder, LogEvent logEvent, Dictionary<string, string> properties, string message = null)
		{
			var attachmentBlocks = new List<BlockBase>();
			if (message != null)
			{
				var messageBlock = CreateMessageBlock(SlackMessageTextHelper.Italic(logEvent.Level.ToString("G")), message);
				attachmentBlocks.Add(messageBlock);
			}

			if (properties != null && properties.Any())
			{
				var propertyBlocks = properties.Select(property => CreateMessageBlock(SlackMessageTextHelper.Bold(property.Key), property.Value));
				attachmentBlocks.AddRange(propertyBlocks);
			}

			if (attachmentBlocks.Any())
			{
				var attachment = new Attachment
                			{
                				Color = GetAttachmentColor(logEvent.Level),
                				Blocks = attachmentBlocks,
                			};

				builder.Attachments(attachment);
			}
			
			return builder;
		}
		
		public static SlackMessageBuilder MentionBlock(this SlackMessageBuilder builder, SlackLogMention mention)
		{
			var mentions = new List<string>();
			if (mention.Channel.HasValue)
			{
				mentions.Add(SlackMessageTextHelper.ChannelMention(mention.Channel.Value));
			}

			if (mention.Users?.Any() == true)
			{
				mentions.AddRange(mention.Users.Select(SlackMessageTextHelper.UserMention));
			}

			if (mention.UserGroups?.Any() == true)
			{
				mentions.AddRange(mention.UserGroups.Select(SlackMessageTextHelper.UserGroupMention));
			}

			if (mentions.Any())
			{
				builder.MessageBlock(string.Join(", ", mentions));
			}
			
			return builder;
		}
		
		
		private static string GetAttachmentColor(LogEventLevel eventLevel)
		{
			string color;
			switch (eventLevel)
			{
				case LogEventLevel.Error:
				case LogEventLevel.Fatal:
					color = "#B22222";
					break;
				case LogEventLevel.Verbose:
				case LogEventLevel.Information:
				case LogEventLevel.Debug:
					color = "#808080";
					break;
				case LogEventLevel.Warning:
					color = "#808000";
					break;
				default:
					color = "#808080";
					break;
			}

			return color;
		}
		
		private static BlockBase CreateMessageBlock(params string[] messages)
			=> new ContextBlock
			{
				Elements = messages.Select(
						message =>
							new MarkdownTextObject
							{
								Text = message,
							} as IContextElement)
					.ToList(),
			};
	}
}
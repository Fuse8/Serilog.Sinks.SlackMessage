using System.Collections.Generic;
using Serilog.Events;

namespace Serilog.Sinks.SlackMessage.Options
{
	public class SlackLogOptions
	{
		/// <summary>
		/// Channel name or ID to post messages
		/// </summary>
		public string Channel { get; set; }

		/// <summary>
		/// Set TRUE to post exception as file content
		/// otherwise exception will be posted as code section 
		/// </summary>
		public bool SendExceptionAsFile { get; set; }

		/// <summary>
		/// Set maximum number if lines in slack message.
		/// If a message is larger than this value, full message will be posted in thread.
		/// Leave it NULL to post all lines in one message
		/// </summary>
		public int? MaxMessageLineCount { get; set; }

		/// <summary>
		/// Set users, usergroup and channel mentions to notify them about events 
		/// </summary>
		public Dictionary<LogEventLevel, SlackLogMention> Mentions { get; set; }
	}
}
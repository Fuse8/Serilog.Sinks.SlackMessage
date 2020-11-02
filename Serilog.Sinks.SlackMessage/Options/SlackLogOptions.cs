using System.Collections.Generic;
using Serilog.Events;

namespace Serilog.Sinks.SlackMessage.Options
{
	public class SlackLogOptions
	{
		public string Channel { get; set; }

		public bool SendExceptionAsFile { get; set; }

		public int? MaxMessageLineCount { get; set; }

		public Dictionary<LogEventLevel, SlackLogMention> Mentions { get; set; }
	}
}
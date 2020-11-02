using SlackBot.Api.Enums;

namespace Serilog.Sinks.SlackMessage.Options
{
	public class SlackLogMention
	{
		public string[] Users { get; set; }

		public string[] UserGroups { get; set; }

		public SlackMention? Channel { get; set; }
	}
}
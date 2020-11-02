using SlackBot.Api.Enums;

namespace Serilog.Sinks.SlackMessage.Options
{
	/// <summary>
	/// Users, groups and channel will be mentions in message
	/// </summary>
	public class SlackLogMention
	{
		/// <summary>
		/// List of Slack user names 
		/// </summary>
		public string[] Users { get; set; }

		/// <summary>
		/// List of Slack group names
		/// </summary>
		public string[] UserGroups { get; set; }

		/// <summary>
		/// Mention channel with @here or @channel 
		/// </summary>
		public SlackMention? Channel { get; set; }
	}
}
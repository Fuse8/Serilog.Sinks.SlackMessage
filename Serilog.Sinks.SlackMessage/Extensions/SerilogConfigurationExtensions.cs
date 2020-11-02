using System;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SlackMessage.Options;

namespace Serilog.Sinks.SlackMessage.Extensions
{
	/// <summary>
	/// Provides extension methods on <see cref="LoggerSinkConfiguration"/>.
	/// </summary>
	public static class SerilogConfigurationExtensions
	{
		public static LoggerConfiguration Slack(
			this LoggerSinkConfiguration loggerSinkConfiguration,
			string token,
			SlackLogOptions options,
			LogEventLevel minimumLevel = LevelAlias.Minimum)
		{
			if (loggerSinkConfiguration == null)
			{
				throw new ArgumentNullException(nameof(loggerSinkConfiguration));
			}

			if (token == null)
			{
				throw new ArgumentNullException(nameof(token));
			}
			
			if (options?.Channel == null)
			{
				throw new ArgumentNullException(nameof(options.Channel));
			}

			ILogEventSink sink = new SlackSink(options, token);

			return loggerSinkConfiguration.Sink(sink, minimumLevel);
		}
	}
}
using System;
using System.Linq;
using SlackBot.Api.Helpers;

namespace Serilog.Sinks.SlackMessage.Helpers
{
	internal static class MessageHelper
	{
		private const int SlackMessageLineLength = 100;

		public static (int LineCount, string Line)[] GetLineCount(string message)
			=> message
				.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
				.Select(line => (LineCount: line.Length / SlackMessageLineLength + 1, Line: line))
				.ToArray();

		public static string GetShortMessage((int LineCount, string Line)[] messageLines, int? maxLineCount)
		{
			var shortMessageLineCount = 0;
			var shortMessageLines = messageLines.TakeWhile(
					line =>
					{
						shortMessageLineCount += line.LineCount;
						return shortMessageLineCount < maxLineCount;
					})
				.Select(p => p.Line)
				.Union(new[] { SlackMessageTextHelper.Italic("<see more in thread>") });

			var shortMessage = string.Join(Environment.NewLine, shortMessageLines);
			return shortMessage;
		}
	}
}
using System.Reflection;

namespace Rockaway.WebApp.Services {
	public class StatusReporter : IStatusReporter {

		private static readonly Assembly assembly = Assembly.GetEntryAssembly()!;
		private static readonly DateTimeOffset dateUpdated = new(File.GetLastWriteTimeUtc(assembly.Location), TimeSpan.Zero);

		public ServerStatus GetStatus() => new() {

			Assembly = assembly.FullName ?? "Assembly.GetEntryAssembly() returned null",
			Modified = dateUpdated.ToString("O"),
			Hostname = Environment.MachineName,
			DateTime = DateTimeOffset.UtcNow.ToString("O"),
			RunningTime = GetRunningTimeString()
		};

		private TimeSpan GetRunningTime() => DateTimeOffset.UtcNow - dateUpdated;

		private string GetRunningTimeString() {
			var timeRunning = GetRunningTime();
			var hours = timeRunning.Hours;
			var minutes = timeRunning.Minutes;
			var seconds = timeRunning.Seconds;
			return String.Format($"Up and running for - {hours} hours, {minutes} minutes, {seconds} seconds");
		}

		public long GetUptimeInSeconds() => Convert.ToInt64(Math.Round(GetRunningTime().TotalSeconds));
	}
}

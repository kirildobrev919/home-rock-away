// Rockaway.WebApp.Tests/Pages/PageTests.cs
using System.Text.Json;
using AngleSharp;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Rockaway.WebApp.Services;
using Shouldly;

namespace Rockaway.WebApp.Tests.Pages {
	public class PageTests {

		private static readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web);

		private static readonly ServerStatus testStatus = new() {
			Assembly = "TEST_ASSEMBLY",
			Modified = new DateTimeOffset(2021, 2, 3, 4, 5, 6, TimeSpan.Zero).ToString("O"),
			Hostname = "TEST_HOSTNAME",
			DateTime = new DateTimeOffset(2022, 3, 4, 5, 6, 7, TimeSpan.Zero).ToString("O"),
			RunningTime = "Up and running for - 1 hours, 10 minutes, 11 seconds"
		};

		private static long expectedSeconds = 4211;

		private class TestStatusReporter : IStatusReporter {
			public ServerStatus GetStatus() => testStatus;

			public long GetUptimeInSeconds() => expectedSeconds;
		}

		[Fact]
		public async Task Index_Page_Returns_Success() {
			await using var factory = new WebApplicationFactory<Program>();
			using var client = factory.CreateClient();
			using var response = await client.GetAsync("/");
			response.EnsureSuccessStatusCode();
		}

		[Theory]
		[InlineData("/privacy")]
		[InlineData("/contact")]
		public async Task Index_Page_Returns_Success_WithTheory(string path) {
			await using var factory = new WebApplicationFactory<Program>();
			using var client = factory.CreateClient();
			using var response = await client.GetAsync(path);
			response.EnsureSuccessStatusCode();
		}

		[Fact]
		public async Task HomePage_Title_Has_Correct_Content() {
			var browsingContext = BrowsingContext.New(Configuration.Default);
			await using var factory = new WebApplicationFactory<Program>();
			var client = factory.CreateClient();
			var html = await client.GetStringAsync("/");
			var dom = await browsingContext.OpenAsync(req => req.Content(html));
			var title = dom.QuerySelector("title");
			title.ShouldNotBeNull();
			title.InnerHtml.ShouldBe("Rockaway");
		}

		[Theory]
		[InlineData("/", "Rockaway")]
		[InlineData("/contact", "Contact Us")]
		[InlineData("/privacy", "Privacy Policy")]
		public async Task Page_Has_Correct_Title(string url, string title) {
			var browsingContext = BrowsingContext.New(Configuration.Default);
			await using var factory = new WebApplicationFactory<Program>();
			var client = factory.CreateClient();
			var html = await client.GetStringAsync(url);
			var dom = await browsingContext.OpenAsync(req => req.Content(html));
			var titleElement = dom.QuerySelector("title");
			titleElement.ShouldNotBeNull();
			titleElement.InnerHtml.ShouldBe(title);
		}

		[Theory]
		[InlineData("/contact")]
		public async Task Page_Has_Correct_EmailAndPhoneNumber(string url) {
			var browsingContext = BrowsingContext.New(Configuration.Default);
			await using var factory = new WebApplicationFactory<Program>();
			var client = factory.CreateClient();
			var html = await client.GetStringAsync(url);
			var dom = await browsingContext.OpenAsync(req => req.Content(html));
			var listItems = dom.QuerySelectorAll("li");
			listItems.ShouldNotBeNull();
			listItems.ShouldContain(i => i.InnerHtml.Contains("hello@rockaway.dev"));
			listItems.ShouldContain(i => i.InnerHtml.Contains("555 1234"));
		}

		[Fact]
		public async Task Status_Endpoint_Return_Status() {
			var factory  = new WebApplicationFactory<Program>();
			var client = factory.CreateClient();
			var result = await client.GetAsync("/status");
			result.EnsureSuccessStatusCode();
		}

		[Fact]
		public async Task Status_Endpoint_Return_Status_UsingHostBuilder() {
			var factory = new WebApplicationFactory<Program>()
				.WithWebHostBuilder(builder => builder.ConfigureServices(services => {
					services.AddSingleton<IStatusReporter>(new TestStatusReporter());
				}));
			var client = factory.CreateClient();
			var json = await client.GetStringAsync("/status");
			var status = JsonSerializer.Deserialize<ServerStatus>(json, jsonSerializerOptions);
			status.ShouldNotBeNull();
			status.ShouldBeEquivalentTo(testStatus);
		}

		[Fact]
		public async Task Uptime_Endpoint_Return_Status() {
			var factory = new WebApplicationFactory<Program>();
			var client = factory.CreateClient();
			var result = await client.GetAsync("/uptime");
			result.EnsureSuccessStatusCode();
		}

		[Fact]
		public async Task Uptime_Endpoint_Return_runningTimeInSeconds_UsingHostBuilder() {
			var factory = new WebApplicationFactory<Program>()
				.WithWebHostBuilder(builder => builder.ConfigureServices(services => {
					services.AddSingleton<IStatusReporter>(new TestStatusReporter());
				}));
			var client = factory.CreateClient();
			var uptime = await client.GetStringAsync("/uptime");
			uptime.ShouldNotBeNull();
			uptime.ShouldBeEquivalentTo(expectedSeconds.ToString());
		}
	}
}

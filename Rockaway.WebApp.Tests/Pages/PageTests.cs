using AngleSharp;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;

namespace Rockaway.WebApp.Tests.Pages {
	public class PageTests {
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
	}
}

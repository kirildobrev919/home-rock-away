using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Rockaway.WebApp.Data;
using Shouldly;

namespace Rockaway.WebApp.Tests.Pages {
	public class VenuesTests {
		[Fact]
		public async Task Venues_Page_Contains_All_Venues() {
			await using var factory = new WebApplicationFactory<Program>();
			var client = factory.CreateClient();
			var html = await client.GetStringAsync("/venues");
			var decodedHtml = WebUtility.HtmlDecode(html);
			using var scope = factory.Services.CreateScope();
			var db = scope.ServiceProvider.GetService<RockawayDbContext>()!;
			var expected = db.Venues.ToList();
			foreach (var venue in expected) decodedHtml.ShouldContain(venue.Name);
		}
	}
}

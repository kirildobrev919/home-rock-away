// Rockaway.WebApp.Tests/FakeAuthenticationFilter.cs

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Rockaway.WebApp.Tests;

internal class FakeAuthenticationFilter(string emailAddress) : IStartupFilter {
	public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
		builder => {
			var options = FakeAuthenticationOptions.Create(emailAddress);
			builder.UseMiddleware<FakeAuthenticationMiddleware>(options);
			next(builder);
		};

	private class FakeAuthenticationOptions(string emailAddress) {

		public string EmailAddress { get; } = emailAddress;

		internal static IOptions<FakeAuthenticationOptions> Create(string emailAddress)
			=> Options.Create(new FakeAuthenticationOptions(emailAddress));
	}

	private class FakeAuthenticationMiddleware(RequestDelegate next, IOptions<FakeAuthenticationOptions> options) {
		private readonly string authenticationType = IdentityConstants.ApplicationScheme;
		private readonly string emailAddress = options.Value.EmailAddress;
		public async Task InvokeAsync(HttpContext context) {
			var claims = new Claim[] {
				new(ClaimTypes.Name, emailAddress),
				new(ClaimTypes.Email, emailAddress)
			};
			var identity = new ClaimsIdentity(claims, authenticationType);
			context.User = new(identity);
			await next(context);
		}
	}
}


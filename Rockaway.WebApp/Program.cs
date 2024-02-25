using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Rockaway.WebApp.Data;
using Rockaway.WebApp.Hosting;
using Rockaway.WebApp.Services;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

var logger = CreateAdHocLogger<Program>();
logger.LogInformation("Rockaway running in {environment} environment", builder.Environment.EnvironmentName);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IStatusReporter>(new StatusReporter());

// A bug in .NET 8 means you can't call extension methods from Program.Main, otherwise
// the aspnet-codegenerator tools fail with "Could not get the reflection type for DbContext"
// ReSharper disable once InvokeAsExtensionMethod
if (HostEnvironmentExtensions.UseSqlite(builder.Environment)) {
	logger.LogInformation("Using Sqlite database");
	var sqliteConnection = new SqliteConnection("Data Source=:memory:");
	sqliteConnection.Open();
	builder.Services.AddDbContext<RockawayDbContext>(options => options.UseSqlite(sqliteConnection));
} else {
	logger.LogInformation("Using SQL Server database");
	var connectionString = builder.Configuration.GetConnectionString("LOCAL_DB_AZURE_REPLACEMENT");
	builder.Services.AddDbContext<RockawayDbContext>(options => options.UseSqlServer(connectionString));
}

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
builder.Services.AddDefaultIdentity<IdentityUser>().AddEntityFrameworkStores<RockawayDbContext>();

var app = builder.Build();

//app.Logger.LogDebug("This is a DEBUG message.");
//app.Logger.LogInformation("This is an INFORMATION message.");
//app.Logger.LogWarning("This is a WARNING message.");
//app.Logger.LogError("This is an ERROR message.");
//app.Logger.LogCritical("This is a CRITICAL message.");
//app.Logger.LogTrace("This is a TRACE message.");


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
} else {
	app.UseDeveloperExceptionPage();
}

using (var scope = app.Services.CreateScope()) {
	using var db = scope.ServiceProvider.GetService<RockawayDbContext>()!;
	if (app.Environment.UseSqlite()) {
		db.Database.EnsureCreated();
	} else if (Boolean.TryParse(app.Configuration["apply-migrations"], out var applyMigrations) && applyMigrations) {
		//TODO: This section is needed when ci cd auto migration db from github to azur for production. Chapter 3.4
		//Log.Information("apply-migrations=true was specified. Applying EF migrations and then exiting.");
		db.Database.Migrate();
		//Log.Information("EF database migrations applied successfully.");
		Environment.Exit(0);
	}
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapGet("/status", (IStatusReporter reporter) => reporter.GetStatus());
app.MapGet("/uptime", (IStatusReporter reporter) => reporter.GetUptimeInSeconds());
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();

ILogger<T> CreateAdHocLogger<T>() {
	var config = new ConfigurationBuilder()
		.AddJsonFile("appsettings.json", false, true)
		.AddEnvironmentVariables()
		.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true, true)
		.Build();
	return LoggerFactory.Create(lb => lb.AddConfiguration(config)).CreateLogger<T>();
}
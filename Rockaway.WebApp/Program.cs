var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

var app = builder.Build();

app.Logger.LogDebug("This is a DEBUG message.");
app.Logger.LogInformation("This is an INFORMATION message.");
app.Logger.LogWarning("This is a WARNING message.");
app.Logger.LogError("This is an ERROR message.");
app.Logger.LogCritical("This is a CRITICAL message.");
app.Logger.LogTrace("This is a TRACE message.");


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
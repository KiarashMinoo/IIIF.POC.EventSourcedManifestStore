using IIIF.POC.EventSourcedManifestStore.Infrastructure;
using IIIF.POC.EventSourcedManifestStore.Services;
using KurrentDB.Client;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration["KurrentDB:ConnectionString"]
    ?? throw new InvalidOperationException(
        "KurrentDB:ConnectionString is not configured.");

var settings =
    KurrentDBClientSettings.Create(connectionString);

builder.Services.AddSingleton(
    new KurrentDBClient(settings));

builder.Services.AddRazorPages();

builder.Services.AddSingleton<ManifestEventSerializer>();
builder.Services.AddScoped<KurrentManifestEventStore>();
builder.Services.AddScoped<ManifestApplicationService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

app.Run();

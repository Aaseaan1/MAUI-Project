using JournalApp.Components;
using JournalApp.Data;
using JournalApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
    
builder.Services.AddDbContext<JournalDbContext>(options =>
    options.UseSqlite("Data Source=journalapp.db"));

builder.Services.AddScoped<JournalService>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<ThemeService>();
builder.Services.AddScoped<PdfExportService>();

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope()) 
{
    var context = scope.ServiceProvider.GetRequiredService<JournalDbContext>();
    context.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();



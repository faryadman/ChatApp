using ChatApp;
using ChatApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IChatRoomService, InMemoryChatRoomService>();
builder.Services.AddSignalR();
builder.Services.AddAuthentication(option =>
    option.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(option =>
{
    option.LoginPath = "/Login";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseCors(builder =>
{
    builder.WithOrigins("http://localhost:5007")
        .AllowAnyHeader()
        .WithMethods("GET", "POST")
        .AllowCredentials();
});
app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();
app.MapRazorPages();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatHub>("/chathub");
    endpoints.MapHub<SupportHub>("/supporthub");
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});
app.Run();


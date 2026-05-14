var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient("api", c => c.BaseAddress = new Uri("http://localhost:5000/"));
builder.Services.AddSession();
var app = builder.Build();
app.UseStaticFiles(); app.UseRouting(); app.UseSession();
app.MapControllerRoute(name:"default", pattern:"{controller=Home}/{action=Index}/{id?}");
app.Run();

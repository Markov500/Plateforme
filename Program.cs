using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using plateforme.Data;
using SmartBreadcrumbs.Extensions;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddDbContext<plateformeContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("plateformeContext") ?? throw new InvalidOperationException("Connection string 'plateformeContext' not found.")));

var connect = builder.Configuration.GetConnectionString("plateformeContext");
IServiceCollection serviceCollection = builder.Services.AddDbContext<plateformeContext>(options => options.UseMySql(connect, ServerVersion.AutoDetect(connect)));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//Les services pour le breadcrumb
builder.Services.AddBreadcrumbs(Assembly.GetExecutingAssembly(), options =>
{
    options.TagName = "nav";
    options.TagClasses = "";
    options.OlClasses = "breadcrumb";
    options.LiClasses = "breadcrumb-item";
    options.ActiveLiClasses = "breadcrumb-item active";
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Membres}/{action=Login}/{id?}");

app.Run();

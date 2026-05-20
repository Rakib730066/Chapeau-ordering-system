using Chapeau_ordering_system.Repositories;
using Chapeau_ordering_system.Repositories.Interfaces;
using Chapeau_ordering_system.Services;
using Chapeau_ordering_system.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

// I register Bar/Kitchen repository and service for dependency injection
// Using real database repository
builder.Services.AddScoped<IBarKitchenRepository, BarKitchenRepository>();
builder.Services.AddScoped<IBarKitchenService, BarKitchenService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");
app.Run();
 
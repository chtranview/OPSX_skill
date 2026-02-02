using Microsoft.EntityFrameworkCore;
using MyWeb.Data;
using MyWeb.Models;
using MyWeb.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// 配置 AppSettings
builder.Services.AddSingleton<AppSettings>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new AppSettings
    {
        TpiUbike = config["tpiUbike"] ?? string.Empty
    };
});

// 配置 DataCollectionSettings
builder.Services.AddSingleton<DataCollectionSettings>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var settings = new DataCollectionSettings();
    config.GetSection("DataCollection").Bind(settings);
    return settings;
});

builder.Services.AddHttpClient<UbikeService>();
builder.Services.AddScoped<IUbikeService, UbikeService>();

builder.Services.AddDbContext<TpiUbikeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TpiUbikeDB"),
        sqlOptions => sqlOptions.CommandTimeout(300))); // 5 minutes timeout

builder.Services.AddSingleton<CollectionStateService>();
builder.Services.AddHostedService<DataCollectionService>();
builder.Services.AddHostedService<DataCleanupService>();

var app = builder.Build();

// 自動套用資料庫遷移
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TpiUbikeDbContext>();
    dbContext.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

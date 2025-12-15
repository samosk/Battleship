using Battleship;
using Battleship.Models;
using Microsoft.EntityFrameworkCore;
using Battleship.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<BattleshipContext>(options => options.UseNpgsql(
    builder.Configuration.GetConnectionString("Battleship"),
    o => o
        .MapEnum<GameState>("game_state")
        .MapEnum<ShipType>("ship_type")
        .MapEnum<ShipOrientation>("ship_orientation")
        .MapEnum<ShotOutcome>("shot_outcome")));
builder.Services.AddDbContext<BattleshipIdentityContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Battleship")));
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<BattleshipIdentityContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.MapRazorPages();

app.Run();

using Battleship;
using Battleship.Models;
using Battleship.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<Battleship.BattleshipDbContext>(options => options.UseNpgsql(
    builder.Configuration.GetConnectionString("Battleship"),
    o => o
        .MapEnum<GameState>("game_state")
        .MapEnum<ShipType>("ship_type")
        .MapEnum<ShipOrientation>("ship_orientation")
        .MapEnum<ShotOutcome>("shot_outcome")));
builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<Battleship.BattleshipDbContext>();
builder.Services.AddSignalR();

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
    pattern: "{controller=Game}/{action=List}/{id?}")
    .WithStaticAssets();
app.MapRazorPages();

app.MapHub<GameHub>("/SignalR/Game");

app.Run();

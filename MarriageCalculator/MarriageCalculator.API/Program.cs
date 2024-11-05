using MarriageCalculator.API.Data;
using MarriageCalculator.API.Services;
using MarriageCalculator.Core.Models;
 
using Microsoft.EntityFrameworkCore;
 
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(
    opt => opt.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();




using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbInitializer = services.GetRequiredService<MarriageGameServices>();
    await dbInitializer.SetupDB();
}

app.MapGet("api/marriagegame", async (AppDbContext context) =>
{
    var items = await context
                        .MarriageGame
                        //.Include(i => i.Players)
                        .ToListAsync();
    return Results.Ok(items);
});

app.MapPost("api/marriagegame", async (AppDbContext context, MarriageGame game) =>
{
    await context.MarriageGame.AddAsync(game);

    await context.SaveChangesAsync();
    return Results.Created($"api/marriagegame/{game.Id}", game);
});

app.MapPut("api/marriagegame/{id}", async (AppDbContext context, int id, MarriageGame game) =>
{
    var model = await context.MarriageGame.FirstOrDefaultAsync(a => a.Id == id);
    if (model is null)
        return Results.NotFound();

    model.DealerPlayer = game.DealerPlayer;
    //model.Players = game.Players;

    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("api/marriagegame/{id}", async (AppDbContext context, int id) =>
{
    var model = await context.MarriageGame.FirstOrDefaultAsync(a => a.Id == id);
    if (model is null)
        return Results.NotFound();
    //foreach(var player in model.Players)
    //{
    //    context.Players.Remove(player);
    //}

    context.MarriageGame.Remove(model);

    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
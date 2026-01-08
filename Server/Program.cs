using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Interfaces;
using Server.Services;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ServerDataContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresContext"));
});

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddHostedService<TcpServerService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

using Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Auth.Infrastructure.DateBase;
using Microsoft.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services
    .AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services
    .AddEntityFrameworkNpgsql()
    .AddDbContextPool<ApplicationDbContext>(config =>
    {
        config
            .UseNpgsql("Host=localhost;Database=authDB;Persist Security Info=True;Port=5432;Username=postgres;Password=admin;Enlist=false");
        //.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    });


builder.Services.AddTransient<ApplicationUserStore>();
//builder.Services.AddAutoMapper(typeof(Startup));

//builder.Services.AddUnitOfWork<ApplicationDbContext, ApplicationUser, ApplicationRole>();


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
using (var scope = app.Services.CreateScope())
{
    DatabaseInitializer.Seed(scope.ServiceProvider);
}


app.Run();

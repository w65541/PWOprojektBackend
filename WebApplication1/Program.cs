using Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using System.Diagnostics;
using WebApplication1.Services;

string con = "Server=localhost;database=UserManagement;User=root;Password=root;";//"Server=localhost\\SQLEXPRESS01;Database=master;Trusted_Connection=True;";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DatabaseContext>();
builder.Services.AddAutoMapper(cfg => { }, typeof(MapperProfile).Assembly);
builder.Services.AddSingleton<SSEService>();
builder.Services.AddControllers().AddJsonOptions(options =>
{
  options.JsonSerializerOptions.ReferenceHandler =
      System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
}); ;

builder.Services.AddSingleton<BackgroundNotificationService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<BackgroundNotificationService>());

builder.Services.AddAuthentication(x => { 
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x => { 
    x.RequireHttpsMetadata=false;
    x.SaveToken = true;
    x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey("Cp5wQhqCpttzoJG53ausxUwlTH38jd24ChC0tA8SGaEkJBHqWpHVwGhevEcXCVE"u8.ToArray()),
        ValidateIssuer = false,
        ValidateAudience = false
    };
}

    );

NLog.LogManager.Setup().LoadConfiguration(builder => {
    builder.ForLogger().FilterMinLevel(NLog.LogLevel.Info).WriteToConsole();
    builder.ForLogger().FilterMinLevel(NLog.LogLevel.Debug).WriteToFile(fileName: "file.txt");
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
     {
         Name = "Authorization",
         Type = SecuritySchemeType.Http,
         Scheme = "bearer",
         BearerFormat = "JWT",
         In = ParameterLocation.Header,
         Description = "Enter your JWT token in the format: Bearer {your token}"
     });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") // Angular dev server
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
            
        });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c=> c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowAngular");
app.MapControllers().RequireAuthorization();

app.Run();

public partial class Program { }

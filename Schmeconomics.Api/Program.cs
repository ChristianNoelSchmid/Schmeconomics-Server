using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Schmeconomics.Api.Auth;
using Schmeconomics.Api.JwtSecrets;
using Schmeconomics.Api.Time;
using Schmeconomics.Api.Tokens;
using Schmeconomics.Api.Users;
using Schmeconomics.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
    In = ParameterLocation.Header, 
    Description = "Please insert JWT with Bearer into field",
    Name = "Authorization",
    Type = SecuritySchemeType.ApiKey 
  });
  c.AddSecurityRequirement(new OpenApiSecurityRequirement {
   { 
     new OpenApiSecurityScheme 
     { 
       Reference = new OpenApiReference 
       { 
         Type = ReferenceType.SecurityScheme,
         Id = "Bearer" 
       } 
      },
      new string[] { } 
    } 
  });
}
);
builder.Services.AddControllers();
builder.Services.AddAuthorization();

// Add JWT authentication
builder.Services.AddDbContext<SchmeconomicsDbContext>(
    config => config.UseSqlite(builder.Configuration.GetConnectionString("Db"))
);
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthTokenProvider, AuthTokenProvider>();
builder.Services.AddScoped<IRefreshTokenProvider, DbRefreshTokenProvider>();
builder.Services.AddScoped<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddScoped<ISecretsProvider, DbSecretsProvider>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<ICurrentUserSetter, CurrentUser>();

builder.Services.AddOptionsWithValidateOnStart<AuthTokenProviderConfig>()
    .Bind(builder.Configuration.GetRequiredSection(nameof(AuthTokenProviderConfig)))
    .ValidateDataAnnotations();
builder.Services.AddOptionsWithValidateOnStart<DbRefreshTokenProviderConfig>()
    .Bind(builder.Configuration.GetRequiredSection(nameof(DbRefreshTokenProviderConfig)))
    .ValidateDataAnnotations();
builder.Services.AddOptionsWithValidateOnStart<DbSecretsProviderConfig>()
    .Bind(builder.Configuration.GetRequiredSection(nameof(DbSecretsProviderConfig)))
    .ValidateDataAnnotations();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<JwtMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.UseHttpsRedirection();
app.Run();

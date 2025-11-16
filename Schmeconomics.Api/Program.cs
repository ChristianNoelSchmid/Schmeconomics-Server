using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Schmeconomics.Api.Auth;
using Schmeconomics.Api.Secrets;
using Schmeconomics.Api.Time;
using Schmeconomics.Api.Users;
using Schmeconomics.Entities;
using Schmeconomics.Api;
using Schmeconomics.Api.Tokens.AuthTokens;
using Schmeconomics.Api.Tokens.RefreshTokens; // Added for WebException

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
);;
builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddCors();

// Add JWT authentication
builder.Services.AddDbContext<SchmeconomicsDbContext>(
    config => config.UseSqlite(builder.Configuration.GetConnectionString("Db"))
);
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthTokenProvider, JwtAuthTokenProvider>();
builder.Services.AddScoped<IRefreshTokenProvider, DbRefreshTokenProvider>();
builder.Services.AddScoped<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddScoped<ISecretsProvider, DbSecretsProvider>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<ICurrentUserSetter, CurrentUser>();

builder.Services.AddOptionsWithValidateOnStart<JwtAuthTokenProviderConfig>()
    .Bind(builder.Configuration.GetRequiredSection(nameof(JwtAuthTokenProviderConfig)))
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

if(app.Environment.IsProduction()) 
{
  app.UseHttpsRedirection();
}

app.UseCors();

// Global exception handler
app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>()?.Error;
        
        // Check if the exception implements IWebErrorInfo
        int statusCode = StatusCodes.Status500InternalServerError;
        string? serverMessage = null;
        string clientMessage = "An internal server error occurred.";

        if (exception is IWebErrorInfo webErrorInfo)
        {
            statusCode = webErrorInfo.StatusCode;
            serverMessage = webErrorInfo.ServerMessage;
            clientMessage = webErrorInfo.ClientMessage;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = clientMessage,
        };

        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    });
});

app.MapControllers();
app.UseMiddleware<JwtMiddleware>();
app.UseAuthorization();

app.Run();

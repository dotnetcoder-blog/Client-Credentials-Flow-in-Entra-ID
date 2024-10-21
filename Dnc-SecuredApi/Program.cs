using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add services to the container.
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//      .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(bearerOptions =>
    {
        bearerOptions.TokenValidationParameters.ValidateAudience = true;
        bearerOptions.Audience = builder.Configuration.GetValue<string>("EntraId:Audience");

        bearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://login.microsoftonline.com/{builder.Configuration["EntraId:TenantId"]}/v2.0",
        };
    }, identityOptions =>
    {
        identityOptions.Instance = builder.Configuration.GetValue<string>("EntraId:Instance");
        identityOptions.Domain = builder.Configuration.GetValue<string>("EntraId:Domain");
        identityOptions.TenantId = builder.Configuration.GetValue<string>("EntraId:TenantId");
        identityOptions.ClientId = builder.Configuration.GetValue<string>("EntraId:ClientId");
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AccessAsApp", policy =>
        policy.RequireRole("access_as_app")); 
});


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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

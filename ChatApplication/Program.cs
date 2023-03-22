using ChatApplication.Data;
using ChatApplication.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using ChatApplication.Hubs;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Logging.ClearProviders();
builder.Logging.AddLog4Net();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
    //ErrorEventHandler errorEventHandler;
});

builder.Services.AddDbContext<ChatAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionString")));

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    //options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value!)),
                ValidateLifetime = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    if (string.IsNullOrEmpty(accessToken) == false)
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });/*.AddGoogle(GoogleOptions =>
        {
            GoogleOptions.ClientId = "336034687630-sghvipd2u0vi3q07ttfqrqskhq8qhq39.apps.googleusercontent.com"; //builder.Configuration.GetSection("Authentication:Google:client_id").Value;
            GoogleOptions.ClientSecret = "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"; // builder.Configuration.GetSection("Authentication:Google:client_secret").Value;
        });*/

builder.Services.AddSignalR();
//builder.Services.AddSignalR(e => {
//    e.MaximumReceiveMessageSize = 102400000;
//});

builder.Services.AddCors(options => options.AddPolicy(name: "CorsPolicy",
    policy =>
    {
        policy.WithOrigins().AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
        policy.WithOrigins("http://127.0.0.1:5500").AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
    }
    ));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUploadPicService, UploadPicService>();
//builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationMiddlewareResultHandlerService>();

// Build the WebApplication instance
var app = builder.Build();

// If the environment is development, use Swagger and SwaggerUI

app.UseSwagger();
app.UseSwaggerUI();

// Use HTTPS redirection
app.UseHttpsRedirection();


try
{
    string path = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
    if (!Directory.Exists(path))
    {
        Directory.CreateDirectory(path);
    }
    app.UseStaticFiles(new StaticFileOptions
    {
        //File assests = new File()
        FileProvider = new PhysicalFileProvider(
               Path.Combine(builder.Environment.ContentRootPath, "Assets")),
        RequestPath = "/Assets"
    });
}
catch(Exception ex)
{
    Console.WriteLine(ex.Message);
}


app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseRouting();
// Use authentication and authorization
app.UseAuthorization();


/*app.UseEndpoints(endpoints =>
{
    app.MapControllers();
    endpoints.MapHub<ChatAppHub>("/hubs/chat");
});*/
app.MapHub<ChatAppHub>("/chatHubs");

// Map the controllers
app.MapControllers();
// Run the application
app.Run();

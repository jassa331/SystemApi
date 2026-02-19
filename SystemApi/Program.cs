using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using ServicePro.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SystemApi.BLL.Interface;
using SystemApi.DAL.Data;
using SystemApi.DAL.IRepository;
using SystemApi.DAL.Repository;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer
(builder.Configuration.GetConnectionString("Service")));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var settings = new ElasticsearchClientSettings(
    new Uri("https://localhost:9200"))
    .Authentication(new BasicAuthentication("elastic", "KXJDNrv9LQs_U4JMcdPK"))
    .ServerCertificateValidationCallback((o, certificate, chain, errors) => true);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("https://localhost:9200"))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "webapi-logs-{0:yyyy.MM.dd}",
        ModifyConnectionSettings = x => x.BasicAuthentication("elastic", "KXJDNrv9LQs_U4JMcdPK")
                                         .ServerCertificateValidationCallback((o, certificate, chain, errors) => true)
    })
    .CreateLogger();

builder.Host.UseSerilog();

var client = new ElasticsearchClient(settings);

builder.Services.AddSingleton(client);
builder.Services.AddScoped<ISimpleUserRepository, SimpleUserRepository>();

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwt = builder.Configuration.GetSection("Jwt");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // 🔥 IMPORTANT
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
        ),

        ClockSkew = TimeSpan.Zero
    };

    // 🔥 DEBUG – SEE EXACT TOKEN
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            Console.WriteLine("AUTH HEADER RECEIVED 👉 " + authHeader);
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("JWT FAILED 👉 " + context.Exception.Message);
            return Task.CompletedTask;
        }
    };
});

Console.WriteLine(builder.Configuration["Jwt:Issuer"]);
Console.WriteLine(builder.Configuration["Jwt:Audience"]);
Console.WriteLine(builder.Configuration["Jwt:Key"]);




// ================= CORS =================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


// ================= SWAGGER =================
builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ServicePro API",
        Version = "v1"
    });

    // JWT Support in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token like: Bearer {token}"
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
            Array.Empty<string>()
        }
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseSerilogRequestLogging();



app.MapControllers();

app.Run();

using API_Sebben_que_e_agenda.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using API_Sebben_que_é_agenda.Interfaces;
using API_Sebben_que_é_agenda.Services;

var builder = WebApplication.CreateBuilder(args);

// Connection string
var conn = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada.");
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(conn));

// JWT (lendo de "Jwt": { Key, Issuer, Audience, ... } )
var jwt = builder.Configuration.GetSection("Jwt");
string issuer = jwt["Issuer"] ?? throw new InvalidOperationException("Config ausente: Jwt:Issuer");
string audience = jwt["Audience"] ?? throw new InvalidOperationException("Config ausente: Jwt:Audience");
string secret = jwt["Key"] ?? throw new InvalidOperationException("Config ausente: Jwt:Key");

if (secret.Length < 32)
    throw new InvalidOperationException("Jwt:Key precisa ter ao menos 32 caracteres.");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = signingKey
    };
});
builder.Services
    .AddOptions<SmtpOptions>()
    .Bind(builder.Configuration.GetSection("Smtp"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// CORS aberto (dev)
const string AllowExpo = "AllowExpo";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowExpo, policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
    );
});

// Kestrel ouvindo na rede local (HTTP 5161)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5161); // http://0.0.0.0:5161
    // options.ListenAnyIP(7162, lo => lo.UseHttps()); // se quiser HTTPS com cert válido
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(AllowExpo);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();



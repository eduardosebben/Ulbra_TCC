using API_Sebben_que_e_agenda.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using API_Sebben_que_é_agenda.Interfaces;
using API_Sebben_que_é_agenda.Services;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var env = builder.Environment;

// ------------------ CONEXÃO COM BANCO ------------------
var conn = configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada.");

builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(conn));

// ------------------ JWT ------------------
var jwt = configuration.GetSection("Jwt");
string issuer = jwt["Issuer"] ?? throw new InvalidOperationException("Config ausente: Jwt:Issuer");
string audience = jwt["Audience"] ?? throw new InvalidOperationException("Config ausente: Jwt:Audience");
string secret = jwt["Key"] ?? throw new InvalidOperationException("Config ausente: Jwt:Key");

if (secret.Length < 32)
    throw new InvalidOperationException("Jwt:Key precisa ter ao menos 32 caracteres.");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

builder.Services
    .AddAuthentication(options =>
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

// ------------------ SMTP / E-MAIL ------------------
builder.Services
    .AddOptions<SmtpOptions>()
    .Bind(configuration.GetSection("Smtp"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

// ------------------ MVC / SWAGGER / CORS ------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string AllowExpo = "AllowExpo";

builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowExpo, policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ⚠️ IMPORTANTE: só fixa a porta 5161 em DESENVOLVIMENTO.
// No Azure, deixa o Kestrel usar a porta que a plataforma informar.
if (env.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(5161); // http://0.0.0.0:5161 na sua máquina
    });
}

var app = builder.Build();

// Swagger: se quiser, pode deixar também em produção enquanto estiver no TCC
if (env.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

// Aplica migrations automaticamente (Azure + local)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseCors(AllowExpo);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

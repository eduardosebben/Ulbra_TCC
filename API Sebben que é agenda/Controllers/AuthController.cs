using API_Sebben_que_e_agenda.Data;
using API_Sebben_que_é_agenda.DTOs;
using API_Sebben_que_é_agenda.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API_Sebben_que_e_agenda.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] RegistroUsuarioDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var erros = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new { Mensagem = "Erro de validação", Erros = erros });
            }

            if (await _context.Usuarios.AnyAsync(x => x.Email == dto.Email))
                return BadRequest("E-mail já cadastrado");

            var usuario = new Usuario
            {
                Nome = dto.Nome,
                Email = dto.Email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
                TipUsuario = dto.TipUsuario
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok("Usuário registrado com sucesso");
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUsuarioDTO dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
                return Unauthorized("Usuário ou senha inválidos");

            try
            {
                var token = GerarToken(usuario); // mantém sua função atual que retorna string
                                                 // Retorne o token + dados úteis (id, nome, email)
                return Ok(new
                {
                    token,
                    userId = usuario.Id,
                    nome = usuario.Nome,
                    email = usuario.Email, 
                    TipUsuario = usuario.TipUsuario
                });
            }
            catch (InvalidOperationException cfgEx)
            {
                return Problem(cfgEx.Message);
            }
            catch (Exception ex)
            {
                return Problem($"Erro ao gerar token: {ex.Message}");
            }
        }
        private string GerarToken(Usuario usuario)
        {
            var issuer = _config["Jwt:Issuer"] ?? throw new InvalidOperationException("Config ausente: Jwt:Issuer");
            var audience = _config["Jwt:Audience"] ?? throw new InvalidOperationException("Config ausente: Jwt:Audience");
            var secret = _config["Jwt:Key"] ?? throw new InvalidOperationException("Config ausente: Jwt:Key");
            var expireStr = _config["Jwt:ExpireDays"];
            var expireDays = int.TryParse(expireStr, out var d) ? d : 3;

            if (secret.Length < 32)
                throw new InvalidOperationException("Jwt:Key precisa ter ao menos 32 caracteres.");

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, usuario.Nome ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                // new Claim(JwtRegisteredClaimNames.Email, usuario.Email ?? string.Empty)
            };

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddDays(expireDays),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
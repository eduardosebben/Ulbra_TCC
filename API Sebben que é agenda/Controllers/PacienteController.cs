using API_Sebben_que_e_agenda.Data;
using API_Sebben_que_é_agenda.DTOs;
using API_Sebben_que_é_agenda.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API_Sebben_que_é_agenda.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PacienteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PacienteController(AppDbContext context)
        {
            _context = context;
        }

        int GetCurrentUserId(ClaimsPrincipal user)
        {
            var id = user.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? user.FindFirstValue("sub");
            return int.Parse(id!);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Paciente>>> Get()
        {
            var userId = GetCurrentUserId(User);
            var query = await _context.Pacientes.ToListAsync();
            query = query.Where(x => x.idUsuario == userId).ToList();
            return Ok(query);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Paciente>> Get(int id)
        {
            var paciente = await _context.Pacientes.FindAsync(id);
            return paciente == null ? NotFound() : paciente;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Paciente>> Post(PacienteCreateDto dto)
        {
            var userId = GetCurrentUserId(User);
            var paciente = new Paciente()
            {
                Nome = dto.Nome,
                CPF = dto.CPF,
                DataNascimento = dto.DataNascimento,
                Email = dto.Email,
                Telefone = dto.Telefone,
                idUsuario = userId
            };
            try
            {
                _context.Pacientes.Add(paciente);
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                return Problem(ex.Message.ToString());
            }
            return CreatedAtAction(nameof(Get), new { id = paciente.Id }, paciente);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(int id, Paciente paciente)
        {
            if (id != paciente.Id) return BadRequest();
            _context.Entry(paciente).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var paciente = await _context.Pacientes.FindAsync(id);
            if (paciente == null) return NotFound();
            _context.Pacientes.Remove(paciente);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

}

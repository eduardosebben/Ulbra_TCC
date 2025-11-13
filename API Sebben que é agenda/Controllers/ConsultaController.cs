using API_Sebben_que_e_agenda.Data;
using API_Sebben_que_é_agenda.DTOs;
using API_Sebben_que_é_agenda.Interfaces;
using API_Sebben_que_é_agenda.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API_Sebben_que_é_agenda.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsultaController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailSender _email;

        public ConsultaController(AppDbContext context, IEmailSender email)
        {
            _context = context;
            _email = email; 
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Consulta>> Get(int id)
        {
            var consulta = await _context.Consultas
                .Include(c => c.Medico)
                .Include(c => c.Paciente)
                .FirstOrDefaultAsync(c => c.Id == id);

            return consulta == null ? NotFound() : consulta;
        }
        int GetCurrentUserId(ClaimsPrincipal user)
        {
            var id = user.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? user.FindFirstValue("sub");
            return int.Parse(id!);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ConsultaRespDto>>> Get([FromQuery] ConsultasGetDto dto)
        {
            var userId = GetCurrentUserId(User);
            try
            {
                var query = _context.Consultas.AsNoTracking()
                    .Include(c => c.Medico).Include(c => c.Paciente)
                    .Include(c => c.Medico.Especialidade).AsQueryable(); 
                query = query.Where(x => x.idUsuario == userId); 

                if (dto.MedicoId.HasValue && dto.MedicoId.Value > 0) 
                    query = query.Where(x => x.MedicoId == dto.MedicoId);

                if (dto.PacienteId.HasValue && dto.PacienteId.Value > 0) 
                    query = query.Where(x => x.PacienteId == dto.PacienteId); 

                if (dto.Data.HasValue) { 
                    var start = dto.Data.Value.ToDateTime(TimeOnly.MinValue);
                    var end = start.AddDays(1); query = query.Where(x => x.DataHora >= start && x.DataHora < end);
                }
                var result = await query.OrderBy(x => x.DataHora).Select(c => new ConsultaRespDto {
                    Id = c.Id, 
                    DataHora = c.DataHora, 
                    Observacoes = c.Observacoes,
                    MedicoId = c.MedicoId, 
                    PacienteId = c.PacienteId,
                    MedicoNome = c.Medico.Nome, 
                    PacienteNome = c.Paciente.Nome, 
                    Especialidade = c.Medico.Especialidade.Nome, 
                }).ToListAsync();
                return Ok(result);
            }
            catch (Exception ex) { 
                return Problem(ex.Message);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Consulta>> Post([FromBody] ConsultaCreateDto dto)
        {
            var userId = GetCurrentUserId(User);
            var consulta = new Consulta
            {
                PacienteId = dto.PacienteId,
                MedicoId = dto.MedicoId,
                DataHora = dto.DataHora,           // já no fuso local que você enviou
                Observacoes = dto.Observacoes ?? "",
                idUsuario = userId
            };
            try
            {
                _context.Consultas.Add(consulta);
                await _context.SaveChangesAsync();
            }catch (Exception ex) 
            { 
                return Problem(ex.Message.ToString());
            }
            var loaded = await _context.Consultas
                .Include(c => c.Medico).ThenInclude(m => m.Especialidade)
                .Include(c => c.Paciente)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == consulta.Id);

            // Envia e-mail (não falha a requisição se der erro)
            try
            {
                if (loaded != null)
                    await _email.SendConsultaConfirmacao(loaded);
            }
            catch (Exception)
            {
                // log se quiser, mas não retorne erro para o app
            }

            return CreatedAtAction(nameof(Get), new { id = consulta.Id }, consulta);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(int id, [FromBody] ConsultaUpdateDto dto)
        {
            var userId = GetCurrentUserId(User);
            if (id != dto.Id) return BadRequest();
            var consulta = await _context.Consultas
                .FirstOrDefaultAsync(c => c.Id == id && c.idUsuario == userId);

            if (consulta == null) return NotFound();

            consulta.MedicoId = dto.MedicoId;
            consulta.PacienteId = dto.PacienteId;
            consulta.DataHora = dto.DataHora;
            consulta.Observacoes = dto.Observacoes ?? "";


            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var consulta = await _context.Consultas.FindAsync(id);
            if (consulta == null) return NotFound();
            _context.Consultas.Remove(consulta);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

}

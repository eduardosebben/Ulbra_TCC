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
    public class MedicoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MedicoController(AppDbContext context) => _context = context;

        int GetCurrentUserId(ClaimsPrincipal user)
        {
            var id = user.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? user.FindFirstValue("sub");
            return int.Parse(id!);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MedicosGetDTO>>> Get()
        {
            var userId = GetCurrentUserId(User);
            var List = await _context.Medicos
                .Include(m => m.Especialidade)
                .Where(m => m.idUsuario == userId)
                .ToListAsync();

            var medicosList = new List<MedicosGetDTO>();
            List.ForEach(list =>
            {
                var medico = new MedicosGetDTO()
                {
                    Id = list.Id,
                    Nome = list.Nome
                };
                medicosList.Add(medico);    
            });
            return medicosList;
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Medico>> Get(int id)
        {
            var medico = await _context.Medicos
                .Include(m => m.Especialidade)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medico == null)
                return NotFound();

            return medico;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Medico>> Post(MedicoCreateDto dto)
        {
            var userId = GetCurrentUserId(User);
            var medico = new Medico
            {
                CRM = dto.CRM,
                Nome = dto.Nome ?? "",
                EspecialidadeId = dto.idEspecialidade,
                idUsuario = userId
            };
            try
            {
                _context.Medicos.Add(medico);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(Get), new { id = medico.Id }, medico);
            }catch (Exception ex)
            {
                return Problem(ex.Message.ToString());
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(int id, Medico medico)
        {
            if (id != medico.Id)
                return BadRequest();

            _context.Entry(medico).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var medico = await _context.Medicos.FindAsync(id);
            if (medico == null)
                return NotFound();

            _context.Medicos.Remove(medico);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

}

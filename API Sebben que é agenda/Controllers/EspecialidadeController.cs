using API_Sebben_que_e_agenda.Data;
using API_Sebben_que_é_agenda.DTOs;
using API_Sebben_que_é_agenda.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_Sebben_que_é_agenda.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EspecialidadeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EspecialidadeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Especialidade>>> Get() =>
            await _context.Especialidades.ToListAsync();

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Especialidade>> Get(int id)
        {
            var especialidade = await _context.Especialidades.FindAsync(id);
            return especialidade == null ? NotFound() : especialidade;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Especialidade>> Post(EspecialidadeCreateDto dto)
        {
            var especialidade = new Especialidade
            {
                Nome = dto.Nome
            };
            try
            {
                _context.Especialidades.Add(especialidade);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message.ToString());
            }
            return CreatedAtAction(nameof(Get), new { id = especialidade.Id }, especialidade);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(int id, Especialidade especialidade)
        {
            if (id != especialidade.Id) return BadRequest();
            _context.Entry(especialidade).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var especialidade = await _context.Especialidades.FindAsync(id);
            if (especialidade == null) return NotFound();
            _context.Especialidades.Remove(especialidade);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

}

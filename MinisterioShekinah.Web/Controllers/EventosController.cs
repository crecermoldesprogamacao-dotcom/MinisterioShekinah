using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinisterioShekinah.Web.Data;
using MinisterioShekinah.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MinisterioShekinah.Web.Controllers
{
    public class EventosController : Controller
    {
        private readonly ShekinahContext _context;

        public EventosController(ShekinahContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var missas = await _context.EventosMissas
                .Include(e => e.Repertorio)
                .Include(e => e.Escala)
                .OrderBy(e => e.DataHora)
                .ToListAsync();

            return View(missas);
        }

        [HttpPost]
        public async Task<IActionResult> Criar(EventoMissa evento)
        {
            if (ModelState.IsValid)
            {
                _context.Add(evento);
                await _context.SaveChangesAsync();
            }

            var missas = await _context.EventosMissas.Include(e => e.Repertorio).Include(e => e.Escala).OrderBy(e => e.DataHora).ToListAsync();
            if (Request.Headers["HX-Request"] == "true")
            {
                return PartialView("_ListaMissas", missas);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AdicionarMusica(int eventoId, string momento, string tituloMusica, string? tomOuCifra)
        {
            if (!string.IsNullOrEmpty(momento) && !string.IsNullOrEmpty(tituloMusica))
            {
                var musica = new MusicaRepertorio
                {
                    EventoMissaId = eventoId,
                    Momento = momento,
                    TituloMusica = tituloMusica,
                    TomOuCifra = tomOuCifra
                };
                _context.Repertorios.Add(musica);
                await _context.SaveChangesAsync();
            }

            var missas = await _context.EventosMissas.Include(e => e.Repertorio).Include(e => e.Escala).OrderBy(e => e.DataHora).ToListAsync();
            if (Request.Headers["HX-Request"] == "true")
            {
                return PartialView("_ListaMissas", missas);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AdicionarMusico(int eventoId, string nomeMusico, string funcao)
        {
            if (!string.IsNullOrEmpty(nomeMusico) && !string.IsNullOrEmpty(funcao))
            {
                var musico = new EscalaMusico
                {
                    EventoMissaId = eventoId,
                    NomeMusico = nomeMusico,
                    Funcao = funcao,
                    Confirmado = false
                };
                _context.Escalas.Add(musico);
                await _context.SaveChangesAsync();
            }

            var missas = await _context.EventosMissas.Include(e => e.Repertorio).Include(e => e.Escala).OrderBy(e => e.DataHora).ToListAsync();
            if (Request.Headers["HX-Request"] == "true")
            {
                return PartialView("_ListaMissas", missas);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AlternarRSVP(int id, int eventoId)
        {
            var musico = await _context.Escalas.FindAsync(id);
            if (musico != null)
            {
                musico.Confirmado = !musico.Confirmado;
                await _context.SaveChangesAsync();
            }

            var missas = await _context.EventosMissas.Include(e => e.Repertorio).Include(e => e.Escala).OrderBy(e => e.DataHora).ToListAsync();
            if (Request.Headers["HX-Request"] == "true")
            {
                return PartialView("_ListaMissas", missas);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
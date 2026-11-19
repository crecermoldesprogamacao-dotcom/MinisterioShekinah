using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinisterioShekinah.Web.Data;
using MinisterioShekinah.Web.Models;
using MinisterioShekinah.Web.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace MinisterioShekinah.Web.Controllers
{
    public class MusicasController : Controller
    {
        private readonly ShekinahContext _context;
        private readonly IAEngineService _iaEngineService;

        public MusicasController(ShekinahContext context, IAEngineService iaEngineService)
        {
            _context = context;
            _iaEngineService = iaEngineService;
        }

        // GET: Musicas
        public async Task<IActionResult> Index()
        {
            var musicas = await _context.Musicas.ToListAsync();
            return View(musicas);
        }

        // GET: Musicas/Criar
        public IActionResult Criar()
        {
            return View();
        }

        // POST: Musicas/Criar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(Musica musica)
        {
            if (!string.IsNullOrEmpty(musica.Cifra))
            {
                var resultadoIa = _iaEngineService.OrganizarCifra(musica.Titulo, musica.Cifra);
                musica.Cifra = resultadoIa.CifraFormatada;
                if (string.IsNullOrEmpty(musica.MomentoLiturgico))
                {
                    musica.MomentoLiturgico = resultadoIa.MomentoSugerido;
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(musica);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(musica);
        }

        // GET: Musicas/Editar/5
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();

            var musica = await _context.Musicas.FindAsync(id);
            if (musica == null) return NotFound();

            return View(musica);
        }

        // POST: Musicas/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Musica musica)
        {
            if (id != musica.Id) return NotFound();

            if (!string.IsNullOrEmpty(musica.Cifra))
            {
                var resultadoIa = _iaEngineService.OrganizarCifra(musica.Titulo, musica.Cifra);
                musica.Cifra = resultadoIa.CifraFormatada;
                if (string.IsNullOrEmpty(musica.MomentoLiturgico))
                {
                    musica.MomentoLiturgico = resultadoIa.MomentoSugerido;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(musica);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Musicas.Any(e => e.Id == musica.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(musica);
        }

        // GET: Musicas/ModoMusico
        public async Task<IActionResult> ModoMusico()
        {
            var musicas = await _context.Musicas.ToListAsync();
            return View(musicas);
        }

        // GET: Musicas/GerarMissal
        public IActionResult GerarMissal()
        {
            return View();
        }

        // Endpoint JSON para o botão "Organizar com IA"
        [HttpPost]
        public JsonResult OrganizarCifraAjax([FromBody] CifraRequestDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.Cifra))
            {
                return Json(new { sucesso = false, mensagem = "Cifra vazia ou inválida." });
            }

            var resultado = _iaEngineService.OrganizarCifra(request.Titulo ?? "Sem Título", request.Cifra);
            return Json(new { sucesso = true, cifraFormatada = resultado.CifraFormatada, tomDetectado = resultado.TomDetectado, momentoSugerido = resultado.MomentoSugerido });
        }
    }

    public class CifraRequestDto
    {
        public string Titulo { get; set; } = string.Empty;
        public string Cifra { get; set; } = string.Empty;
    }
}
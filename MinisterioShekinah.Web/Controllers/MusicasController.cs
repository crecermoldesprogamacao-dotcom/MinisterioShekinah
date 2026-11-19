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
            // Processamento inteligente da cifra e momento litúrgico via IAEngineService (CNBB)
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
    }
}
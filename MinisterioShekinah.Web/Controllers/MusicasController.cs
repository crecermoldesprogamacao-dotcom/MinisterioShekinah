using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinisterioShekinah.Web.Data;
using MinisterioShekinah.Web.Models;

namespace MinisterioShekinah.Web.Controllers
{
    public class MusicasController : Controller
    {
        private readonly ShekinahContext _context;

        public MusicasController(ShekinahContext context)
        {
            _context = context;
        }

        // Exibir detalhes da cifra
        public async Task<IActionResult> Detalhes(int? id)
        {
            if (id == null) return NotFound();
            var musica = await _context.Musicas.FirstOrDefaultAsync(m => m.Id == id);
            if (musica == null) return NotFound();
            return View(musica);
        }

        // GET: Musicas/ModoMusico
        public async Task<IActionResult> ModoMusico()
        {
            var musicas = await _context.Musicas
                .OrderBy(m => m.Titulo)
                .ToListAsync();
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
        public async Task<IActionResult> Criar([Bind("Titulo,Artista,TomOriginal,Capotraste,Cifra,PartituraUrl,MomentoLiturgico")] Musica musica)
        {
            if (ModelState.IsValid)
            {
                _context.Add(musica);
                await _context.SaveChangesAsync();
                return RedirectToAction("Repertorio", "Home");
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
        public async Task<IActionResult> Editar(int id, [Bind("Id,Titulo,Artista,TomOriginal,Capotraste,Cifra,PartituraUrl,MomentoLiturgico")] Musica musica)
        {
            if (id != musica.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(musica);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Musicas.Any(e => e.Id == musica.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction("Repertorio", "Home");
            }
            return View(musica);
        }

        // GET: Musicas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var musica = await _context.Musicas.FirstOrDefaultAsync(m => m.Id == id);
            if (musica == null) return NotFound();
            return View(musica);
        }

        // POST: Musicas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var musica = await _context.Musicas.FindAsync(id);
            if (musica != null)
            {
                _context.Musicas.Remove(musica);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Repertorio", "Home");
        }

        // GET: Musicas/SelecionarMissal
        public async Task<IActionResult> SelecionarMissal()
        {
            var musicas = await _context.Musicas.OrderBy(m => m.Titulo).ToListAsync();
            return View(musicas);
        }

        // POST: Musicas/ExportarPdfMissa
        [HttpPost]
        public async Task<IActionResult> ExportarPdfMissa(string nomeEvento, List<int> musicasIds)
        {
            if (musicasIds == null || !musicasIds.Any())
            {
                ModelState.AddModelError(string.Empty, "Selecione pelo menos uma música para a Missa ou Evento.");
                var musicasLista = await _context.Musicas.OrderBy(m => m.Titulo).ToListAsync();
                return View("SelecionarMissal", musicasLista);
            }

            var musicasSelecionadas = await _context.Musicas
                .Where(m => musicasIds.Contains(m.Id))
                .ToListAsync();

            var viewModel = new
            {
                NomeEvento = string.IsNullOrEmpty(nomeEvento) ? "Santa Missa / Celebração" : nomeEvento,
                Musicas = musicasSelecionadas
            };

            return View("PdfMissaTemplate", viewModel);
        }
    }
}
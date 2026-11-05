using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MinisterioShekinah.Web.Data;
using MinisterioShekinah.Web.Hubs;
using MinisterioShekinah.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MinisterioShekinah.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ShekinahContext _context;
        private readonly IHubContext<VelaHub> _hubContext;

        public HomeController(ShekinahContext context, IHubContext<VelaHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var musicas = await _context.Musicas
                .OrderBy(m => m.Titulo)
                .ToListAsync();
            return View(musicas);
        }

        public async Task<IActionResult> Repertorio()
        {
            var musicas = await _context.Musicas
                .OrderBy(m => m.Titulo)
                .ToListAsync();
            return View(musicas);
        }

        public IActionResult Agenda()
        {
            return View();
        }

        public IActionResult Videos()
        {
            return View();
        }

        public IActionResult Sobre()
        {
            return View();
        }

        // GET: /Home/PedidoOracao
        public IActionResult PedidoOracao()
        {
            return View();
        }

        // GET: /Home/VelaVirtual
        public async Task<IActionResult> VelaVirtual()
        {
            // Busca apenas as velas ativas ordenadas por data de forma segura
            var velas = await _context.VelasVirtuais
                .Where(v => v.Ativa)
                .OrderByDescending(v => v.DataAcesa)
                .ToListAsync();

            return View(velas);
        }

        // POST: /Home/AcenderVela
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcenderVela(string nomeFiel, string intencao, string corVela)
        {
            if (!string.IsNullOrEmpty(nomeFiel) && !string.IsNullOrEmpty(intencao))
            {
                var novaVela = new VelaVirtual
                {
                    NomeFiel = nomeFiel,
                    Intencao = intencao,
                    CorVela = string.IsNullOrEmpty(corVela) ? "branco" : corVela,
                    DataAcesa = DateTime.Now,
                    Ativa = true
                };

                _context.VelasVirtuais.Add(novaVela);
                await _context.SaveChangesAsync();

                // Transmite os dados da nova vela em tempo real
                await _hubContext.Clients.All.SendAsync("ReceberNovaVela", new
                {
                    nomeFiel = novaVela.NomeFiel,
                    intencao = novaVela.Intencao,
                    corVela = novaVela.CorVela,
                    dataAcesa = novaVela.DataAcesa.ToString("dd/MM/yyyy HH:mm"),
                    alturaPx = 75,
                    escalaChama = 1.0
                });

                TempData["Sucesso"] = "A sua vela foi acesa com fé! O Ministério Shekinah está intercedendo por você.";
            }

            return RedirectToAction("VelaVirtual");
        }
    }
}
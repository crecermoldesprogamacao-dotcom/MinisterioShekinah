using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MinisterioShekinah.Web.Data;
using MinisterioShekinah.Web.Hubs;
using MinisterioShekinah.Web.Models;
using MinisterioShekinah.Web.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MinisterioShekinah.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ShekinahContext _context;
        private readonly IHubContext<VelaHub> _hubContext;
        private readonly IAEngineService _iaService;

        public HomeController(ShekinahContext context, IHubContext<VelaHub> hubContext, IAEngineService iaService)
        {
            _context = context;
            _hubContext = hubContext;
            _iaService = iaService;
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
            // 1. Moderação Inteligente por IA
            if (!_iaService.ValidarTextoSeguro(nomeFiel) || !_iaService.ValidarTextoSeguro(intencao))
            {
                TempData["Erro"] = "O seu pedido contém termos não permitidos pelas diretrizes de moderação do Ministério.";
                return RedirectToAction(nameof(VelaVirtual));
            }

            if (!string.IsNullOrEmpty(nomeFiel) && !string.IsNullOrEmpty(intencao))
            {
                // 2. Classificação Automática da Intenção por IA
                string categoriaSugerida = _iaService.CategorizarIntencao(intencao);

                var novaVela = new VelaVirtual
                {
                    NomeFiel = nomeFiel,
                    Intencao = intencao,
                    CorVela = string.IsNullOrEmpty(corVela) ? "branco" : corVela,
                    DataAcesa = DateTime.UtcNow,
                    Ativa = true
                };

                _context.VelasVirtuais.Add(novaVela);
                await _context.SaveChangesAsync();

                // Transmite os dados da nova vela em tempo real via SignalR
                await _hubContext.Clients.All.SendAsync("ReceberNovaVela", new
                {
                    nomeFiel = novaVela.NomeFiel,
                    intencao = novaVela.Intencao,
                    corVela = novaVela.CorVela,
                    dataAcesa = novaVela.DataAcesa.ToString("dd/MM/yyyy HH:mm"),
                    alturaPx = 75,
                    escalaChama = 1.0
                });

                TempData["Sucesso"] = $"A sua vela foi acesa com fé! Categoria espiritual identificada: {categoriaSugerida}.";
            }
            return RedirectToAction("VelaVirtual");
        }
    }
}
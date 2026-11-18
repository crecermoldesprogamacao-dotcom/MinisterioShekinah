using Microsoft.AspNetCore.Mvc;
using MinisterioShekinah.Web.Data;
using MinisterioShekinah.Web.Models;
using MinisterioShekinah.Web.Services;

namespace MinisterioShekinah.Web.Controllers
{
    public class MusicasController : Controller
    {
        private readonly ShekinahContext _context;
        private readonly IAEngineService _aiService;
        private readonly LiturgiaService _liturgiaService;

        public MusicasController(ShekinahContext context, IAEngineService aiService, LiturgiaService liturgiaService)
        {
            _context = context;
            _aiService = aiService;
            _liturgiaService = liturgiaService;
        }

        // GET: Musicas/Index
        public IActionResult Index()
        {
            var musicas = _context.Musicas.ToList();
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
        public IActionResult Criar(Musica musica)
        {
            if (ModelState.IsValid)
            {
                if (_aiService.VerificarMusicaDuplicada(musica.Titulo ?? "", musica.Cifra ?? "", out string nomeExistente))
                {
                    ModelState.AddModelError("", $"Esta música parece ser duplicada ou semelhante a '{nomeExistente}' já cadastrada no sistema.");
                    return View(musica);
                }

                if (!string.IsNullOrEmpty(musica.Cifra))
                {
                    var resultado = _aiService.OrganizarCifra(musica.Titulo ?? "Sem Título", musica.Cifra);
                    musica.Cifra = resultado.CifraFormatada;
                    musica.TomOriginal = resultado.TomDetectado;
                    if (string.IsNullOrEmpty(musica.MomentoLiturgico))
                    {
                        musica.MomentoLiturgico = resultado.MomentoSugerido;
                    }
                }

                _context.Musicas.Add(musica);
                _context.SaveChanges();
                return RedirectToAction("Repertorio", "Home");
            }
            return View(musica);
        }

        // POST: Musicas/OrganizarComIa (Corrigido para aceitar Form Data e JSON sem falhar)
        [HttpPost]
        public IActionResult OrganizarComIa(string? titulo, string? cifra)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cifra))
                {
                    return BadRequest(new { erro = "Insira a cifra antes de organizar." });
                }

                var resultado = _aiService.OrganizarCifra(titulo ?? "Sem Título", cifra);

                return Json(new
                {
                    cifraFormatada = resultado.CifraFormatada,
                    tomDetectado = resultado.TomDetectado,
                    momentoSugerido = resultado.MomentoSugerido,
                    sugestaoCapotraste = resultado.SugestaoCapotraste
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = $"Erro interno ao organizar cifra: {ex.Message}" });
            }
        }

        // POST: Musicas/EnviarFeedbackIa (Regista o aprendizado contínuo)
        [HttpPost]
        public async Task<IActionResult> EnviarFeedbackIa([FromBody] FeedbackRequestDto request)
        {
            if (request == null) return BadRequest(new { sucesso = false });

            await _aiService.RegistarFeedbackAsync(request.TipoAcao, request.TextoOriginal, request.GeradoPorIa, request.CorrecaoDoUsuario);
            return Json(new { sucesso = true, mensagem = "Feedback registado com sucesso. A IA aprendeu com esta alteração!" });
        }

        // GET ou POST: Musicas/ModoMusico (Modo Altar)
        public IActionResult ModoMusico(List<int>? musicasIds, string? NomeEvento, string? CorLiturgica, string? SalmoRefrao)
        {
            ViewBag.NomeEvento = NomeEvento;
            ViewBag.CorLiturgica = CorLiturgica;
            ViewBag.SalmoRefrao = SalmoRefrao;

            if (musicasIds != null && musicasIds.Any())
            {
                var selecionadas = OrdenarPorRitoLiturgico(
                    _context.Musicas.Where(m => musicasIds.Contains(m.Id)).ToList()
                );
                return View(selecionadas);
            }

            var todas = OrdenarPorRitoLiturgico(_context.Musicas.ToList());
            return View(todas);
        }

        // GET: Musicas/GerarMissal
        [HttpGet]
        public IActionResult GerarMissal()
        {
            var musicas = OrdenarPorRitoLiturgico(_context.Musicas.ToList());
            return View(musicas);
        }

        // GET: Musicas/SugerirPorLiturgiaAjax
        [HttpGet]
        public async Task<IActionResult> SugerirPorLiturgiaAjax(string data)
        {
            if (!DateTime.TryParse(data, out DateTime dataCeleb))
            {
                dataCeleb = DateTime.Today;
            }

            var liturgia = await _liturgiaService.ObterLiturgiaPorDataAsync(dataCeleb);
            var todasMusicas = _context.Musicas.ToList();
            var sugestoes = _aiService.SugerirRepertorioPorLiturgia(todasMusicas, liturgia);

            Musica? salmoEncontrado = null;
            if (!string.IsNullOrWhiteSpace(liturgia.SalmoRefrao))
            {
                var palavrasRefrao = liturgia.SalmoRefrao.ToLower()
                    .Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(p => p.Length > 4)
                    .ToList();

                salmoEncontrado = todasMusicas
                    .Where(m => (m.MomentoLiturgico ?? "").Contains("Salmo", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(m => palavrasRefrao.Count(p => $"{m.Titulo} {m.Cifra}".ToLower().Contains(p)))
                    .FirstOrDefault(m => palavrasRefrao.Any(p => $"{m.Titulo} {m.Cifra}".ToLower().Contains(p)));

                if (salmoEncontrado != null)
                {
                    sugestoes["Salmo Responsorial"] = salmoEncontrado;
                }
            }

            var idsSugeridos = sugestoes.Values.Select(m => m.Id).Distinct().ToList();

            return Json(new
            {
                sucesso = liturgia.Sucesso,
                dataFormatada = liturgia.Data,
                liturgiaTitulo = liturgia.Liturgia,
                cor = liturgia.Cor,
                salmoRefrao = liturgia.SalmoRefrao,
                salmoTexto = liturgia.SalmoTexto,
                salmoCadastradoTitulo = salmoEncontrado?.Titulo,
                ehQuaresma = liturgia.Liturgia.Contains("Quaresma", StringComparison.OrdinalIgnoreCase),
                idsSugeridos = idsSugeridos
            });
        }

        // POST ou GET: Musicas/ExportarPdfMissa
        public IActionResult ExportarPdfMissa(List<int>? musicasIds, string? NomeEvento, string? CorLiturgica, string? SalmoRefrao, string? SalmoTexto)
        {
            ViewBag.NomeEvento = string.IsNullOrWhiteSpace(NomeEvento) ? "Santa Missa" : NomeEvento;
            ViewBag.CorLiturgica = CorLiturgica;
            ViewBag.SalmoRefrao = SalmoRefrao;
            ViewBag.SalmoTexto = SalmoTexto;

            List<Musica> musicasSelecionadas;
            if (musicasIds != null && musicasIds.Any())
            {
                musicasSelecionadas = _context.Musicas
                    .Where(m => musicasIds.Contains(m.Id))
                    .ToList();
            }
            else
            {
                var repertorioAuto = _aiService.GerarRepertorioMissaAutomatico();
                musicasSelecionadas = repertorioAuto.Values.Distinct().ToList();
            }

            return View(OrdenarPorRitoLiturgico(musicasSelecionadas));
        }

        // GET ou POST: Musicas/EnsaioInteligente
        public IActionResult EnsaioInteligente(List<int>? musicasIds, string? NomeEvento)
        {
            ViewBag.NomeEvento = string.IsNullOrWhiteSpace(NomeEvento) ? "Repertório da Santa Missa" : NomeEvento;

            List<Musica> musicasSelecionadas;
            if (musicasIds != null && musicasIds.Any())
            {
                musicasSelecionadas = _context.Musicas
                    .Where(m => musicasIds.Contains(m.Id))
                    .ToList();
            }
            else
            {
                var repertorioAuto = _aiService.GerarRepertorioMissaAutomatico();
                musicasSelecionadas = repertorioAuto.Values.Distinct().ToList();
            }

            var relatorioEnsaio = musicasSelecionadas.Select(m => {
                var analise = _aiService.OrganizarCifra(m.Titulo, m.Cifra ?? "");
                return new EnsaioItemViewModel
                {
                    Musica = m,
                    Dificuldade = analise.DificuldadeNivel,
                    Tessitura = analise.TessituraSugerida,
                    RitmoBpm = analise.RitmoSugerido,
                    SugestaoCapo = analise.SugestaoCapotraste
                };
            }).ToList();

            return View(relatorioEnsaio);
        }

        // GET: Musicas/Teleprompter/5 (Individual)
        public IActionResult Teleprompter(int? id)
        {
            if (id == null) return NotFound();
            var musica = _context.Musicas.FirstOrDefault(m => m.Id == id);
            if (musica == null) return NotFound();
            return View(new List<Musica> { musica });
        }

        // GET ou POST: Musicas/TeleprompterMissa (Sequência completa da Missa)
        public IActionResult TeleprompterMissa(List<int>? musicasIds, string? NomeEvento, string? CorLiturgica, string? SalmoRefrao)
        {
            ViewBag.NomeEvento = string.IsNullOrWhiteSpace(NomeEvento) ? "Repertório da Santa Missa" : NomeEvento;
            ViewBag.CorLiturgica = CorLiturgica;
            ViewBag.SalmoRefrao = SalmoRefrao;

            List<Musica> musicasSelecionadas;
            if (musicasIds != null && musicasIds.Any())
            {
                musicasSelecionadas = _context.Musicas
                    .Where(m => musicasIds.Contains(m.Id))
                    .ToList();
            }
            else
            {
                var repertorioAuto = _aiService.GerarRepertorioMissaAutomatico();
                musicasSelecionadas = repertorioAuto.Values.Distinct().ToList();
            }

            return View("Teleprompter", OrdenarPorRitoLiturgico(musicasSelecionadas));
        }

        private List<Musica> OrdenarPorRitoLiturgico(List<Musica> musicas)
        {
            var ordemRitos = new List<string>
            {
                "Entrada", "Ato Penitencial", "Hino de Louvor", "Glória", "Salmo",
                "Aclamação", "Ofertório", "Santo", "Amém", "Cordeiro",
                "Comunhão", "Pós-Comunhão", "Final", "Adoração"
            };

            int ObterPeso(string? momento)
            {
                if (string.IsNullOrWhiteSpace(momento)) return 99;
                for (int i = 0; i < ordemRitos.Count; i++)
                {
                    if (momento.Contains(ordemRitos[i], StringComparison.OrdinalIgnoreCase))
                        return i;
                }
                return 98;
            }

            return musicas.OrderBy(m => ObterPeso(m.MomentoLiturgico)).ThenBy(m => m.Titulo).ToList();
        }

        // GET: Musicas/Detalhes/5
        public IActionResult Detalhes(int? id)
        {
            if (id == null) return NotFound();
            var musica = _context.Musicas.FirstOrDefault(m => m.Id == id);
            if (musica == null) return NotFound();
            return View(musica);
        }

        // GET: Musicas/Editar/5
        public IActionResult Editar(int? id)
        {
            if (id == null) return NotFound();
            var musica = _context.Musicas.FirstOrDefault(m => m.Id == id);
            if (musica == null) return NotFound();
            return View(musica);
        }

        // POST: Musicas/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Musica musica)
        {
            if (id != musica.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(musica);
                    _context.SaveChanges();
                }
                catch
                {
                    if (!_context.Musicas.Any(e => e.Id == musica.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction("Repertorio", "Home");
            }
            return View(musica);
        }
    }

    public class OrganizarCifraRequest
    {
        public string? Titulo { get; set; }
        public string? Cifra { get; set; }
    }

    public class FeedbackRequestDto
    {
        public string TipoAcao { get; set; } = string.Empty;
        public string TextoOriginal { get; set; } = string.Empty;
        public string GeradoPorIa { get; set; } = string.Empty;
        public string CorrecaoDoUsuario { get; set; } = string.Empty;
    }

    public class EnsaioItemViewModel
    {
        public Musica Musica { get; set; } = new();
        public string Dificuldade { get; set; } = string.Empty;
        public string Tessitura { get; set; } = string.Empty;
        public string RitmoBpm { get; set; } = string.Empty;
        public string SugestaoCapo { get; set; } = string.Empty;
    }
}
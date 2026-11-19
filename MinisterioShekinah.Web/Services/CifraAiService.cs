using Microsoft.ML;
using Microsoft.ML.Data;
using MinisterioShekinah.Web.Data;
using MinisterioShekinah.Web.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace MinisterioShekinah.Web.Services
{
    public class ResultadoOrganizacaoCifra
    {
        public string CifraFormatada { get; set; } = string.Empty;
        public string TomDetectado { get; set; } = string.Empty;
        public string MomentoSugerido { get; set; } = string.Empty;
        public string SugestaoCapotraste { get; set; } = string.Empty;
        public string TessituraSugerida { get; set; } = string.Empty;
        public string RitmoSugerido { get; set; } = string.Empty;
        public string DificuldadeNivel { get; set; } = "Fácil";
    }

    public class MusicaTreinamentoData
    {
        public string Texto { get; set; } = string.Empty;
        public string MomentoLiturgico { get; set; } = string.Empty;
    }

    public class MusicaPredicao
    {
        [ColumnName("PredictedLabel")]
        public string MomentoPrevisto { get; set; } = string.Empty;
    }

    public class CifraAiService
    {
        private readonly MLContext _mlContext;
        private PredictionEngine<MusicaTreinamentoData, MusicaPredicao>? _predictionEngine;
        private readonly IServiceProvider _serviceProvider;
        private readonly System.Threading.Lock _lock = new();

        public CifraAiService(IServiceProvider serviceProvider)
        {
            _mlContext = new MLContext(seed: 0);
            _serviceProvider = serviceProvider;
            RetreinarModelo();
        }

        public void RetreinarModelo()
        {
            lock (_lock)
            {
                var dadosTreinamento = new List<MusicaTreinamentoData>
                {
                    new() { Texto = "Vimos te adorar ó Senhor entramos em tua casa com alegria celebrar", MomentoLiturgico = "Entrada" },
                    new() { Texto = "Aqui estamos Senhor reunidos em teu nome abre as portas do teu templo", MomentoLiturgico = "Entrada" },
                    new() { Texto = "Senhor tende piedade de nós Cristo tende piedade perdoa os nossos pecados", MomentoLiturgico = "Ato Penitencial" },
                    new() { Texto = "Kyrie eleison misericórdia Senhor eu pequei perdoa me", MomentoLiturgico = "Ato Penitencial" },
                    new() { Texto = "Glória a Deus nas alturas e paz na terra aos homens por Ele amados", MomentoLiturgico = "Hino de Louvor (Glória)" },
                    new() { Texto = "Aleluia aleluia a tua palavra é luz para o meu caminho evangelho", MomentoLiturgico = "Aclamação ao Evangelho" },
                    new() { Texto = "Buscai primeiro o reino de Deus aleluia palavra de salvação", MomentoLiturgico = "Aclamação ao Evangelho" },
                    new() { Texto = "Pão e vinho te apresentamos neste altar frutos da terra e do trabalho", MomentoLiturgico = "Ofertório" },
                    new() { Texto = "Recebe Senhor nossa oferta nossas vidas colocamos sobre o altar", MomentoLiturgico = "Ofertório" },
                    new() { Texto = "Santo Santo Santo Senhor Deus do universo o céu e a terra proclamam", MomentoLiturgico = "Santo" },
                    new() { Texto = "Bendito o que vem em nome do Senhor hosana nas alturas", MomentoLiturgico = "Santo" },
                    new() { Texto = "Amém por Cristo com Cristo e em Cristo na unidade do Espírito Santo", MomentoLiturgico = "Amém (Doxologia)" },
                    new() { Texto = "Eis o pão da vida corpo e sangue de Cristo alimento da alma comunhão", MomentoLiturgico = "Comunhão" },
                    new() { Texto = "Tomai e comei isto é o meu corpo banquete do céu Jesus eucarístico", MomentoLiturgico = "Comunhão" },
                    new() { Texto = "Cordeiro de Deus que tirais o pecado do mundo tende piedade", MomentoLiturgico = "Cordeiro de Deus" },
                    new() { Texto = "Graças te damos Senhor pelo teu amor obrigado Jesus ação de graças", MomentoLiturgico = "Pós-Comunhão" },
                    new() { Texto = "Ide em paz anunciar o evangelho missão enviar pelo mundo", MomentoLiturgico = "Final / Envio" },
                    new() { Texto = "Tão sublime sacramento adoremos neste altar Jesus presente adoração", MomentoLiturgico = "Adoração / Louvor Geral" }
                };

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ShekinahContext>();
                    var musicasBanco = db.Musicas
                        .Where(m => !string.IsNullOrEmpty(m.MomentoLiturgico) && !string.IsNullOrEmpty(m.Cifra))
                        .ToList();

                    foreach (var m in musicasBanco)
                    {
                        dadosTreinamento.Add(new MusicaTreinamentoData
                        {
                            Texto = $"{m.Titulo} {ExtrairApenasLetra(m.Cifra!)}",
                            MomentoLiturgico = m.MomentoLiturgico!
                        });
                    }
                }
                catch { }

                var dataView = _mlContext.Data.LoadFromEnumerable(dadosTreinamento);
                var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "MomentoLiturgico", outputColumnName: "Label")
                    .Append(_mlContext.Transforms.Text.FeaturizeText(inputColumnName: "Texto", outputColumnName: "Features"))
                    .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
                    .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

                var modeloTreinado = pipeline.Fit(dataView);
                _predictionEngine = _mlContext.Model.CreatePredictionEngine<MusicaTreinamentoData, MusicaPredicao>(modeloTreinado);
            }
        }

        public ResultadoOrganizacaoCifra OrganizarCifra(string titulo, string cifraBruta)
        {
            string cifraComEstruturas = IdentificarEOrganizarEstruturas(cifraBruta);
            string cifraFormatada = FormatarEConverterParaColchetes(cifraComEstruturas);
            string tomDetectado = DetectarTom(cifraFormatada);
            string sugestaoCapo = SugerirCapotraste(tomDetectado);
            string momentoSugerido = "Outro";

            lock (_lock)
            {
                if (_predictionEngine != null)
                {
                    var apenasLetra = ExtrairApenasLetra(cifraFormatada);
                    var predicao = _predictionEngine.Predict(new MusicaTreinamentoData
                    {
                        Texto = $"{titulo} {apenasLetra}"
                    });

                    if (!string.IsNullOrWhiteSpace(predicao.MomentoPrevisto))
                    {
                        momentoSugerido = predicao.MomentoPrevisto;
                    }
                }
            }

            string tessitura = AnalisarTessituraVocal(tomDetectado);
            string ritmo = SugerirRitmoPorMomento(momentoSugerido);
            string dificuldade = AvaliarDificuldadeCifra(cifraFormatada);

            return new ResultadoOrganizacaoCifra
            {
                CifraFormatada = cifraFormatada,
                TomDetectado = tomDetectado,
                MomentoSugerido = momentoSugerido,
                SugestaoCapotraste = sugestaoCapo,
                TessituraSugerida = tessitura,
                RitmoSugerido = ritmo,
                DificuldadeNivel = dificuldade
            };
        }

        private string AnalisarTessituraVocal(string tom)
        {
            string t = tom.ToUpper().Trim();
            if (t.Contains("F#") || t.Contains("GB") || t.Contains("B") || t.Contains("C#"))
                return "Recomendado para Voz Feminina (Soprano/Contralto) ou Capo ajustado";
            if (t.Contains("C") || t.Contains("D") || t.Contains("G"))
                return "Excelente para Voz Mista (Congregação e Coro - Confortável)";
            return "Recomendado para Voz Masculina (Tenor/Barítono)";
        }

        private string SugerirRitmoPorMomento(string momento)
        {
            if (momento.Contains("Entrada") || momento.Contains("Final"))
                return "Pop Rock / Alegrete (BPM: 110-120)";
            if (momento.Contains("Comunhão") || momento.Contains("Adoração"))
                return "Balada Romântica / Dedilhado Suave (BPM: 70-85)";
            if (momento.Contains("Ofertório") || momento.Contains("Santo"))
                return "Marcha Moderada / Batida Folk (BPM: 90-100)";
            return "Ritmo Livre / Contemplativo";
        }

        private string AvaliarDificuldadeCifra(string cifra)
        {
            int pestanas = Regex.Matches(cifra, @"\b(Bm|F#m|Cm|Fm|Bb|F)\b").Count;
            if (pestanas > 6) return "Avançado (Muitas pestanas - Exige ensaio dedicado)";
            if (pestanas > 2) return "Intermediário (Requer atenção em acordes fechados)";
            return "Fácil (Acordes abertos e acessíveis)";
        }

        public List<Musica> BuscaSemanticaPorTema(List<Musica> todasMusicas, string termoBusca)
        {
            if (string.IsNullOrWhiteSpace(termoBusca) || todasMusicas == null) return todasMusicas ?? [];

            string termoNormalizado = NormalizarTexto(termoBusca);
            var palavrasTermo = termoNormalizado.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();

            return todasMusicas
                .OrderByDescending(m =>
                {
                    string textoMusica = NormalizarTexto($"{m.Titulo} {m.Artista} {m.MomentoLiturgico} {ExtrairApenasLetra(m.Cifra ?? "")}");
                    int pontuacao = 0;
                    foreach (var p in palavrasTermo)
                    {
                        if (textoMusica.Contains(p)) pontuacao++;
                    }
                    return pontuacao;
                })
                .ToList();
        }

        public Dictionary<string, Musica> GerarRepertorioMissaAutomatico()
        {
            var repertorioSugerido = new Dictionary<string, Musica>();
            var momentosObrigatorios = new List<string>
            {
                "Entrada",
                "Ato Penitencial",
                "Hino de Louvor (Glória)",
                "Salmo Responsorial",
                "Aclamação ao Evangelho",
                "Ofertório",
                "Santo",
                "Amém (Doxologia)",
                "Cordeiro de Deus",
                "Comunhão",
                "Pós-Comunhão",
                "Final / Envio"
            };

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ShekinahContext>();
            var todasMusicas = db.Musicas.ToList();

            if (!todasMusicas.Any()) return repertorioSugerido;

            var random = new Random();
            foreach (var momento in momentosObrigatorios)
            {
                var candidatas = todasMusicas
                    .Where(m => !string.IsNullOrEmpty(m.MomentoLiturgico) &&
                                m.MomentoLiturgico.Equals(momento, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (candidatas.Any())
                {
                    int index = random.Next(candidatas.Count);
                    repertorioSugerido[momento] = candidatas[index];
                }
                else if (todasMusicas.Any())
                {
                    int index = random.Next(todasMusicas.Count);
                    repertorioSugerido[momento] = todasMusicas[index];
                }
            }

            return repertorioSugerido;
        }

        public Dictionary<string, Musica> SugerirRepertorioPorLiturgia(List<Musica> todasMusicas, LiturgiaDiaDto liturgia)
        {
            var repertorioSugerido = new Dictionary<string, Musica>();
            if (todasMusicas == null || !todasMusicas.Any()) return repertorioSugerido;

            bool ehQuaresmaOuAdvento = liturgia.Liturgia.Contains("Quaresma", StringComparison.OrdinalIgnoreCase) ||
                                       liturgia.Liturgia.Contains("Advento", StringComparison.OrdinalIgnoreCase) ||
                                       liturgia.Cor.Equals("Roxo", StringComparison.OrdinalIgnoreCase);

            var momentos = new List<string>
            {
                "Entrada",
                "Ato Penitencial",
                "Hino de Louvor (Glória)",
                "Salmo Responsorial",
                "Aclamação ao Evangelho",
                "Ofertório",
                "Santo",
                "Amém (Doxologia)",
                "Cordeiro de Deus",
                "Comunhão",
                "Pós-Comunhão",
                "Final / Envio"
            };

            if (liturgia.Liturgia.Contains("Quaresma", StringComparison.OrdinalIgnoreCase))
            {
                momentos.Remove("Hino de Louvor (Glória)");
            }

            string textoBiblico = NormalizarTexto($"{liturgia.Evangelho} {liturgia.PrimeiraLeitura}");
            var palavrasChave = textoBiblico
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(p => p.Length > 4)
                .Distinct()
                .ToHashSet();

            foreach (var momento in momentos)
            {
                var candidatas = todasMusicas
                    .Where(m => !string.IsNullOrEmpty(m.MomentoLiturgico) &&
                                m.MomentoLiturgico.Contains(momento.Split(' ')[0], StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (ehQuaresmaOuAdvento && momento == "Aclamação ao Evangelho")
                {
                    var semAleluia = candidatas
                        .Where(m => !NormalizarTexto($"{m.Titulo} {m.Cifra}").Contains("aleluia"))
                        .ToList();

                    if (semAleluia.Any()) candidatas = semAleluia;
                }

                if (candidatas.Any())
                {
                    var melhorMusica = candidatas
                        .OrderByDescending(m => CalcularAfinidadeBiblica(m, palavrasChave))
                        .First();

                    repertorioSugerido[momento] = melhorMusica;
                }
            }

            return repertorioSugerido;
        }

        private int CalcularAfinidadeBiblica(Musica musica, HashSet<string> palavrasChaveLiturgia)
        {
            string textoMusica = NormalizarTexto($"{musica.Titulo} {ExtrairApenasLetra(musica.Cifra ?? "")}");
            int pontos = 0;

            foreach (var palavra in palavrasChaveLiturgia)
            {
                if (textoMusica.Contains(palavra))
                    pontos++;
            }

            return pontos;
        }

        public bool VerificarMusicaDuplicada(string titulo, string cifra, out string musicaExistenteNome)
        {
            musicaExistenteNome = string.Empty;
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ShekinahContext>();
            var todas = db.Musicas.ToList();

            string tituloNormalizado = NormalizarTexto(titulo);
            string letraNormalizada = NormalizarTexto(ExtrairApenasLetra(cifra));

            foreach (var m in todas)
            {
                string tBanco = NormalizarTexto(m.Titulo);
                if (!string.IsNullOrEmpty(tituloNormalizado) && tBanco == tituloNormalizado)
                {
                    musicaExistenteNome = m.Titulo;
                    return true;
                }

                if (!string.IsNullOrEmpty(letraNormalizada) && !string.IsNullOrEmpty(m.Cifra))
                {
                    string lBanco = NormalizarTexto(ExtrairApenasLetra(m.Cifra));
                    if (lBanco.Length > 30 && letraNormalizada.Length > 30)
                    {
                        if (lBanco.Contains(letraNormalizada) || letraNormalizada.Contains(lBanco))
                        {
                            musicaExistenteNome = m.Titulo;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public string IdentificarEOrganizarEstruturas(string cifraBruta)
        {
            var linhas = cifraBruta.Replace("\r\n", "\n").Split('\n');
            var resultado = new StringBuilder();

            foreach (var linha in linhas)
            {
                string linhaTrim = linha.Trim();
                if (linhaTrim.StartsWith('[') && linhaTrim.EndsWith(']'))
                {
                    resultado.AppendLine(linhaTrim);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(linhaTrim))
                {
                    resultado.AppendLine();
                    continue;
                }

                string linhaLower = linhaTrim.ToLower();
                if (linhaLower.Contains("aleluia") || linhaLower.Contains("santo") || linhaLower.Contains("glória") || linhaLower.Contains("cordeiro") || linhaLower.Contains("amém"))
                {
                    resultado.AppendLine($"[Refrão / Parte Litúrgica]");
                }

                resultado.AppendLine(linha);
            }

            return resultado.ToString().TrimEnd();
        }

        public string SugerirCapotraste(string tomOriginal)
        {
            if (string.IsNullOrWhiteSpace(tomOriginal)) return "Sem sugestão de capo";
            string tom = tomOriginal.Trim().ToUpper();

            var sugestoesCapo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "C#", "Usar Capo na 1ª casa (Formato de C)" },
                { "Db", "Usar Capo na 1ª casa (Formato de C)" },
                { "D#", "Usar Capo na 1ª casa (Formato de D)" },
                { "Eb", "Usar Capo na 1ª casa (Formato de D)" },
                { "F#", "Usar Capo na 2ª casa (Formato de E) ou 4ª casa (Formato de D)" },
                { "GB", "Usar Capo na 2ª casa (Formato de E)" },
                { "G#", "Usar Capo na 1ª casa (Formato de G) ou 4ª casa (Formato de E)" },
                { "Ab", "Usar Capo na 1ª casa (Formato de G)" },
                { "A#", "Usar Capo na 1ª casa (Formato de A)" },
                { "Bb", "Usar Capo na 1ª casa (Formato de A)" }
            };

            if (sugestoesCapo.ContainsKey(tom))
            {
                return sugestoesCapo[tom];
            }

            return "Tom confortável em acordes abertos (Sem necessidade de Capo)";
        }

        private string NormalizarTexto(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return string.Empty;
            var semAcentos = string.Concat(texto.Normalize(System.Text.NormalizationForm.FormD)
                .Where(ch => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch) != System.Globalization.UnicodeCategory.NonSpacingMark));
            return Regex.Replace(semAcentos.ToLower(), @"\s+", " ").Trim();
        }

        private string FormatarEConverterParaColchetes(string texto)
        {
            var linhas = texto.Replace("\r\n", "\n").Split('\n');
            var resultado = new StringBuilder();
            var regexAcorde = new Regex(@"^\[?[A-G](#|b)?(m|maj|min|dim|aug|sus|add)?[0-9]*(\/[A-G](#|b)?)?\]?$", RegexOptions.Compiled);
            var regexSubstituicao = new Regex(@"(?<!\[)\b([A-G](#|b)?(m|maj|min|dim|aug|sus|add)?[0-9]*(\/[A-G](#|b)?)?)\b(?!\])", RegexOptions.Compiled);

            foreach (var linha in linhas)
            {
                string linhaLimpa = linha.Replace("\t", "    ").TrimEnd();
                if (string.IsNullOrWhiteSpace(linhaLimpa))
                {
                    resultado.AppendLine();
                    continue;
                }

                var tokens = linhaLimpa.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int qtdAcordes = tokens.Count(t => regexAcorde.IsMatch(t));

                if (tokens.Length > 0 && (double)qtdAcordes / tokens.Length >= 0.6)
                {
                    string linhaComColchetes = regexSubstituicao.Replace(linhaLimpa, m => $"[{m.Value}]");
                    resultado.AppendLine(linhaComColchetes);
                }
                else
                {
                    resultado.AppendLine(linhaLimpa.TrimStart());
                }
            }

            return resultado.ToString().TrimEnd();
        }

        private string DetectarTom(string cifra)
        {
            var cifraSemColchetes = cifra.Replace("[", " ").Replace("]", " ");
            var regexAcordes = new Regex(@"\b([A-G](#|b)?m?)\b");
            var matches = regexAcordes.Matches(cifraSemColchetes);

            if (matches.Count == 0) return "C";

            var contagem = new Dictionary<string, int>();
            for (int i = 0; i < matches.Count; i++)
            {
                string acorde = matches[i].Value;
                int peso = (i == 0 || i == matches.Count - 1) ? 3 : 1;
                if (!contagem.ContainsKey(acorde)) contagem[acorde] = 0;
                contagem[acorde] += peso;
            }

            return contagem.OrderByDescending(x => x.Value).First().Key;
        }

        private string ExtrairApenasLetra(string cifra)
        {
            var regexAcorde = new Regex(@"^\[?[A-G](#|b)?(m|maj|min|dim|aug|sus|add)?[0-9]*(\/[A-G](#|b)?)?\]?$");
            var linhas = cifra.Split('\n');
            var letra = new StringBuilder();

            foreach (var linha in linhas)
            {
                var tokens = linha.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int qtdAcordes = tokens.Count(t => regexAcorde.IsMatch(t));

                if (tokens.Length > 0 && (double)qtdAcordes / tokens.Length < 0.5)
                {
                    string limpa = Regex.Replace(linha, @"\[.*?\]", "");
                    letra.Append(limpa.Trim() + " ");
                }
            }

            return letra.ToString();
        }
    }
}

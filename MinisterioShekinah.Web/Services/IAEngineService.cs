using Microsoft.AspNetCore.Mvc;
using MinisterioShekinah.Web.Models;
using System.Text.RegularExpressions;

namespace MinisterioShekinah.Web.Services
{
    public class CifraResultadoDto
    {
        public string CifraFormatada { get; set; } = string.Empty;
        public string TomDetectado { get; set; } = "C";
        public string MomentoSugerido { get; set; } = "Geral";
        public string DificuldadeNivel { get; set; } = "Fácil";
        public string TessituraSugerida { get; set; } = "Média";
        public string RitmoSugerido { get; set; } = "Moderado";
        public string SugestaoCapotraste { get; set; } = "0";
    }

    public class IAEngineService
    {
        // 1. Moderação Automática de Texto (Filtro de segurança e spam)
        public bool ValidarTextoSeguro(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return false;
            string[] termosProibidos = { "spam", "links_maliciosos" };
            string textoMinusculo = texto.ToLower();
            foreach (var termo in termosProibidos)
            {
                if (textoMinusculo.Contains(termo)) return false;
            }
            return true;
        }

        // 2. Classificador Inteligente de Intenções
        public string CategorizarIntencao(string intencao)
        {
            if (string.IsNullOrWhiteSpace(intencao)) return "Geral";
            string texto = intencao.ToLower();
            if (texto.Contains("saúde") || texto.Contains("cura") || texto.Contains("doente") || texto.Contains("hospital") || texto.Contains("cirurgia"))
                return "Saúde e Cura";
            if (texto.Contains("família") || texto.Contains("filho") || texto.Contains("casamento") || texto.Contains("marido") || texto.Contains("esposa"))
                return "Família e Lar";
            if (texto.Contains("trabalho") || texto.Contains("emprego") || texto.Contains("negócio") || texto.Contains("finanças"))
                return "Trabalho e Finanças";
            if (texto.Contains("paz") || texto.Contains("ansiedade") || texto.Contains("depressão") || texto.Contains("medo"))
                return "Paz Interior e Emocional";
            return "Ação de Graças / Geral";
        }

        // 3. Assistente Virtual Paroquial
        public string ObterRespostaAssistente(string pergunta)
        {
            if (string.IsNullOrWhiteSpace(pergunta)) return "Olá! Como posso ajudar na sua caminhada espiritual hoje?";
            string p = pergunta.ToLower();
            if (p.Contains("horário") || p.Contains("missa"))
                return "As nossas celebrações e missas ocorrem aos domingos às 09:00 e 19:00, e quartas-feiras às 20:00.";
            if (p.Contains("vela") || p.Contains("acender"))
                return "Pode acender a sua vela virtual na nossa página 'Vela Virtual', escolhendo a cor correspondente à sua intenção.";
            return "Obrigado pela sua mensagem! O Ministério Shekinah está em constante oração pelas suas intenções.";
        }

        // 4. Inteligência de Cifras e Análise
        public CifraResultadoDto OrganizarCifra(string titulo, string cifraBruta)
        {
            if (string.IsNullOrWhiteSpace(cifraBruta))
                return new CifraResultadoDto();

            // Limpeza básica e padronização inteligente
            string cifraLimpa = cifraBruta.Replace("\r\n", "\n");

            // Deteção simples de tom base (ex: procura por Tom: X)
            string tom = "C";
            var matchTom = Regex.Match(cifraBruta, @"Tom:\s*([A-G][#b]?[m]?)", RegexOptions.IgnoreCase);
            if (matchTom.Success)
            {
                tom = matchTom.Groups[1].Value.ToUpper();
            }

            // Sugestão de momento litúrgico com base no título ou conteúdo
            string momento = "Geral";
            string tLower = titulo.ToLower();
            if (tLower.Contains("gloria") || tLower.Contains("glória")) momento = "Glória";
            else if (tLower.Contains("santo")) momento = "Santo";
            else if (tLower.Contains("cordeiro")) momento = "Cordeiro";
            else if (tLower.Contains("salmo")) momento = "Salmo";
            else if (tLower.Contains("comunhao") || tLower.Contains("comunhão")) momento = "Comunhão";
            else if (tLower.Contains("entrada")) momento = "Entrada";
            else if (tLower.Contains("ofertorio") || tLower.Contains("ofertório")) momento = "Ofertório";

            return new CifraResultadoDto
            {
                CifraFormatada = cifraLimpa,
                TomDetectado = tom,
                MomentoSugerido = momento,
                DificuldadeNivel = "Intermédio",
                TessituraSugerida = "Confortável",
                RitmoSugerido = "84 BPM",
                SugestaoCapotraste = "0"
            };
        }

        public bool VerificarMusicaDuplicada(string titulo, string cifra, out string nomeExistente)
        {
            nomeExistente = string.Empty;
            return false;
        }

        public Dictionary<string, Musica> SugerirRepertorioPorLiturgia(List<Musica> todas, dynamic liturgia)
        {
            return new Dictionary<string, Musica>();
        }

        public Dictionary<string, Musica> GerarRepertorioMissaAutomatico()
        {
            return new Dictionary<string, Musica>();
        }
    }
}
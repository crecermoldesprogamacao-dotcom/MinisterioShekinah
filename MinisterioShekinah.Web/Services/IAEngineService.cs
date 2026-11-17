using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MinisterioShekinah.Web.Data;
using MinisterioShekinah.Web.Hubs;
using MinisterioShekinah.Web.Models;
using MinisterioShekinah.Web.Services;
using System.Runtime.ConstrainedExecution;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MinisterioShekinah.Web.Services
{
    public class IAEngineService
    {
        // 1. Moderação Automática de Texto (Filtro de segurança e spam)
        public bool ValidarTextoSeguro(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return false;

            // Lista básica de exemplo de termos impróprios ou spam a bloquear
            string[] termosProibidos = { "spam", "links_maliciosos", "palavrao1", "palavrao2" };
            
            string textoMinusculo = texto.ToLower();
            foreach (var termo in termosProibidos)
            {
                if (textoMinusculo.Contains(termo))
                {
                    return false; // Conteúdo reprovado pela moderação
                }
            }
            return true; // Conteúdo seguro
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
            
            if (texto.Contains("trabalho") || texto.Contains("emprego") || texto.Contains("negócio") || texto.Contains("finanças") || texto.Contains("dívida"))
                return "Trabalho e Finanças";
            
            if (texto.Contains("paz") || texto.Contains("ansiedade") || texto.Contains("depressão") || texto.Contains("medo") || texto.Contains("angústia"))
                return "Paz Interior e Emocional";

            return "Ação de Graças / Geral";
        }

        // 3. Assistente Virtual Paroquial (Respostas automáticas para o Chat)
        public string ObterRespostaAssistente(string pergunta)
        {
            if (string.IsNullOrWhiteSpace(pergunta)) return "Olá! Como posso ajudar na sua caminhada espiritual hoje?";

            string p = pergunta.ToLower();

            if (p.Contains("horário") || p.Contains("missa"))
                return "As nossas celebrações e missas ocorrem aos domingos às 09:00 e 19:00, e quartas-feiras às 20:00.";
            
            if (p.Contains("vela") || p.Contains("acender"))
                return "Pode acender a sua vela virtual na nossa página 'Vela Virtual', escolhendo a cor correspondente à sua intenção e deixando o seu pedido de oração.";
            
            if (p.Contains("confissão") || p.Contains("atendimento"))
                return "O atendimento e confissões realizam-se às terças e quintas-feiras, das 14:00 às 17:00, na secretaria paroquial.";

            return "Obrigado pela sua mensagem! O Ministério Shekinah está em constante oração pelas suas intenções. Deseja que a sua intenção seja encaminhada para o grupo de intercessão?";
        }
    }
}
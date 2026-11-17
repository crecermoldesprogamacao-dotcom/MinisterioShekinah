using System.Text.Json;

namespace MinisterioShekinah.Web.Services
{
    public class LiturgiaDiaDto
    {
        public string Data { get; set; } = string.Empty;
        public string Liturgia { get; set; } = "Tempo Comum";
        public string Cor { get; set; } = "Verde";
        public string PrimeiraLeitura { get; set; } = string.Empty;
        public string SalmoRefrao { get; set; } = string.Empty;
        public string SalmoTexto { get; set; } = string.Empty;
        public string Evangelho { get; set; } = string.Empty;
        public bool Sucesso { get; set; }
    }

    public class LiturgiaService
    {
        private readonly HttpClient _httpClient;

        public LiturgiaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LiturgiaDiaDto> ObterLiturgiaPorDataAsync(DateTime data)
        {
            var resultado = new LiturgiaDiaDto
            {
                Data = data.ToString("dd/MM/yyyy")
            };

            try
            {
                string url = $"https://liturgia.up.railway.app/v2/?dia={data.Day}&mes={data.Month}&ano={data.Year}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonString);
                    var root = doc.RootElement;

                    resultado.Liturgia = root.TryGetProperty("liturgia", out var lit) ? lit.GetString() ?? "Tempo Comum" : "Tempo Comum";
                    resultado.Cor = root.TryGetProperty("cor", out var cor) ? cor.GetString() ?? "Verde" : "Verde";

                    if (root.TryGetProperty("leituras", out var leituras))
                    {
                        if (leituras.TryGetProperty("primeiraLeitura", out var pl) && pl.GetArrayLength() > 0)
                            resultado.PrimeiraLeitura = pl[0].TryGetProperty("texto", out var txtPl) ? txtPl.GetString() ?? "" : "";

                        if (leituras.TryGetProperty("salmo", out var salmo) && salmo.GetArrayLength() > 0)
                        {
                            resultado.SalmoRefrao = salmo[0].TryGetProperty("refrao", out var refrao) ? refrao.GetString() ?? "" : "";
                            resultado.SalmoTexto = salmo[0].TryGetProperty("texto", out var txtSalmo) ? txtSalmo.GetString() ?? "" : "";
                        }

                        if (leituras.TryGetProperty("evangelho", out var ev) && ev.GetArrayLength() > 0)
                            resultado.Evangelho = ev[0].TryGetProperty("texto", out var txtEv) ? txtEv.GetString() ?? "" : "";
                    }

                    resultado.Sucesso = true;
                }
            }
            catch
            {
                resultado.Sucesso = false;
            }

            return resultado;
        }
    }
}
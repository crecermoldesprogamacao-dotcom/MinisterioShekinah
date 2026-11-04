namespace MinisterioShekinah.Web.Models
{
    public class Escala
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;

        // Adicione esta propriedade necessária na linha 101:
        public bool Confirmado { get; set; }
    }
}
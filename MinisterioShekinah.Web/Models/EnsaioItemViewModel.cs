namespace MinisterioShekinah.Web.Models
{
    public class EnsaioItemViewModel
    {
        public int Id { get; set; }
        public string Dificuldade { get; set; } = "Fácil";
        public string SugestaoCapo { get; set; } = "Capotraste: 0";
        public string Tessitura { get; set; } = "Média";
        public string RitmoBpm { get; set; } = "84 BPM";
        public Musica Musica { get; set; } = new Musica();
    }
}
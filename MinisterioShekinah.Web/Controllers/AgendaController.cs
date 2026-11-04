using Microsoft.AspNetCore.Mvc;
using MinisterioShekinah.Web.Data;
using MinisterioShekinah.Web.Models;
using System.Linq;

namespace MinisterioShekinah.Web.Controllers
{
    public class AgendaController : Controller
    {
        private readonly ShekinahContext _context;

        public AgendaController(ShekinahContext context)
        {
            _context = context;
        }

        // GET: Agenda
        public IActionResult Index()
        {
            // Se ainda não tiver a tabela AgendaItems criada, pode retornar a View vazia ou com uma lista simulada
            var eventos = _context.AgendaItems != null ? _context.AgendaItems.OrderBy(e => e.DataHora).ToList() : new System.Collections.Generic.List<AgendaItem>();

            return View(eventos);
        }
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinisterioShekinah.Web.Data;
using MinisterioShekinah.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MinisterioShekinah.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly ShekinahContext _context;

        public AdminController(ShekinahContext context)
        {
            _context = context;
        }

        // GET: /Admin/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Admin/Login
        [HttpPost]
        public IActionResult Login(string usuario, string senha)
        {
            if (usuario == "shekinahvalinhos" && senha == "MaRiAnA120580#")
            {
                HttpContext.Session.SetString("AdminLogado", "True");
                return RedirectToAction("Painel");
            }
            ViewBag.Erro = "Utilizador ou palavra-passe incorretos.";
            return View();
        }

        // GET: /Admin/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AdminLogado");
            return RedirectToAction("Login");
        }

        // GET: /Admin/Painel
        public IActionResult Painel()
        {
            if (HttpContext.Session.GetString("AdminLogado") != "True")
            {
                return RedirectToAction("Login");
            }
            var eventos = _context.AgendaItems != null ? _context.AgendaItems.OrderBy(e => e.DataHora).ToList() : new System.Collections.Generic.List<AgendaItem>();
            return View(eventos);
        }

        // GET: /Admin/CriarEvento
        public IActionResult CriarEvento()
        {
            if (HttpContext.Session.GetString("AdminLogado") != "True")
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        // POST: /Admin/CriarEvento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CriarEvento(AgendaItem modelo)
        {
            if (HttpContext.Session.GetString("AdminLogado") != "True")
            {
                return RedirectToAction("Login");
            }
            if (ModelState.IsValid)
            {
                _context.AgendaItems.Add(modelo);
                _context.SaveChanges();
                return RedirectToAction("Painel");
            }
            return View(modelo);
        }

        // POST: /Admin/Eliminar/5
        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            if (HttpContext.Session.GetString("AdminLogado") != "True")
            {
                return RedirectToAction("Login");
            }
            var evento = _context.AgendaItems.Find(id);
            if (evento != null)
            {
                _context.AgendaItems.Remove(evento);
                _context.SaveChanges();
            }
            return RedirectToAction("Painel");
        }

        // ==========================================
        // MÓDULO: VELAS VIRTUAIS (ÁREA RESTRITA)
        // ==========================================

        // GET: /Admin/VelasVirtuais
        public IActionResult VelasVirtuais()
        {
            if (HttpContext.Session.GetString("AdminLogado") != "True")
            {
                return RedirectToAction("Login");
            }
            var velas = _context.VelasVirtuais != null
                ? _context.VelasVirtuais.OrderByDescending(v => v.DataAcesa).ToList()
                : new System.Collections.Generic.List<VelaVirtual>();
            return View(velas);
        }

        // POST: /Admin/EliminarVela/5
        [HttpPost]
        public IActionResult EliminarVela(int id)
        {
            if (HttpContext.Session.GetString("AdminLogado") != "True")
            {
                return RedirectToAction("Login");
            }
            var vela = _context.VelasVirtuais.Find(id);
            if (vela != null)
            {
                _context.VelasVirtuais.Remove(vela);
                _context.SaveChanges();
            }
            return RedirectToAction("VelasVirtuais");
        }
    }
}
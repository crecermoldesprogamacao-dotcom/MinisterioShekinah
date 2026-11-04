using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinisterioShekinah.Web.Data;
using MinisterioShekinah.Web.Models;
using System.Linq;

namespace MinisterioShekinah.Web.Controllers
{
    public class SobreController : Controller
    {
        private readonly ShekinahContext _context;

        public SobreController(ShekinahContext context)
        {
            _context = context;
        }

        // GET: /Sobre/Index (Página Institucional)
        public IActionResult Index()
        {
            var membros = _context.MembrosEquipa != null ? _context.MembrosEquipa.ToList() : new System.Collections.Generic.List<MembroEquipe>();

            // Procura o registo da história/missão. Se não existir, cria um padrão.
            var sobre = _context.SobreMinisterios?.FirstOrDefault();
            if (sobre == null)
            {
                sobre = new SobreMinisterio
                {
                    TituloHistoria = "A Nossa História",
                    TextoHistoria = "O Ministério Shekinah nasceu com o propósito de levar a presença manifesta de Deus através da música e da adoração a Valinhos e região. Fundado com base na paixão pela evangelização e pelo serviço no altar, o ministério tem acompanhado gerações em celebrações, retiros e momentos marcantes de fé.",
                    TituloMissao = "Missão & Valores",
                    TextoMissao = "A nossa missão é conduzir os fiéis a um encontro profundo com Deus através de canções inspiradas e excelência litúrgica. Pautamos a nossa atuação pela reverência, comunhão fraterna e dedicação ao altar."
                };
            }

            ViewBag.Sobre = sobre;
            return View(membros);
        }

        // GET: /Sobre/Gerir (Painel Admin)
        public IActionResult Gerir()
        {
            if (HttpContext.Session.GetString("AdminLogado") != "True")
            {
                return RedirectToAction("Login", "Admin");
            }

            var membros = _context.MembrosEquipa != null ? _context.MembrosEquipa.ToList() : new System.Collections.Generic.List<MembroEquipe>();
            var sobre = _context.SobreMinisterios?.FirstOrDefault() ?? new SobreMinisterio
            {
                TituloHistoria = "A Nossa História",
                TextoHistoria = "O Ministério Shekinah nasceu com o propósito de levar a presença manifesta de Deus através da música e da adoração a Valinhos e região.",
                TituloMissao = "Missão & Valores",
                TextoMissao = "A nossa missão é conduzir os fiéis a um encontro profundo com Deus através de canções inspiradas e excelência litúrgica."
            };

            ViewBag.Sobre = sobre;
            return View(membros);
        }

        // GET: /Sobre/EditarTexto
        public IActionResult EditarTexto()
        {
            if (HttpContext.Session.GetString("AdminLogado") != "True")
            {
                return RedirectToAction("Login", "Admin");
            }

            var sobre = _context.SobreMinisterios?.FirstOrDefault();
            if (sobre == null)
            {
                sobre = new SobreMinisterio
                {
                    TituloHistoria = "A Nossa História",
                    TextoHistoria = "O Ministério Shekinah nasceu com o propósito de levar a presença manifesta de Deus através da música e da adoração a Valinhos e região.",
                    TituloMissao = "Missão & Valores",
                    TextoMissao = "A nossa missão é conduzir os fiéis a um encontro profundo com Deus através de canções inspiradas e excelência litúrgica."
                };
                _context.SobreMinisterios.Add(sobre);
                _context.SaveChanges();
            }

            return View(sobre);
        }

        // POST: /Sobre/EditarTexto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarTexto(SobreMinisterio modelo)
        {
            if (HttpContext.Session.GetString("AdminLogado") != "True")
            {
                return RedirectToAction("Login", "Admin");
            }

            if (ModelState.IsValid)
            {
                var existing = _context.SobreMinisterios.Find(modelo.Id);
                if (existing != null)
                {
                    existing.TituloHistoria = modelo.TituloHistoria;
                    existing.TextoHistoria = modelo.TextoHistoria;
                    existing.TituloMissao = modelo.TituloMissao;
                    existing.TextoMissao = modelo.TextoMissao;
                }
                else
                {
                    _context.SobreMinisterios.Add(modelo);
                }

                _context.SaveChanges();
                return RedirectToAction("Gerir");
            }
            return View(modelo);
        }

        // Gestão de Membros (Adicionar / Eliminar)
        public IActionResult AdicionarMembro()
        {
            if (HttpContext.Session.GetString("AdminLogado") != "True") return RedirectToAction("Login", "Admin");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdicionarMembro(MembroEquipe modelo)
        {
            if (HttpContext.Session.GetString("AdminLogado") != "True") return RedirectToAction("Login", "Admin");
            if (ModelState.IsValid)
            {
                _context.MembrosEquipa.Add(modelo);
                _context.SaveChanges();
                return RedirectToAction("Gerir");
            }
            return View(modelo);
        }

        [HttpPost]
        public IActionResult EliminarMembro(int id)
        {
            if (HttpContext.Session.GetString("AdminLogado") != "True") return RedirectToAction("Login", "Admin");
            var membro = _context.MembrosEquipa.Find(id);
            if (membro != null)
            {
                _context.MembrosEquipa.Remove(membro);
                _context.SaveChanges();
            }
            return RedirectToAction("Gerir");
        }
    }
}
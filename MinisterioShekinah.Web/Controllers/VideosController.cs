using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinisterioShekinah.Web.Data;
using MinisterioShekinah.Web.Models;
using System.Linq;

namespace MinisterioShekinah.Web.Controllers
{
    public class VideosController : Controller
    {
        private readonly ShekinahContext _context;

        public VideosController(ShekinahContext context)
        {
            _context = context;
        }

        // GET: /Videos/Index (Página pública)
        public IActionResult Index()
        {
            var videos = _context.Videos != null ? _context.Videos.OrderByDescending(v => v.DataPublicacao).ToList() : new System.Collections.Generic.List<VideoItem>();
            return View(videos);
        }

        // GET: /Videos/Gerir (Painel restrito ao Admin)
        public IActionResult Gerir()
        {
            if (HttpContext.Session.GetString("AdminLogado") != "True")
            {
                return RedirectToAction("Login", "Admin");
            }
            var videos = _context.Videos != null ? _context.Videos.OrderByDescending(v => v.DataPublicacao).ToList() : new System.Collections.Generic.List<VideoItem>();
            return View(videos);
        }

        // GET: /Videos/Adicionar
        public IActionResult Adicionar()
        {
            if (HttpContext.Session.GetString("AdminLogado") != "True")
            {
                return RedirectToAction("Login", "Admin");
            }
            return View();
        }

        // POST: /Videos/Adicionar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Adicionar(VideoItem modelo)
        {
            if (HttpContext.Session.GetString("AdminLogado") != "True")
            {
                return RedirectToAction("Login", "Admin");
            }

            if (ModelState.IsValid)
            {
                _context.Videos.Add(modelo);
                _context.SaveChanges();
                return RedirectToAction("Gerir");
            }
            return View(modelo);
        }

        // POST: /Videos/Eliminar/5
        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            if (HttpContext.Session.GetString("AdminLogado") != "True")
            {
                return RedirectToAction("Login", "Admin");
            }

            var video = _context.Videos.Find(id);
            if (video != null)
            {
                _context.Videos.Remove(video);
                _context.SaveChanges();
            }
            return RedirectToAction("Gerir");
        }
    }
}
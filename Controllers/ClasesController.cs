using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ImMusik2;

namespace ImMusik2.Controllers
{
    public class ClasesController : Controller
    {
        private immusikEntities db = new immusikEntities();

        // GET: Clases
        public ActionResult Index()
        {
            var clases = db.Clases.Include(c => c.Profesores).Include(c => c.Sucursales);
            return View(clases.ToList());
        }

        // GET: Clases/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Clases clases = db.Clases.Find(id);
            if (clases == null)
            {
                return HttpNotFound();
            }

            clases.RegistrosClases = db.RegistrosClases
            .Include(rc => rc.Alumnos)
            .Where(rc => rc.IdClase == id)
            .ToList();
            return View(clases);

        }

        // GET: Clases/Create
        public ActionResult Create()
        {
            ViewBag.IdProfesor = new SelectList(db.Profesores, "Id", "Nombre");
            ViewBag.IdSucursal = new SelectList(db.Sucursales, "Id", "Nombre");
            return View();
        }

        // POST: Clases/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,IdSucursal,Nombre,NumAlumnos,IdProfesor,Hora,Dias")] Clases clase)
        {
            if (ModelState.IsValid) {
                if (!db.Clases.Any(p => p.Id == clase.Id)) {
                    db.Clases.Add(clase);
                    db.SaveChanges();

                    return Json(new { success = true });
                }
                else {
                    return Json(new { success = false, error = "Este id ya está registrado" });
                }
            }

            return Json(new { success = false, error = "Invalid model state" });
        }

        // GET: Clases/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Clases clases = db.Clases.Find(id);
            if (clases == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdProfesor = new SelectList(db.Profesores, "Id", "Nombre", clases.IdProfesor);
            ViewBag.IdSucursal = new SelectList(db.Sucursales, "Id", "Nombre", clases.IdSucursal);
            return View(clases);
        }

        // POST: Clases/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,IdSucursal,Nombre,NumAlumnos,IdProfesor,Hora,Dias")] Clases clases)
        {
            if (ModelState.IsValid)
            {
                db.Entry(clases).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdProfesor = new SelectList(db.Profesores, "Id", "Nombre", clases.IdProfesor);
            ViewBag.IdSucursal = new SelectList(db.Sucursales, "Id", "Nombre", clases.IdSucursal);
            return View(clases);
        }

        // GET: Clases/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Clases clases = db.Clases.Find(id);
            if (clases == null)
            {
                return HttpNotFound();
            }
            return View(clases);
        }

        // POST: Clases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Clases clases = db.Clases.Find(id);
            if (clases != null) {
                var registrosClases = db.RegistrosClases.Where(r => r.IdClase == id).ToList();
                foreach (var registro in registrosClases) {
                    db.RegistrosClases.Remove(registro);
                    db.SaveChanges();
                }
                db.Clases.Remove(clases);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

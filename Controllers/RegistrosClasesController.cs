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
    public class RegistrosClasesController : Controller
    {
        private immusikEntities db = new immusikEntities();

        // GET: RegistrosClases
        public ActionResult Index()
        {
            var registrosClases = db.RegistrosClases.Include(r => r.Alumnos).Include(r => r.Clases);
            return View(registrosClases.ToList());
        }

        // GET: RegistrosClases/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RegistrosClases registrosClases = db.RegistrosClases.Find(id);
            if (registrosClases == null)
            {
                return HttpNotFound();
            }
            return View(registrosClases);
        }

        // GET: RegistrosClases/Create
        public ActionResult Create()
        {
            ViewBag.IdAlumno = new SelectList(db.Alumnos, "Id", "Nombre");
            ViewBag.IdClase = new SelectList(db.Clases, "Id", "Nombre");
            return View();
        }

        // POST: RegistrosClases/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,IdClase,IdAlumno")] RegistrosClases registrosClases)
        {
            if (ModelState.IsValid)
            {
                db.RegistrosClases.Add(registrosClases);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IdAlumno = new SelectList(db.Alumnos, "Id", "Nombre", registrosClases.IdAlumno);
            ViewBag.IdClase = new SelectList(db.Clases, "Id", "Nombre", registrosClases.IdClase);
            return View(registrosClases);
        }

        // GET: RegistrosClases/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RegistrosClases registrosClases = db.RegistrosClases.Find(id);
            if (registrosClases == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdAlumno = new SelectList(db.Alumnos, "Id", "Nombre", registrosClases.IdAlumno);
            ViewBag.IdClase = new SelectList(db.Clases, "Id", "Nombre", registrosClases.IdClase);
            return View(registrosClases);
        }

        // POST: RegistrosClases/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,IdClase,IdAlumno")] RegistrosClases registrosClases)
        {
            if (ModelState.IsValid)
            {
                db.Entry(registrosClases).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdAlumno = new SelectList(db.Alumnos, "Id", "Nombre", registrosClases.IdAlumno);
            ViewBag.IdClase = new SelectList(db.Clases, "Id", "Nombre", registrosClases.IdClase);
            return View(registrosClases);
        }

        // GET: RegistrosClases/Delete/5
        public ActionResult Delete(int? id, int idalumno)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RegistrosClases registrosClases = db.RegistrosClases.Find(id);
            if (registrosClases == null)
            {
                return HttpNotFound();
            }
            return View(registrosClases);
        }

        // POST: RegistrosClases/Delete/5
        [HttpPost]
        public ActionResult DeleteConfirmed(int id, int idalumno, string sourceController) {
            RegistrosClases registrosClases = db.RegistrosClases.FirstOrDefault(r => r.IdClase == id && r.IdAlumno == idalumno);
            if (registrosClases != null) {
                db.RegistrosClases.Remove(registrosClases);
                db.SaveChanges();
            }
            Alumnos alumno = db.Alumnos.Find(idalumno); 
            if (alumno == null) {
                return HttpNotFound();
            }
            if (!db.RegistrosClases.Any(r => r.IdAlumno == idalumno)) {
                alumno.Activo = "NO";
                db.SaveChanges();
            }

            int idview;
            if (sourceController == "Clases") {
                idview = id;
            }
            else {
                idview = idalumno;
            }

            return RedirectToAction("Details", sourceController, new { id = idview });
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

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
    public class AlumnosController : Controller
    {
        private immusikEntities db = new immusikEntities();

        // GET: Alumnos
        public ActionResult Index() {
            var alumnos = db.Alumnos.ToList();
            foreach (var al in alumnos) {
                // Check if the Alumnos record is in the updated RegistrosClases table
                if (!db.RegistrosClases.Any(r => r.IdAlumno == al.Id)) {
                    // If not, change the Activo field to 'NO'
                    al.Activo = "NO";
                    db.Entry(al).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            var items = db.Alumnos.OrderByDescending(i => i.Activo).ToList();
            return View(items);
        }

        // GET: Alumnos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Alumnos alumnos = db.Alumnos.Find(id);
            if (alumnos == null)
            {
                return HttpNotFound();
            }

            var alumno = db.Alumnos.Find(id);
            if (alumno == null) {
                return HttpNotFound();
            }

            alumno.RegistrosClases = db.RegistrosClases
                .Include(rc => rc.Clases)
                .Include(rc => rc.Clases.Sucursales) 
                .Where(rc => rc.IdAlumno == id)
                .ToList();

            return View(alumnos);
        }

        // GET: Alumnos/Create
        public ActionResult Create()
        {
            ViewBag.Cursos = new SelectList(db.Clases
            .Join(db.Sucursales, clase => clase.IdSucursal, sucursal => sucursal.Id, (clase, sucursal) => new {
                ClaseId = clase.Id,
                Nombre = clase.Nombre + " - " + sucursal.Nombre
            }).ToList(), "ClaseId", "Nombre");
            return View();
        }

        // POST: Alumnos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Nombre,ApellidoPaterno,ApellidoMaterno,Contacto,Tutor,Edad,Activo")] Alumnos alumnos, int CursoId) {
            if (ModelState.IsValid) {
                alumnos.Activo = "SI";
                if (!db.Alumnos.Any(p => p.Id == alumnos.Id)) {
                    db.Alumnos.Add(alumnos);
                    db.SaveChanges(); 
                    
                    var registroClase = new RegistrosClases {
                        IdAlumno = alumnos.Id,
                        IdClase = CursoId
                    };
                    db.RegistrosClases.Add(registroClase);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                else {
                    return Json(new { success = false, error = "Este id ya está registrado" });
                }
            }

            return Json(new { success = false, error = "Invalid model state" });
        }

        // GET: Alumnos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Alumnos alumnos = db.Alumnos.Find(id);
            if (alumnos == null)
            {
                return HttpNotFound();
            }
            return View(alumnos);
        }

        // POST: Alumnos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Nombre,ApellidoPaterno,ApellidoMaterno,Contacto,Tutor,Edad,Activo")] Alumnos alumnos)
        {
            if (ModelState.IsValid)
            {
                db.Entry(alumnos).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(alumnos);
        }

        // GET: Alumnos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Alumnos alumnos = db.Alumnos.Find(id);
            if (alumnos == null)
            {
                return HttpNotFound();
            }
            return View(alumnos);
        }

        // POST: Alumnos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Alumnos alumnos = db.Alumnos.Find(id);
            db.Alumnos.Remove(alumnos);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult AddCourse(int? Id) {
            var existingCourses = db.RegistrosClases
            .Where(rc => rc.IdAlumno == Id)
            .Select(rc => rc.IdClase)
            .ToList();

            var cursos = db.Clases
            .Where(c => !existingCourses.Contains(c.Id))
            .Select(c => new SelectListItem {
                Value = c.Id.ToString(),
                Text = c.Nombre
            })
            .ToList();

            ViewBag.Cursos = cursos;

            if (Id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Alumnos alumnos = db.Alumnos.Find(Id);
            if (alumnos == null) {
                return HttpNotFound();
            }
            return View(alumnos);
        }

        [HttpPost]
        public ActionResult AddCourse(int id, int? CursoId) {
            if (CursoId == null) {
                ModelState.AddModelError("CursoId", "Seleccione un curso");
                return View();  
            }
            var registroClase = new RegistrosClases {
                IdAlumno = id,
                IdClase = (int)CursoId
            };

            db.RegistrosClases.Add(registroClase);
            db.SaveChanges();

            Alumnos al = db.Alumnos.Find(id);
            if (al != null) {
                al.Activo = "SI";
                db.Entry(al).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult RealizarPago(int id) {
            var pagos = new Pagos {
                IdAlumno = id
            };
            return View(pagos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RealizarPago([Bind(Include = "Cantidad,Metodo,IdAlumno,Concepto,Fecha")] Pagos pagos) {
            if(ModelState.IsValid) {
                db.Pagos.Add(pagos);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(pagos);
        }

        public ActionResult Historial(int id) {
            var pagos = db.Pagos.Where(p => p.IdAlumno == id).ToList();
            return View(pagos);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public JsonResult CheckId(int id) {
            bool exist = db.Alumnos.Any(p => p.Id == id);
            return Json(new { exists = exist }, JsonRequestBehavior.AllowGet);
        }
    }
}

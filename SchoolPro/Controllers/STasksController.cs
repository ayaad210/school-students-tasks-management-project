using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SchoolPro.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace SchoolPro.Controllers
{
    [Authorize(Roles = "Teachers,Students")]

    public class STasksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: STasks
        public ActionResult Index(int id)
        {
            List<STask> tasks;
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            ApplicationUser user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
            if (userManager.IsInRole(user.Id, "Teachers"))//مدرس
            {
                Teacher t = db.Teachers.Where(tt => tt.user.Id == user.Id).FirstOrDefault();

                
                tasks = db.Tasks.Include(s => s.Group).Include(s => s.Teacher).Include(s=>s.Teacher.user).Include(s=>s.Teacher.Subject).Where(tt => tt.Groupid == id && tt.Teacherid == t.id).ToList();
                ViewBag.who = "T";
            }
            else 
            {
                ViewBag.who = "S";

                Student s = db.Students.Include(ss => ss.Group.STasks.Select(st=>st.Teacher.user)).Where(ss => ss.user.Id == user.Id).FirstOrDefault();
                tasks = s.Group.STasks.ToList();
            }

            //def filter


            return View(tasks.ToList());

        }

        // GET: STasks/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            STask sTask = db.Tasks.Find(id);
            if (sTask == null)
            {
                return HttpNotFound();
            }
            return View(sTask);
        }

        // GET: STasks/Create
        [Authorize(Roles = "Teachers")]

        public ActionResult Create()
        {
            ViewBag.Groupid = new SelectList(db.Groups, "id", "Name");
            //     ViewBag.Teacherid = new SelectList(db.Teachers, "id", "id");
            return View();
        }

        // POST: STasks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Teachers")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,Name,Content,Notes,Type,Total,Groupid,Teacherid")] STask sTask)
        {
            ApplicationUser user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());

            Teacher t = db.Teachers.Where(tt => tt.user.Id == user.Id).FirstOrDefault();

            if (ModelState.IsValid && t != null)
            {
                if (user != null)
                {
                    sTask.Teacherid = t.id;
                }
                db.Tasks.Add(sTask);
                db.SaveChanges();
                return RedirectToAction("index",new { id=sTask.Groupid});
            }
            ViewBag.Groupid = new SelectList(db.Groups, "id", "Name", sTask.Groupid);
            ViewBag.Teacherid = new SelectList(db.Teachers, "id", "id", sTask.Teacherid);
            return View(sTask);
        }

        // GET: STasks/Edit/5
        [Authorize(Roles = "Teachers")]

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            STask sTask = db.Tasks.Find(id);
            if (sTask == null)
            {
                return HttpNotFound();
            }
            ViewBag.Groupid = new SelectList(db.Groups, "id", "Name", sTask.Groupid);
            return View(sTask);
        }

        // POST: STasks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Teachers")]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,Name,Content,Notes,Type,Total,Groupid,Teacherid")] STask sTask)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sTask).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("index", new { id = sTask.Groupid });
            }
            ViewBag.Groupid = new SelectList(db.Groups, "id", "Name", sTask.Groupid);
            ViewBag.Teacherid = new SelectList(db.Teachers, "id", "id", sTask.Teacherid);
            return View(sTask);
        }

        // GET: STasks/Delete/5
        [Authorize(Roles = "Teachers")]

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            STask sTask = db.Tasks.Find(id);
            if (sTask == null)
            {
                return HttpNotFound();
            }
            return View(sTask);
        }

        // POST: STasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teachers")]

        public ActionResult DeleteConfirmed(int id)
        {
            STask sTask = db.Tasks.Find(id);
            db.Tasks.Remove(sTask);
            db.SaveChanges();
            return RedirectToAction("index", new { id = sTask.Groupid });
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

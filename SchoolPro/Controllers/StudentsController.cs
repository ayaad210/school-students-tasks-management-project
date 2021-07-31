using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SchoolPro.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace SchoolPro.Controllers
{
    [Authorize]

    public class StudentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Students
        [Authorize(Roles = "Admins")]

        public ActionResult Index()
        {
            var m = db.Students.Include(s => s.user).ToList();
            ViewBag.who = "A";

            return View(m);
        }
        [Authorize(Roles = "Teachers,Admins")]

        public ActionResult GroupStudents(int id)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            ApplicationUser user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
            if (userManager.IsInRole(user.Id, "Teachers"))//مدرس
            {
                ViewBag.who = "T";

            }
            else
            {
                ViewBag.who = "A";

            }
            var m = db.Students.Include(s => s.user).Where(ss => ss.Group.id == id).ToList();

            return View("index", m);
        }

        [Authorize(Roles = "Perants")]
        public ActionResult PerantStudents(int id)
        {
            ViewBag.who = "P";

            var m = db.Students.Include(s => s.user).Where(ss => ss.Perant.Id == id).ToList();

            return View("index", m);
        }
        public ActionResult Studentprogress(int id)
        {
            Student s = db.Students.Include(ss => ss.Group.STasks.Select(t => t.Teacher.user)).Where(ss => ss.id == id).FirstOrDefault();
        List<STask>   tasks=s.Group.STasks.ToList();

            List<StudentProgressModel> prolist = new List<Models.StudentProgressModel>();
            foreach (var item in tasks)
            {
                StudentProgressModel pro= new StudentProgressModel();
                Answer ans = item.Answers.Where(a => a.Studentid == id).FirstOrDefault();
                pro.Task = item;
                pro.Ans = ans;
                prolist.Add(pro);
            }
            ViewBag.studentid = id;
        return View(prolist);
        }


        // GET: Students/Details/5
        [Authorize(Roles = "Teachers,Admins")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = db.Students.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        // GET: Students/Create
        [Authorize(Roles = "Admins")]

        public ActionResult Create()
        {

            ViewBag.groubs = new SelectList(db.Groups.ToList(), "id", "Name");
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admins")]

        [ValidateAntiForgeryToken]
        public ActionResult Create(StudentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser {Name=model.Name,Age=model.Age , UserName = model.Email, Email = model.Email };

                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                var check = userManager.Create(user,model.Password);
                db.SaveChanges();
                if (check.Succeeded)
                {
                     userManager.AddToRoles(user.Id, "Students");
                    Student student = new Student();
                    student.Points = 0;
                    student.user = user;
                   Group  g= db.Groups.Find(model.Grpupid);
                    student.Group = g;
                    db.Students.Add(student);
                    db.SaveChanges();
                    return RedirectToAction("GroupStudents",new { id=model.Grpupid });

                }

                
            }

            return View(model);
        }
        [Authorize(Roles = "Admins")]

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = db.Students.Include(ss => ss.user).Include(ss=>ss.Group).Where(ss => ss.id == id).FirstOrDefault();
            // List<SemestersSubject>  subs=  student.Group.Semester.s


            if (student == null)
            {
                return HttpNotFound();
            }
            ApplicationUser user = db.Users.Find(student.user.Id);
            StudentViewModel model = new StudentViewModel();
            model.Points=student.Points;
            model.Name = user.Name;
            model.id = student.id;
            model.userid = user.Id;
            model.Password = user.PasswordHash;
            model.ConfirmPassword = user.PasswordHash;
            model.Email = user.Email;
            model.Age= user.Age;

            model.Grpupid = student.Group.id;
            ViewBag.Grpupid = new SelectList(db.Groups.ToList(), "id", "Name", student.Group.id);
            return View(model);
        }

        [Authorize(Roles = "Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StudentViewModel model)
        {
            if (ModelState.IsValid)
            {

                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

                ApplicationUser user = db.Users.Find(model.userid);
                user.Name = model.Name;
                user.Age = model.Age;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();

                Student st = new Student();
                st.id = model.id;
                   st.user = user;
                st.Points = model.Points;
                st.Group = db.Groups.Find(model.Grpupid);

                db.Entry(st).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("GroupStudents", new { id = model.Grpupid });
            }
            return View(model);
        }

        // GET: Students/Delete/5
        [Authorize(Roles = "Admins")]

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = db.Students.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admins")]

        public ActionResult DeleteConfirmed(int id)
        {
            Student student = db.Students.Include(ss => ss.user).Include(ss => ss.Group).FirstOrDefault();
            ApplicationUser user = student.user;
                int gid = student.Group.id;

                db.Students.Remove(student);
                db.SaveChanges();

            user.Roles.Clear();
            db.Users.Remove(user);
            db.SaveChanges();

            return RedirectToAction("GroupStudents", new { id = gid });
        }
        //public PartialViewResult TotalProgress(int id)//student id
        //{
        //    Student s = db.Students.Include(ss => ss.Group.STasks.Select(t => t.Teacher.user)).Where(ss => ss.id == id).FirstOrDefault();
        //    List<STask> tasks = s.Group.STasks.ToList();
        //    decimal DegreeSum = 0;
        //    decimal totalSum = 0;
        //    foreach (var item in tasks)
        //    {
        //        Answer ans = item.Answers.Where(a => a.Studentid == id).FirstOrDefault();
        //        if (ans==null)
        //        {
        //            totalSum += item.Total;

        //        }
        //        else if(ans.Degree==null)
        //        {


        //        }
        //        else
        //        {
        //            totalSum += item.Total;
        //            DegreeSum +=(decimal) ans.Degree;

        //        }


        //    }

        //    decimal? precent = (DegreeSum / totalSum) * 100;

        //    return PartialView("TotalProgress",precent);
        //}
        
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

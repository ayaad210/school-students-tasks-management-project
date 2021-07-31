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

    public class AnswersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Answers
        [Authorize(Roles = "Teachers")]
        public ActionResult Index()
        {
            var answers = db.Answers.Include(a => a.STask).Include(a => a.Student);
            return View(answers.ToList());
        }
    //    [Authorize(Roles ="Teachers")]
      //  [Authorize( Roles = "Students")]

        public ActionResult TaskAnwsers(int id)
        {
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                ApplicationUser user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());

            var task = db.Tasks.Include(tt => tt.Answers.Select(a => a.Student.user)).Where(tt => tt.id == id).FirstOrDefault(); ;
            if (task != null)
            {


                if ( userManager.IsInRole(user.Id,"Teachers"))//مدرس
                {
                    List< Answer> model = task.Answers.ToList();
                    return View("index",model);
                }
                else//طالب
                {
                    Student s = db.Students.Where(ss => ss.user.Id == user.Id).FirstOrDefault();

                    Answer ans =   task.Answers.Where(a => a.Studentid == s.id).FirstOrDefault();
                    if (ans!=null)
                    {
                        int and = ans.id;
                        return RedirectToAction("Edit", new { id = and });

                    }
                    else
                    {
                        return RedirectToAction("Create", new { id = id });

                    }
                }
                
            
            }
            return View("index");
        }

        // GET: Answers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Answer answer = db.Answers.Include(aa => aa.Student.user).Where(aa=>aa.id==id).FirstOrDefault();
            if (answer == null)
            {
                return HttpNotFound();
            }
            return View(answer);
        }

        // GET: Answers/Create
        public ActionResult Create(int id)
        {
            ApplicationUser user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
            Student s=     db.Students.Where(ss => ss.user.Id == user.Id).FirstOrDefault();
            ViewBag.STaskid = new SelectList(db.Tasks, "id", "Name",id);
            ViewBag.Studentid = new SelectList(db.Students.Include(ss => ss.user).ToList(), "id", "user.Name", s.id);
            return View();
        }

        // POST: Answers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,AnswerContent,Degree,Datetime,Studentid,STaskid")] Answer answer)
        {
            
            if (ModelState.IsValid)
            {
                answer.Datetime = DateTime.Now;
                db.Answers.Add(answer);
                db.SaveChanges();
               STask  task= db.Tasks.Where(t => t.id == answer.STaskid).FirstOrDefault();
                return RedirectToAction("Index","stasks",new { id=task.Groupid});
            }

            ViewBag.STaskid = new SelectList(db.Tasks, "id", "Name", answer.STaskid);
            ViewBag.Studentid = new SelectList(db.Students.Include(ss => ss.user).ToList(), "id", "user.Name", answer.Studentid);
            return View(answer);
        }

        // GET: Answers/Edit/5
        public ActionResult Edit(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            ApplicationUser user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
            if (userManager.IsInRole(user.Id, "Teachers"))//مدرس
            {
                ViewBag.who = "T";
            }
            else//طالب
            {
                ViewBag.who = "s";

            }
            Answer answer = db.Answers.Find(id);
            if (answer == null)
            {
                return HttpNotFound();
            }

            ViewBag.STaskid = new SelectList(db.Tasks, "id", "Name", answer.STaskid);
            ViewBag.Studentid = new SelectList(db.Students.Include(ss=>ss.user).ToList(), "id", "user.Name", answer.Studentid);
            return View(answer);
        }

        // POST: Answers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,AnswerContent,Degree,Datetime,Studentid,STaskid")] Answer answer)
        {
          
            if (ModelState.IsValid)
            {
                
                answer.Datetime = DateTime.Now;
                db.Entry(answer).State = EntityState.Modified;
                db.SaveChanges();
                STask task = db.Tasks.Where(t => t.id == answer.STaskid).FirstOrDefault();

                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                ApplicationUser user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
                if (userManager.IsInRole(user.Id, "Teachers"))//مدرس
                {
                        Student st = db.Students.Find(answer.Studentid);
            
                    if (answer.Degree!=null&&(answer.Degree/ db.Tasks.Find(answer.STaskid).Total)*100>80)
                    {
                        st.Points += 12;
                    }
                   else if (answer.Degree != null && (answer.Degree / db.Tasks.Find(answer.STaskid).Total) * 100 > 65)
                    {
                        st.Points += 6;
                    }
                    else if (answer.Degree != null && (answer.Degree / db.Tasks.Find(answer.STaskid).Total) * 100 > 50)
                    {
                        st.Points += 3;
                    }

                    db.Entry(st).State = EntityState.Modified;
                        db.SaveChanges();
                    

                    return RedirectToAction("TaskAnwsers", "answers", new { id = task.id });

                }
                return RedirectToAction("Index", "stasks", new { id = task.Groupid });

            }
            ViewBag.STaskid = new SelectList(db.Tasks, "id", "Name", answer.STaskid);
            ViewBag.Studentid = new SelectList(db.Students.Include(ss => ss.user).ToList(), "id", "user.Name", answer.Studentid);

            return View(answer);
        }

        // GET: Answers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Answer answer = db.Answers.Find(id);
            if (answer == null)
            {
                return HttpNotFound();
            }
            return View(answer);
        }

        // POST: Answers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Answer answer = db.Answers.Find(id);
            db.Answers.Remove(answer);
            db.SaveChanges();
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

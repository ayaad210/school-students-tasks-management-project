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
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace SchoolPro.Controllers
{
    [Authorize(Roles = "Admins")]
    public class PerantsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Perants
        public ActionResult Index()
        {
            return View(db.Perants.Include(path=>path.User).ToList());
        }

        // GET: Perants/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Perant perant = db.Perants.Find(id);
            if (perant == null)
            {
                return HttpNotFound();
            }
            return View(perant);
        }

        // GET: Perants/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Perants/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( PerantModel model)
        {
            if (ModelState.IsValid)
            {

                var user = new ApplicationUser { Name = model.Name, UserName = model.Email, Email = model.Email };

                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                user.Age = 0;
                var check = userManager.Create(user, model.Password);
                db.SaveChanges();
                if (check.Succeeded)
                {
                    userManager.AddToRoles(user.Id, "perants");
                    Perant perant = new Perant();
                    perant.User = user;

                    db.Perants.Add(perant);
                    db.SaveChanges();
                    return RedirectToAction("Index");

                }

              
            }

            return View(model);
        }

        // GET: Perants/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Perant perant = db.Perants.Include(p => p.User).Where(p => p.Id == id).FirstOrDefault(); ;
            if (perant == null)
            {

                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(perant.User.Id);
            PerantModel model = new PerantModel();
            model.Name = user.Name;
            model.id = perant.Id;
            model.userid = user.Id;
            model.Password = user.PasswordHash;
            model.ConfirmPassword = user.PasswordHash;
            model.Email = user.Email;
           
            
            return View(model);
        }

        // POST: Perants/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PerantModel model)
        {
            if (ModelState.IsValid)
            {
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

                ApplicationUser user = db.Users.Find(model.userid);
                user.Name = model.Name;


                userManager.Update(user);

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: Perants/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Perant perant = db.Perants.Find(id);
            if (perant == null)
            {
                return HttpNotFound();
            }
            return View(perant);
        }


        // POST: Perants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Perant perant = db.Perants.Include(ss => ss.User).Where(t => t.Id == id).FirstOrDefault();

            if (perant.Children.Count<=0)
            {
                ApplicationUser user = perant.User;
                db.Perants.Remove(perant);
                db.SaveChanges();

                user.Roles.Clear();
                db.Users.Remove(user);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public ActionResult PerantChildren(int id)
        {
            ViewBag.Groups = new SelectList(db.Groups, "id", "Name").ToList(); ;

            ViewBag.Students = new SelectList(db.Students.Include(ss => ss.user), "id","user.Name").ToList(); ;
            Perant per = db.Perants.Include(p => p.Children).Where(p => p.Id== id).FirstOrDefault();
            return View(per.Children.ToList());
        }
        public string InsertStudentToPerant(string perantid, string Studentid)
        {

            try
            {
               Perant per= db.Perants.Include(p=>p.Children).Where( p=>p.Id.ToString()==perantid).FirstOrDefault();
                Student st = db.Students.Include(p => p.user).Where(p => p.id.ToString() == Studentid).FirstOrDefault();

                per.Children.Add(st);
                db.SaveChanges();

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(" <input type='button' class='btndelete'  id='btndelete'  value='-' />");
                sb.AppendLine("<input type = 'hidden' value='" + st.id + "' />");
                string x = "<tr> <td> " + st.user.Name + "</td> <td>" + sb + "</td> </tr>";
                return x;

                //ViewBag.Groups = new SelectList(db.Groups, "id", "Name").ToList(); ;

                //ViewBag.Students = new SelectList(db.Students.Include(ss => ss.user), "id", "user.Name").ToList(); ;
                //Perant perR = db.Perants.Include(p => p.Children).Where(p => p.Id.ToString() == perantid).FirstOrDefault();
                //return View("PerantChildren", per.Children.ToList());
            }
            catch (Exception)
            {

                return "-1";//RedirectToAction("PerantChildren", new { id = perantid });
            }
        }

        public string DeleteStudentFromPerant(string perantid, string Studentid)
        {
            try
            {
                Perant per = db.Perants.Include(p => p.Children).Where(p => p.Id.ToString() == perantid).FirstOrDefault();
                Student st = db.Students.Include(p => p.user).Where(p => p.id.ToString() == Studentid).FirstOrDefault();

                per.Children.Remove(st);
                db.SaveChanges();
                return "1";
            }
            catch (Exception)
            {

                return "-1";

            }

        }

        public ContentResult GetGroubstudents(string groupid)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                int groupid_ = Convert.ToInt32(groupid);
                var group = db.Groups.Include(gg => gg.Students).Include(gg => gg.Students.Select(ss => ss.user)).Where(e => e.id == groupid_).FirstOrDefault();

                //  return Json(new {id= });

                var list = JsonConvert.SerializeObject(group.Students.ToList(),
                 Formatting.None,
                 new JsonSerializerSettings()
                 {
                     ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                 });
               
                return Content(list, "application/json");
            }
            catch (FormatException)
            {
                this.Dispose();
                return null;
            }
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

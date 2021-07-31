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
    [Authorize(Roles = "Students,Teachers,Admins")]
    
    public class BooksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Books

        public ActionResult Index()
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            ApplicationUser user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());

            if (userManager.IsInRole(user.Id, "Admins"))
            {
                return View(db.Books.Include(b => b.User).ToList());
            }

            return View(db.Books.Include(b => b.User).Where(b=>b.User.Id==user.Id).ToList());

        }
        public ActionResult ShareIndex()
        {
            return View(db.Books.Include(b => b.User).ToList());
        }
        public ActionResult SearchByBookName(string BookName)
        {
            return View("ShareIndex",db.Books.Include(b => b.User).Where(b=>b.BookName.Contains(BookName)).ToList());
        }

        // GET: Books/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Include(bb=>bb.User).Where(aa=>aa.Id==id) .FirstOrDefault();
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // GET: Books/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BookViewModel model)
        {
            if (ModelState.IsValid)
            {

                if (model.files.ContentType == "application/pdf"||model.files.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                {
                    var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                    ApplicationUser user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());



                    byte[] Filedata = new byte[model.files.ContentLength];
                    model.files.InputStream.Read(Filedata, 0, model.files.ContentLength);

                    Book book = new Book();
                    book.BookName = model.BookName;
                    book.File = Filedata;
                    book.Description = model.Description;
                    book.Points = model.Points;
                    book.uploadDate = DateTime.Now;



                    if (userManager.IsInRole(user.Id, "students"))
                    {
                        Student st = db.Students.Where(s => s.user.Id == user.Id).FirstOrDefault();
                        if (st != null)
                        {
                            st.Points += 2;
                            db.Entry(st).State = EntityState.Modified;

                        }
                    }
                    book.User = db.Users.Find(user.Id);
                    db.Books.Add(book);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
            }

                return View(model);
            
        }

        // GET: Books/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            BookViewModel model = new BookViewModel { Id = book.Id, BookName = book.BookName, Description = book.Description, Points = book.Points };

            return View(model);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(BookViewModel model)
        {
            if (ModelState.IsValid)
            {
                Book book=  db.Books.Include(b=>b.User).Where(b => b.Id == model.Id).FirstOrDefault();

                book.BookName = model.BookName;
                book.Points = model.Points;
                book.Description = model.Description;


                db.Entry(book).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: Books/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Book book = db.Books.Find(id);
            db.Books.Remove(book);

            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            ApplicationUser user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());

            if (userManager.IsInRole(user.Id, "students"))
            {
                Student st = db.Students.Where(s => s.user.Id == user.Id).FirstOrDefault();
                if (st != null)
                {
                    st.Points -= 2;
                    db.Entry(st).State = EntityState.Modified;

                }
            }
            db.SaveChanges();

            return RedirectToAction("Index");
        }
        public ActionResult DownLoadFile(int id)
        {

            var FileById = (from FC in db.Books
                            where FC.Id.Equals(id)
                            select new { FC.BookName, FC.File ,FC.Points}).ToList().FirstOrDefault();

            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            ApplicationUser user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());

            if (userManager.IsInRole(user.Id, "students"))
            {
                Student st = db.Students.Where(s => s.user.Id == user.Id).FirstOrDefault();
                if (st != null&&st.Points>=FileById.Points)
                {
                    st.Points -= FileById.Points;
                    db.Entry(st).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {;
                    object x = "You need more " + (FileById.Points - st.Points) + " Points  To Upload this Book";
                    return View("ExceedPriciial",x);
                }
                
            }

            return File(FileById.File, "application/pdf", FileById.BookName);

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

using LibrarySystemV1._1.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Net.Mail;
using System.Net;

namespace LibrarySystemV1._1.Controllers
{
    public class BasicAdminController : Controller
    {
        // GET: Admin
        ApplicationDbContext db;
        IUnitOfWork unitOfWork;
        public BasicAdminController(IUnitOfWork iUnitOfWorkCons)
        {
            unitOfWork = iUnitOfWorkCons;
            db = unitOfWork.DbContext;
        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["userid"] != null)
            {
                string userid = Session["userid"].ToString();
                var user = db.Users.FirstOrDefault(e => e.Id == userid);
                if (user.Photo == null)
                {
                    filterContext.Result = new RedirectResult("/Account/FirstLogin");
                }
            }
        }

        [Authorize(Roles = "BasicAdmin")]
        public ActionResult ViewProfile()
        {
            var id = User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Where(x => x.Id == id).FirstOrDefault();
            return View(currentUser);
        }
        [Authorize(Roles = "BasicAdmin")]

        [HttpGet]
        public ActionResult UpdateMyProfile()
        {
            string id= User.Identity.GetUserId();
            BasicAdmin BasicAdmin = db.BasicAdmins.Include(e => e.ApplicationUser).Where(x => x.Id == id).FirstOrDefault();
            if (BasicAdmin != null)
            {
                return View(BasicAdmin);
            }
            else
            {
                return HttpNotFound();
            }
        }
        [Authorize(Roles = "BasicAdmin")]

        [HttpPost]
        public ActionResult UpdateMyProfile(BasicAdmin bAdmin, HttpPostedFileBase img)
        {

            var oldUser = db.Users.Where(x => x.Id == bAdmin.Id).FirstOrDefault();
            string ext;
            string imgName;
            if (ModelState.IsValid)
            {
                if (oldUser != null)
                {
                    oldUser.Fname = bAdmin.ApplicationUser.Fname;
                    oldUser.Lname = bAdmin.ApplicationUser.Lname;
                    oldUser.BDate = bAdmin.ApplicationUser.BDate;
                    oldUser.Address = bAdmin.ApplicationUser.Address;
                    if (img != null)
                    {
                        ext = img.FileName.Substring(img.FileName.LastIndexOf("."));
                        imgName = bAdmin.Id + ext;
                        img.SaveAs(Server.MapPath("/Images/") + imgName);
                        oldUser.Photo = "/Images/" + imgName;
                        db.SaveChanges();
                        return RedirectToAction(nameof(ViewProfile));
                    }
                    else
                    {
                        db.SaveChanges();
                        return RedirectToAction(nameof(ViewProfile));
                    }
                }

                else
                {
                    return HttpNotFound();
                }
            }
            return View(bAdmin);
        }
        [Authorize(Roles = "BasicAdmin")]

        public ActionResult DashBoard()
        {
            ViewBag.totalMembers = db.Members.Count();
            ViewBag.newMembers = (db.Members.Where(s => s.RegisterationDay == DateTime.Today).Count()) * 10;
            ViewBag.totalEmps = db.Employees.Count();
            ViewBag.newEmps = (db.Employees.Where(s => s.HireDate == DateTime.Today).Count()) * 10;
            ViewBag.books = db.Books.AsEnumerable();
            return View();
        }


    }
}

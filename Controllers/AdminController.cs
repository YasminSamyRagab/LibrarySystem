using LibrarySystemV1._1.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace LibrarySystemV1._1.Controllers
{
    
    public class AdminController : Controller
    {
        ApplicationDbContext db;
        IUnitOfWork unitOfWork;
        public AdminController(IUnitOfWork iUnitOfWorkCons)
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
        string subject = "Deletion from website";
        string body = "We're sorry to inform you that we deleted you from our website";


        [Authorize(Roles = "Admin")]
        // GET: Admin
        public ActionResult ViewProfile()
        {
            if (Session["userid"] != null)
            {
                string userid = Session["userid"].ToString();
                Admin ad = db.Admins.Include(e => e.ApplicationUser).FirstOrDefault(e => e.Id == userid);
               
                if (ad != null)
                {
                    ViewBag.Title = "View Profile";
                    return View(ad);
                }
                else
                    return HttpNotFound();
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult ViewProfile(string id)
        {
            string userid = Session["userid"].ToString();
            Admin ad = db.Admins.Include(e => e.ApplicationUser).FirstOrDefault(e => e.Id == userid);
            if (ad != null)
            {
                ad.ChangedByBasicAdmin = false;
                db.SaveChanges();
                return View(ad);
            }
            else
                return HttpNotFound();
        }
        [Authorize(Roles = "Admin")]
        public ActionResult EditProfile()
        {
            ViewBag.Title = "Edit Profile";
            string userid = Session["userid"].ToString();
            Admin ad = db.Admins.Include(e => e.ApplicationUser).FirstOrDefault(e => e.Id == userid);
            if (ad != null)
            { return PartialView(ad); }
            else
                return HttpNotFound();

        }
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditProfile(Admin newad, HttpPostedFileBase img)
        {
            string imgName;
            string ext;
            if (ModelState.IsValid)
            {
                Admin oldad = db.Admins.Include(a => a.ApplicationUser).FirstOrDefault(a => a.Id == newad.Id);
                if (oldad != null)
                {
                    oldad.ApplicationUser.Fname = newad.ApplicationUser.Fname;
                    oldad.ApplicationUser.Lname = newad.ApplicationUser.Lname;
                    oldad.ApplicationUser.BDate = newad.ApplicationUser.BDate;
                    oldad.ApplicationUser.Address = newad.ApplicationUser.Address;
                    oldad.ApplicationUser.Email = newad.ApplicationUser.Email;
                    oldad.ApplicationUser.PhoneNumber = newad.ApplicationUser.PhoneNumber;
                    oldad.ApplicationUser.UserName = newad.ApplicationUser.Email;
                    if (img != null)
                    {
                        ext = img.FileName.Substring(img.FileName.LastIndexOf("."));
                        imgName = newad.Id + ext;
                        img.SaveAs(Server.MapPath("/Images/") + imgName);
                        oldad.ApplicationUser.Photo = "/Images/" + imgName;
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
            return PartialView(newad);
        }


        [Authorize(Roles = "BasicAdmin")]
        public ActionResult AdminForm()
        {
            return View();
        }



        [Authorize(Roles = "BasicAdmin")]
        public ActionResult AdminFormpartial()
        {
            string searchInput = Request.QueryString["name"];
            if (searchInput != null && searchInput != "")
            {
                if ((searchInput.Split(' ')).Length == 2)
                {
                    string firstName = searchInput.Split(' ')[0];
                    string lastName = searchInput.Split(' ')[1];
                    List<Admin> adsSearch = db.Admins.Include(e => e.ApplicationUser).Where(b => b.ApplicationUser.Fname == firstName && b.ApplicationUser.Lname == lastName).ToList();
                    if (adsSearch != null)
                    {
                        return View(adsSearch);
                    }
                }
            }

            List<Admin> ads = db.Admins.Include(e => e.ApplicationUser).ToList();
            return PartialView(ads);
        }
        public JsonResult GetSearchValue(string search)
        {
            List<Admin> allsearch = db.Admins.Include(u => u.ApplicationUser).Where(x => x.ApplicationUser.Fname.Contains(search) || x.ApplicationUser.Lname.Contains(search)).ToList();
            return new JsonResult { Data = allsearch, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        [Authorize(Roles = "BasicAdmin")]

        public ActionResult AddAdmin()
        {
            return PartialView(new adData { });
        }
        [Authorize(Roles = "BasicAdmin")]

        [HttpPost]
        public async Task<ActionResult> AddAdmin(adData newad)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {

                    UserName = newad.rad.Email,
                    Email = newad.rad.Email,
                    Fname = newad.rad.Fname,
                    Lname = newad.rad.Lname,
                    BDate = newad.rad.BDate,
                    Address = newad.rad.Address,
                    PhoneNumber = newad.rad.PhoneNumber,
                    EmailConfirmed = true
                };
                UserManager<ApplicationUser> us = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                var res = await us.CreateAsync(user, newad.rad.Password);
                if (res.Succeeded)
                {
                    db.Admins.Add(new Admin() { Id = user.Id, Salary = newad.ad.Salary, HireDate = newad.ad.HireDate });
                    db.SaveChanges();
                    await us.AddToRoleAsync(user.Id, "Admin");
                    return PartialView("AdminRowpartial", newad);
                }
                else
                {
                    ModelState.AddModelError("", "invalid password");
                    return PartialView(newad);
                }
            }
            return PartialView(newad);
        }
        [Authorize(Roles = "BasicAdmin")]

        public ActionResult EditAdmin(string id)
        {
            Admin ad = db.Admins.Include(e => e.ApplicationUser).First(e => e.Id == id);
            return PartialView(ad);
        }
        [Authorize(Roles = "BasicAdmin")]

        [HttpPost]
        public ActionResult EditAdmin(Admin newad)
        {
            if (ModelState.IsValid)
            {
                Admin oldad = db.Admins.Include(a => a.ApplicationUser).First(a => a.Id == newad.Id);
                oldad.ApplicationUser.Fname = newad.ApplicationUser.Fname;
                oldad.ApplicationUser.Lname = newad.ApplicationUser.Lname;
                oldad.ApplicationUser.BDate = newad.ApplicationUser.BDate;
                oldad.ApplicationUser.Address = newad.ApplicationUser.Address;
                oldad.ApplicationUser.Email = newad.ApplicationUser.Email;
                oldad.ApplicationUser.PhoneNumber = newad.ApplicationUser.PhoneNumber;
                oldad.ApplicationUser.UserName = newad.ApplicationUser.UserName;
                oldad.HireDate = newad.HireDate;
                oldad.Salary = newad.Salary;
                oldad.ChangedByBasicAdmin = true;
                db.SaveChanges();

                return RedirectToAction(nameof(AdminForm));

            }
            return View(newad);
        }
        [Authorize(Roles = "BasicAdmin")]

        public ActionResult DeleteAdmin(string id)
        {

            Admin ad = db.Admins.FirstOrDefault(e => e.Id == id);
            ApplicationUser user = db.Users.First(u => u.Id == id);
            string mail;
            bool response;
            if (ad != null)
            {

                mail = user.Email;
                response = SendEmail(mail, subject, body);
                if (response)
                {
                    db.Admins.Remove(ad);
                    var roleuser = user.Roles.First(u => u.UserId == id);
                    user.Roles.Remove(roleuser);
                    db.Users.Remove(user);
                    db.SaveChanges();
                    return View();
                }
                else
                {
                    return Content("error");
                }
            }
            else
            {
                return HttpNotFound("no Admin with this id exist");
            }
        }

        public bool SendEmail(string receiver, string subject, string message)
        {
            try
            {
                var senderEmail = new MailAddress("sarahmosbbb@gmail.com", "Sarah");
                var receiverEmail = new MailAddress(receiver, "Receiver");
                var password = "01144096559";
                var sub = subject;
                var body = message;
                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(senderEmail.Address, password)
                };
                using (var mess = new MailMessage(senderEmail, receiverEmail)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(mess);
                }
                return true;
            }

            catch (Exception)
            {
                return false;
            }

        }

        //[ValidateAntiForgeryToken]
        //// GET: /Manage/ChangePassword
        //public ActionResult ChangePassword()
        //{
        //    return PartialView();
        //}

        ////
        //// POST: /Manage/ChangePassword
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public  ActionResult ChangePassword(ChangePasswordViewModel model)
        //{
        //    return RedirectToAction("ChangePassword", "Manage");
        //}

    }
}
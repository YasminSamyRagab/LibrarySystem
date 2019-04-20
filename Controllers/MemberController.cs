using LibrarySystemV1._1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Net.Mail;
using System.Net;

namespace LibrarySystemV1._1.Controllers
{
    [Authorize(Roles = "Member,BasicAdmin")]
    public class MemberController : Controller
    {
        ApplicationDbContext db;
        IUnitOfWork unitOfWork;
        public MemberController(IUnitOfWork iUnitOfWorkCons)
        {
            unitOfWork = iUnitOfWorkCons;
            db = unitOfWork.DbContext;
        }
        string subject = "Deletion from website";
        string body = "We're sorry to inform you that we deleted you from our website";


        [Authorize(Roles = "Member")]
        // GET: Member
        public ActionResult ViewProfile()
        {
            if (Session["userid"] != null)
            {
                string userid = Session["userid"].ToString();
                Member mem = db.Members.Include(e => e.ApplicationUser).FirstOrDefault(e => e.Id == userid);
              
                if (mem != null)
                {
                    ViewBag.Title = "View Profile";
                    return View(mem);
                }
                else
                    return HttpNotFound();
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }
        [Authorize(Roles = "Member")]
        [HttpPost]
        public ActionResult ViewProfile(string id)
        {
            string userid = Session["userid"].ToString();
            Member mem = db.Members.Include(e => e.ApplicationUser).FirstOrDefault(e => e.Id == userid);
            if (mem != null)
            {
                mem.ChangedByBasicAdmin = false;
                db.SaveChanges();
                return View(mem);
            }
            else
                return HttpNotFound();
        }
        [Authorize(Roles = "Member")]
        public ActionResult EditProfile()
        {
            ViewBag.Title = "Edit Profile";
            string userid = Session["userid"].ToString();
            Member mem = db.Members.Include(e => e.ApplicationUser).FirstOrDefault(e => e.Id == userid);
          
            if (mem != null)
            { return PartialView(mem); }
            else
                return HttpNotFound();

        }
        [Authorize(Roles = "Member")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditProfile(Member newmem, HttpPostedFileBase img)
        {
            string imgName;
            string ext;
            //HttpPostedFileBase img = Request.Form["img"];
            if (ModelState.IsValid)
            {
                Member oldmem = db.Members.Include(a => a.ApplicationUser).FirstOrDefault(a => a.Id == newmem.Id);
                if (oldmem != null)
                {
                    oldmem.ApplicationUser.Fname = newmem.ApplicationUser.Fname;
                    oldmem.ApplicationUser.Lname = newmem.ApplicationUser.Lname;
                    oldmem.ApplicationUser.BDate = newmem.ApplicationUser.BDate;
                    oldmem.ApplicationUser.Address = newmem.ApplicationUser.Address;
                    oldmem.ApplicationUser.Email = newmem.ApplicationUser.Email;
                    oldmem.ApplicationUser.PhoneNumber = newmem.ApplicationUser.PhoneNumber;
                    oldmem.ApplicationUser.UserName = newmem.ApplicationUser.Email;
                    if (img != null)
                    {
                        ext = img.FileName.Substring(img.FileName.LastIndexOf("."));
                        imgName = newmem.Id + ext;
                        img.SaveAs(Server.MapPath("/Images/") + imgName);
                        oldmem.ApplicationUser.Photo = "/Images/" + imgName;
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
            return PartialView(newmem);
        }


        [Authorize(Roles = "BasicAdmin")]
        public ActionResult MemberForm()
        {
            return View();
        }

        [Authorize(Roles = "BasicAdmin")]
        public ActionResult MemberFormpartial()
        {
            string searchInput = Request.QueryString["name"];
            if (searchInput != null && searchInput != "")
            {
                if ((searchInput.Split(' ')).Length == 2)
                {
                    string firstName = searchInput.Split(' ')[0];
                    string lastName = searchInput.Split(' ')[1];
                    List<Member> memsSearch = db.Members.Include(e => e.ApplicationUser).Where(b => b.ApplicationUser.Fname == firstName && b.ApplicationUser.Lname == lastName).ToList();
                    if (memsSearch != null)
                    {
                        return View(memsSearch);
                    }
                }
            }

            List<Member> mems = db.Members.Include(e => e.ApplicationUser).ToList();
            return PartialView(mems);
        }
        public JsonResult GetSearchValue(string search)
        {
            List<Member> allsearch = db.Members.Include(u => u.ApplicationUser).Where(x => x.ApplicationUser.Fname.Contains(search) || x.ApplicationUser.Lname.Contains(search)).ToList();
            return new JsonResult { Data = allsearch, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
       
        [Authorize(Roles = "BasicAdmin")]

        public ActionResult EditMember(string id)
        {
            Member mem = db.Members.Include(e => e.ApplicationUser).First(e => e.Id == id);
            return PartialView(mem);
        }
        [Authorize(Roles = "BasicAdmin")]

        [HttpPost]
        public ActionResult EditMember(Member newmem)
        {
            if (ModelState.IsValid)
            {
                Member oldmem = db.Members.Include(a => a.ApplicationUser).First(a => a.Id == newmem.Id);
                oldmem.ApplicationUser.Fname = newmem.ApplicationUser.Fname;
                oldmem.ApplicationUser.Lname = newmem.ApplicationUser.Lname;
                oldmem.ApplicationUser.BDate = newmem.ApplicationUser.BDate;
                oldmem.ApplicationUser.Address = newmem.ApplicationUser.Address;
                oldmem.ApplicationUser.Email = newmem.ApplicationUser.Email;
                oldmem.ApplicationUser.PhoneNumber = newmem.ApplicationUser.PhoneNumber;
                oldmem.ApplicationUser.UserName = newmem.ApplicationUser.UserName;
                oldmem.ChangedByBasicAdmin = true;
                if (Session["userRole"].ToString() == "BasicAdmin")
                {             
                    oldmem.ChangedByBasicAdminToAdmins = true;
                }
                db.SaveChanges();

                return RedirectToAction(nameof(MemberForm));

            }
            return View(newmem);
        }
        [Authorize(Roles = "BasicAdmin")]

        public ActionResult DeleteMember(string id)
        {

            Member mem = db.Members.FirstOrDefault(e => e.Id == id);
            ApplicationUser user = db.Users.First(u => u.Id == id);
            string mail;
            bool response;
            if (mem != null)
            {

                mail = user.Email;
                response = SendEmail(mail, subject, body);
                if (response)
                {
                    db.Members.Remove(mem);
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
                return HttpNotFound("no Member with this id exist");
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
    }
}
using LibrarySystemV1._1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Net.Mail;
using System.Net;

namespace LibrarySystemV1._1.Controllers
{

    [Authorize(Roles = "Admin,BasicAdmin,Employee")]
    public class EmployeeController : Controller
    {

        ApplicationDbContext db;
        IUnitOfWork unitOfWork;
        string subject = "Deletion from website";
        string body = "We're sorry to inform you that we deleted you from our website";
        public EmployeeController(IUnitOfWork iUnitOfWorkCons)
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

        [Authorize(Roles = "Employee")]
        // GET: Employee
        public ActionResult ViewProfile()
        {
            if (Session["userid"] != null)
            {
                string userid = Session["userid"].ToString();
                Employee emp = db.Employees.Include(e => e.ApplicationUser).FirstOrDefault(e => e.Id == userid);

                if (emp != null)
                {
                    ViewBag.Title = "View Profile";
                    return View(emp);
                }
                else
                    return HttpNotFound();
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }
        [Authorize(Roles = "Employee")]
        [HttpPost]
        public ActionResult ViewProfile(string id)
        {
            string userid = Session["userid"].ToString();
            Employee emp = db.Employees.Include(e => e.ApplicationUser).FirstOrDefault(e => e.Id == userid);
            if (emp != null)
            {
                emp.ChangedByBasicAdmin = false;
                db.SaveChanges();
                return View(emp);
            }
            else
                return HttpNotFound();
        }
        [Authorize(Roles = "Employee")]
        public ActionResult EditProfile()
        {
            ViewBag.Title = "Edit Profile";
            string userid = Session["userid"].ToString();
            Employee emp = db.Employees.Include(e => e.ApplicationUser).FirstOrDefault(e => e.Id == userid);
            if (emp != null)
            { return PartialView(emp); }
            else
                return HttpNotFound();

        }
        [Authorize(Roles = "Employee")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditProfile(Employee newemp, HttpPostedFileBase img)
        {
            string imgName;
            string ext;
            if (ModelState.IsValid)
            {
                Employee oldemp = db.Employees.Include(a => a.ApplicationUser).FirstOrDefault(a => a.Id == newemp.Id);
                if (oldemp != null)
                {
                    oldemp.ApplicationUser.Fname = newemp.ApplicationUser.Fname;
                    oldemp.ApplicationUser.Lname = newemp.ApplicationUser.Lname;
                    oldemp.ApplicationUser.BDate = newemp.ApplicationUser.BDate;
                    oldemp.ApplicationUser.Address = newemp.ApplicationUser.Address;
                    oldemp.ApplicationUser.Email = newemp.ApplicationUser.Email;
                    oldemp.ApplicationUser.PhoneNumber = newemp.ApplicationUser.PhoneNumber;
                    oldemp.ApplicationUser.UserName = newemp.ApplicationUser.Email;
                    if (img != null)
                    {
                        ext = img.FileName.Substring(img.FileName.LastIndexOf("."));
                        imgName = newemp.Id + ext;
                        img.SaveAs(Server.MapPath("/Images/") + imgName);
                        oldemp.ApplicationUser.Photo = "/Images/" + imgName;
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
            return PartialView(newemp);
        }


        // GET: Employee
        [Authorize(Roles = "Admin,BasicAdmin")]
        public ActionResult EmployeeForm()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]

        public ActionResult EmployeeNotif()
        {
            List<Employee> emps = db.Employees.Include(e => e.ApplicationUser).Where(e => e.ChangedByBasicAdminToAdmins == true).ToList();
            return PartialView(emps);
        }
        [Authorize(Roles = "Admin")]

        public ActionResult EmployeeNotifed(String id)
        {
            Employee emp = db.Employees.FirstOrDefault(e => e.Id == id);
            if (emp != null)
            {
                emp.ChangedByBasicAdminToAdmins = false;
                db.SaveChanges();
            }
            List<Employee> emps = db.Employees.Include(e => e.ApplicationUser).Where(e => e.ChangedByBasicAdminToAdmins == true).ToList();
            return PartialView(emps);
        }
        [Authorize(Roles = "Admin,BasicAdmin")]
        public ActionResult EmployeeFormpartial()
        {
            string searchInput = Request.QueryString["name"];
            if (searchInput != null && searchInput != "")
            {
                if ((searchInput.Split(' ')).Length == 2)
                {
                    string firstName = searchInput.Split(' ')[0];
                    string lastName = searchInput.Split(' ')[1];
                    List<Employee> empsSearch = db.Employees.Include(e => e.ApplicationUser).Where(b => b.ApplicationUser.Fname == firstName && b.ApplicationUser.Lname == lastName).ToList();
                    if (empsSearch != null)
                    {
                        return View(empsSearch);
                    }
                }
            }

            List<Employee> emps = db.Employees.Include(e => e.ApplicationUser).ToList();
            return PartialView(emps);
        }
        public JsonResult GetSearchValue(string search)
        {
            List<Employee> allsearch = db.Employees.Include(u => u.ApplicationUser).Where(x => x.ApplicationUser.Fname.Contains(search) || x.ApplicationUser.Lname.Contains(search)).ToList();
            return new JsonResult { Data = allsearch, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        [Authorize(Roles = "Admin")]

        public ActionResult AddEmployee()
        {
            return PartialView(new empData { });
        }
        [Authorize(Roles = "Admin")]

        [HttpPost]
        public async Task<ActionResult> AddEmployee(empData newemp)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {

                    UserName = newemp.remp.Email,
                    Email = newemp.remp.Email,
                    Fname = newemp.remp.Fname,
                    Lname = newemp.remp.Lname,
                    BDate = newemp.remp.BDate,
                    Address = newemp.remp.Address,
                    PhoneNumber = newemp.remp.PhoneNumber,
                    EmailConfirmed = true
                };
                UserManager<ApplicationUser> us = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                var res = await us.CreateAsync(user, newemp.remp.Password);
                if (res.Succeeded)
                {
                    db.Employees.Add(new Employee() { Id = user.Id, Salary = newemp.emp.Salary, HireDate = newemp.emp.HireDate });
                    db.SaveChanges();
                    await us.AddToRoleAsync(user.Id, "Employee");
                    return PartialView("EmployeeRowpartial", newemp);
                }
                else
                {
                    ModelState.AddModelError("", "invalid password");
                    return PartialView(newemp);
                }
            }
            return PartialView(newemp);
        }
        [Authorize(Roles = "Admin,BasicAdmin")]

        public ActionResult EditEmployee(string id)
        {
            Employee emp = db.Employees.Include(e => e.ApplicationUser).First(e => e.Id == id);
            return PartialView(emp);
        }
        [Authorize(Roles = "Admin,BasicAdmin")]

        [HttpPost]
        public ActionResult EditEmployee(Employee newemp)
        {
            if (ModelState.IsValid)
            {
                Employee oldemp = db.Employees.Include(a => a.ApplicationUser).First(a => a.Id == newemp.Id);
                oldemp.ApplicationUser.Fname = newemp.ApplicationUser.Fname;
                oldemp.ApplicationUser.Lname = newemp.ApplicationUser.Lname;
                oldemp.ApplicationUser.BDate = newemp.ApplicationUser.BDate;
                oldemp.ApplicationUser.Address = newemp.ApplicationUser.Address;
                oldemp.ApplicationUser.Email = newemp.ApplicationUser.Email;
                oldemp.ApplicationUser.PhoneNumber = newemp.ApplicationUser.PhoneNumber;
                oldemp.ApplicationUser.UserName = newemp.ApplicationUser.UserName;
                oldemp.HireDate = newemp.HireDate;
                oldemp.Salary = newemp.Salary;
                oldemp.ChangedByBasicAdmin = true;
                if (Session["userRole"].ToString() == "BasicAdmin")
                {
                    oldemp.ChangedByBasicAdminToAdmins = true;
                }
                db.SaveChanges();

                return RedirectToAction(nameof(EmployeeForm));

            }
            return View(newemp);
        }
        [Authorize(Roles = "Admin,BasicAdmin")]

        public ActionResult DeleteEmployee(string id)
        {

            Employee emp = db.Employees.FirstOrDefault(e => e.Id == id);
            ApplicationUser user = db.Users.First(u => u.Id == id);
            string mail;
            bool response;
            if (emp != null)
            {
                mail = user.Email;
                response = SendEmail(mail, subject, body);
                if (response)
                {

                    db.Employees.Remove(emp);
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
    }
}

       

        
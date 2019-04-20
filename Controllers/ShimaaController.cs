using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Threading.Tasks;
using LibrarySystemV1._1.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;


namespace LibrarySystemV1._1.Controllers
{
    //[Authorize(Roles = "Member")]
    public class ShimaaController : Controller
    {
        ApplicationDbContext db; //= new ApplicationDbContext();
        ApplicationUser user = new ApplicationUser();
        IUnitOfWork unitofwork;
        // GET: Member
        public ShimaaController(IUnitOfWork UoW)
        {
            db = UoW.DbContext;
            unitofwork = UoW;
        }


        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ViewProfile()
        {
            string id = Session["userid"].ToString();
            Member member = db.Members.Include(u => u.ApplicationUser).First(e => e.Id == id);            
            return View(member);
        }

        [HttpGet]
        public ActionResult update()
        {
            string id = Session["userid"].ToString();
            Member member = db.Members.Include(u => u.ApplicationUser).First(e => e.Id == id);
            return View(member);
        }


        [HttpPost]
        public ActionResult update(string dummy)
        {
            string id = Session["userid"].ToString();
            Member member = db.Members.Include(u => u.ApplicationUser).First(e => e.Id == id);
            member.ApplicationUser.Fname = Request.Form["Fname"];
            member.ApplicationUser.Lname = Request.Form["Lname"];
            member.ApplicationUser.BDate = DateTime.Parse(Request.Form["BDate"]);
            member.ApplicationUser.Address = Request.Form["Address"];
            member.ApplicationUser.Email = Request.Form["Email"];
            db.SaveChanges();
            return RedirectToAction(nameof(ViewProfile));
        }

        //View Current Borrowed Books and Return Due Date
        public ActionResult ShowMemberBooks()
        {
            string id = Session["userid"].ToString();
            List<MemberBook> mbook = db.MembersBooks.Include(e => e.Book).Where(b => b.Id == id).ToList();

            if (User.IsInRole("Member") && mbook != null) //null
            {
                return View(mbook);
            }
            else
            {
                return RedirectToAction(nameof(ViewProfile));
            }
        }

        //View New Arrived Books"Publishing Date"
        public ActionResult ShowArrivedBooks()
        {
            string id = Session["userid"].ToString();
            List<MemberBook> mbook = db.MembersBooks.Include(e => e.Book).Where(b => b.Id == id).ToList();
            return View(mbook);
        }
       
        //Notification on late returned books
        public ActionResult LateReturnedBooks()
        {
            string id = Session["userid"].ToString();
            List<MemberBook> mbook = db.MembersBooks.Include(e => e.Book).Where(b => b.Id == id).ToList();
            return View(mbook);
        }
        [HttpGet]
        public ActionResult ShowBooks()
        {
            return View();
        }

        [HttpPost]
        //Search books filtered by(Year, Category, Publisher, Author, Title, Availability)
        public ActionResult ShowBooksPartial(string dummy)
        {
            List<Book> books;
            int Year;
            bool IsYearNumber = int.TryParse(Request.Form["PublishingDate.Year"].ToString(), out Year);
            string category = Request.Form["category"].ToString().ToLower();
            string publisher = Request.Form["publisher"].ToString().ToLower();
            string author = Request.Form["author"].ToString().ToLower();
            string title = Request.Form["title"].ToString().ToLower();
            //    string AvailabilityCheck = Request.Form["available"].ToString();

            if (!IsYearNumber)
            {
                books = unitofwork.BookRepository.Get(b => b.Category.ToLower().Contains(category) && b.Publisher.ToLower().Contains(publisher) && b.Title.ToLower().Contains(title) && b.Author.ToLower().Contains(author)).ToList<Book>();
            }
            else
            {
                books = unitofwork.BookRepository.Get(b => b.Category.ToLower().Contains(category) && b.Publisher.ToLower().Contains(publisher) && b.Title.ToLower().Contains(title) && b.Author.ToLower().Contains(author) && b.PublishingDate.Year == Year).ToList<Book>();
            }
            ViewBag.Books = books;
            return PartialView(books);
        }


    }
}


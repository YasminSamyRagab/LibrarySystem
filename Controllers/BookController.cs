using LibrarySystemV1._1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LibrarySystemV1._1.Controllers
{
    public class BookController : Controller
    {
        ApplicationDbContext db;
        IUnitOfWork unitOfWork;
        public BookController(IUnitOfWork iUnitOfWorkCons)
        {
            unitOfWork = iUnitOfWorkCons;
            db = unitOfWork.DbContext;
        }

        // GET: Book
        public ActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        public ActionResult ShowBooks()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        public ActionResult ShowBookspartial()
        {  
            List<Book> books = db.Books.ToList();
            return PartialView(books);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult AddBook()
        {
            return PartialView(new Book { });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult AddBook(Book bk)
        {
            if (ModelState.IsValid)
            {
                db.Books.Add(bk);
                db.SaveChanges();
                return PartialView("BookRowpartial", bk);
            }
            return PartialView(bk);
        }
        [Authorize(Roles = "Admin")]

        public ActionResult EditBook(int id)
        {
            Book books = db.Books.First(b => b.BookID == id);
            return PartialView(books);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]

        public ActionResult EditBook(Book bk)
        {
            if (ModelState.IsValid)
            {
                Book oldbook = db.Books.FirstOrDefault(b => b.BookID == bk.BookID);
                if (oldbook != null)
                {
                    oldbook.Author = bk.Author;
                    oldbook.Category = bk.Category;
                    oldbook.Edition = bk.Edition;
                    oldbook.NoOfCopies = bk.NoOfCopies;
                    oldbook.Pages = bk.Pages;
                    oldbook.Publisher = bk.Publisher;
                    oldbook.PublishingDate = bk.PublishingDate;
                    oldbook.ShelfNumber = bk.ShelfNumber;
                    oldbook.Title = bk.Title;
                    db.SaveChanges();
                }
                else
                {
                    return HttpNotFound();
                }
            }
            return PartialView(bk);
        }
        public ActionResult DeleteBook(int id)
        {
            Book bk = db.Books.FirstOrDefault(b => b.BookID == id);
            if(bk != null)
            {
                db.Books.Remove(bk);
                db.SaveChanges();
                return View();
            }
            else
            {
                return HttpNotFound();
            }
        }
    }
}
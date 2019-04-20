using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using LibrarySystemV1._1.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace LibrarySystemV1._1.Controllers
{
    public class MemberBookController : Controller
    {
		IUnitOfWork unitOfWork;
		public MemberBookController(IUnitOfWork iUnitOfWorkCons) :base()
		{
			unitOfWork = iUnitOfWorkCons;
		}

		//protected override void OnActionExecuting(ActionExecutingContext filterContext)
		//{
		//	//filterContext.Result = RedirectToAction("login","account");
		//	filterContext.Result = RedirectToAction("library","employee");
		//}

		// GET: Employee
		public ActionResult Library()
        {
			IEnumerable<Book> books = unitOfWork.BookRepository.Get();
			return View(books);
        }

		[HttpPost]
		public ActionResult Library(string dummy ) //int id
		{
			ViewBag.Error = "";
			int id = int.Parse(Request.Form["bookid"]);
			string email = Request.Form["email"];
			string action = Request.Form["action"].ToLower();
			Book bk = unitOfWork.BookRepository.GetByID(id);
			Member mem; 
			mem = unitOfWork.MemberRepository.DbSet.SingleOrDefault<Member>(m => m.ApplicationUser.Email == email);

			if (mem == null)
			{
				//ViewBag.memberExists = false;
				ViewBag.Error = "Member Doesn't Exist or Incorrect Email";
			}
			else //Member was found by Email and is in the DB
			{
				//////////////////////////////Check if Member is Verified or Not/////////////////////////////////////////

				//Check if Member is prevented or not
				bool RemainPrevented = false;
				if (mem.IsPrevented)
				{
					List<MemberBook> mBooks;
					//PREVENT READ CHECK
					mBooks = unitOfWork.MemberBookRepository.DbSet.Where(mb => mb.Member.Id == mem.Id && mb.Borrow == false).ToList<MemberBook>();
					
					foreach (MemberBook mBook in mBooks)
					{
						// are all return dates+7 > today and not returned in the same day and not borrowed today
						if (mBook.BorrowDate!=DateTime.Today && mBook.ReturnDate==null ||  mBook.ReturnDate != null && mBook.BorrowDate != mBook.ReturnDate && ((DateTime)mBook.ReturnDate).AddDays(7) > DateTime.Today)
						{
							RemainPrevented = true;
						}
					}

					//PREVENT BORROW CHECK
					mBooks = unitOfWork.MemberBookRepository.DbSet.Where(mb => mb.Member.Id == mem.Id && mb.Borrow == true).ToList<MemberBook>();
					//are all return dates+7 > today 
					foreach (MemberBook mBook in mBooks)
					{
						if ( (  ((DateTime)mBook.BorrowDate).AddDays(14) < DateTime.Today && mBook.ReturnDate==null  ) || ((DateTime)mBook.BorrowDate).AddDays(14) < mBook.ReturnDate && ((DateTime)mBook.ReturnDate).AddDays(7) > DateTime.Today )//mBook.ReturnDate != null && ((DateTime)mBook.ReturnDate).AddDays(7) > DateTime.Today) //ReturnDate!=null to prevent exception
						{
							RemainPrevented = true;
						}
					}
					//else set IsPrevened = false and let member borrow the book
					if (!RemainPrevented)
					{
						mem.IsPrevented = false;
					}
				}

				if (RemainPrevented)
				{
					ViewBag.Error = "This Member is Temporarily Prevented From Borrowing";
				}
				else // Member is not Prevented
				{ 
					if (action == "read")
					{
						MemberBook mBook;
						mBook = unitOfWork.MemberBookRepository.DbSet.SingleOrDefault<MemberBook>(mb => mb.BookID == id && mb.ReturnDate == null && mb.Borrow == false);
						if (mBook != null)
						{
							ViewBag.Error = "Another Member is Currently Reading this Book";
						}
						else
						{


							if (bk.NoOfAvailableCopies <= 0) //no Available Copies
							{
								ViewBag.Error = "There are Currently No Available Copies For Reading";
							}
							else
							{
								//Check if Member Book Read is in DB or not
								mBook = unitOfWork.MemberBookRepository.DbSet.SingleOrDefault<MemberBook>(mb => mb.BookID == id && mb.Member.Id == mem.Id && mb.Borrow == false);
								if (mBook == null) //Create a new Record in DB
								{
									mBook = new MemberBook
									{
										Id = mem.Id,
										BookID = id,
										BorrowDate = DateTime.Today,
										ReadCount = 1,
										Borrow = false
									};
									bk.NoOfBorrowedCopies++;
									unitOfWork.MemberBookRepository.Insert(mBook);
								}
								else // a Record was found
								{
									mBook.ReadCount++;
									mBook.BorrowDate = DateTime.Today;
									mBook.ReturnDate = null;
									bk.NoOfBorrowedCopies++;
								}
								unitOfWork.Save();
							}
						}
					}
					else //Borrow
					{
						if (bk.NoOfAvailableCopies <= 1) //no Available Copies: Library must have a spare book for reading
						{
							ViewBag.Error = "There are Currently No Available Copies For Borrowing";
						}
						else //Can Borrow Book
						{
							MemberBook mBook;
							mBook = unitOfWork.MemberBookRepository.DbSet.SingleOrDefault<MemberBook>(mb => mb.BookID == id && mb.Member.Id == mem.Id && mb.Borrow == true); //&& mb.ReturnDate==null);
							if (mBook == null) //Create a new Record in DB
							{
								mBook = new MemberBook
								{
									Id = mem.Id,
									BookID = id,
									BorrowDate = DateTime.Today,
									BorrowCount = 1,
									Borrow = true
								};
								unitOfWork.MemberBookRepository.Insert(mBook);
								bk.NoOfBorrowedCopies++;
								unitOfWork.Save();
							}
							else // a Record was found
							{
								if (mBook.ReturnDate == null)
								{
									ViewBag.Error = "This Member has a Copy of this body and has not returned it yet";
								}
								else //book was borrowed and returned
								{
									mBook.BorrowCount++;
									mBook.BorrowDate = DateTime.Today;
									mBook.ReturnDate = null;
									bk.NoOfBorrowedCopies++;
									unitOfWork.Save();  
								}
							} // end of a Record was Found
						} // end of Can Borrow Book
					}//end of Read or Borrow
				}// end of  Member is not Prevented
			}//end of Member was found by Email and is in the DB
			return View(nameof(Library), unitOfWork.BookRepository.Get()); 
		} // end of Action

		[HttpGet]
		public ActionResult ReturnBooksView()
		{
			string searchInput = Request.QueryString["name"];
			if (searchInput != null)
			{
				List<Book> books = unitOfWork.DbContext.Books.Where<Book>(b => b.Title == searchInput).ToList<Book>();				
				return View(books);
			}
			return View();
		}
		[HttpPost]
		public ActionResult ReturnBooksView(string dummy)
		{
			int id = int.Parse(Request.Form["bookid"]);
			string memEmail = Request.Form["email"];
			string action = Request.Form["action"].ToLower();
			Member mem = unitOfWork.MemberRepository.DbSet.SingleOrDefault<Member>(m => m.ApplicationUser.Email == memEmail);
			if (mem == null)
			{
				ViewBag.Error = "Member Doesn't Exist or Incorrect Email";
			}
			else // Member Exists and is in DB
			{
				MemberBook mbook;
				if (action == "read")
				{
					mbook = unitOfWork.DbContext.MembersBooks.Include(mb => mb.Book).SingleOrDefault<MemberBook>(mb => mb.BookID == id && mb.Member.ApplicationUser.Email == memEmail && mb.Borrow == false);
					if (mbook == null)
					{
						ViewBag.Error = "This Book wasn't Borrowed by this Member for Reading";
					}
					else // Record Exists
					{
						if (mbook.ReturnDate != null)
						{
							ViewBag.Error = "This Book was Already returned by this Member";
						}
						else // Book Taken for Reading and Not Returned
						{
							mbook.ReturnDate = DateTime.Today;
							unitOfWork.Save();
							if (mbook.BorrowDate < mbook.ReturnDate)
							{
								ViewBag.Late = "This Member is Late on Returning the Book, Prevent him/her ?";
								ViewBag.Email = memEmail;
								ViewBag.BorrowDate = ((DateTime)mbook.BorrowDate).ToShortDateString();
								ViewBag.ReturnDate = DateTime.Today.ToShortDateString();
								ViewBag.Duration = (DateTime.Today).Subtract((DateTime)mbook.BorrowDate).Days;
							}
						}
					}
				}
				else //Borrow
				{
					mbook = unitOfWork.DbContext.MembersBooks.Include(mb=>mb.Book).SingleOrDefault<MemberBook>(mb => mb.BookID == id && mb.Member.ApplicationUser.Email == memEmail && mb.Borrow==true);
					if (mbook == null) 
					{
						ViewBag.Error = "This Book is not Currently Borrowed by this Member";
					}
					else // This Book is Currently Borrowed by this Member 
					{
						if (mbook.ReturnDate == null) //Book wasn't already Returned by this Member
						{ 
							mbook.ReturnDate = new DateTime(2080, 05, 04);
							mbook.ReturnDate = DateTime.Today;
							mbook.Book.NoOfBorrowedCopies--;
							//DateTime aa = new DateTime();
							//aa = mbook.BorrowDate;
							if (mbook.ReturnDate > ((DateTime)mbook.BorrowDate).AddDays(14))
							{
								ViewBag.Late = "This Member is Late on Returning the Book, Prevent him/her ?";
								ViewBag.Email = memEmail;
								//ViewBag.BookID = id;
								ViewBag.BorrowDate = ((DateTime)mbook.BorrowDate).ToShortDateString();
								ViewBag.ReturnDate = DateTime.Today.ToShortDateString();
								ViewBag.Duration = (DateTime.Today).Subtract((DateTime)mbook.BorrowDate).Days;
							}
							//mbook.BorrowDate = null;
							//dbContext.SaveChanges();
							unitOfWork.Save();
						}
						else
						{
							ViewBag.Error = "This Book was Already Returned by this Member";
						}
					}// end of This Book is Currently Borrowed by this Member
				}//end of Borrow
			}// end of Member Exists and is in DB
			
			return View();
		}
		public ActionResult Prevent()
		{
			//int id = int.Parse(Request.Form["id"]);
			string memEmail = Request.Form["email"];
			Member mem=unitOfWork.MemberRepository.DbSet.Include("ApplicationUser").SingleOrDefault(m => m.ApplicationUser.Email == memEmail);
			mem.IsPrevented = true;
			unitOfWork.Save();
			return View(nameof(ReturnBooksView));
		}

		//public ActionResult EnterEmail(int id)
		//{
		//	//Create this view
		//	ViewBag.bookID = id;
		//	return View(nameof(ReturnBooks));
		//}
		//public ActionResult ReturnBooks()
		//{

		//}
		//new AuthenticationManager
		//public ActionResult bleh()
		//{
		//	return View();
		//}
		public JsonResult GetSearchValue(string search)
		{
			//Context db = new ApplicationDbContext();
			//List<BookBL> allsearch = db.Books.Where(x => x.Title.Contains(search)).Select(x => new BookBL
			List < BookBL> allsearch = unitOfWork.DbContext.Books.Where(x => x.Title.Contains(search)).Select(x => new BookBL
			{
				BookID = x.BookID,
				Title = x.Title
			}).ToList();
			return new JsonResult { Data = allsearch, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}
	}
}
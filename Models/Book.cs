using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LibrarySystemV1._1.Models
{
	public class Book
	{
		[Key]
		public int BookID { get; set; }
		[Required]
		public string Title { get; set; }
		[Required]
		public string Author { get; set; }
		[Required]
		public string Publisher { get; set; }
        [DataType(DataType.Date)]
        [Display(Name ="Publishing Date")]
        public DateTime PublishingDate { get; set; }
		[Required]
		public string Category { get; set; }
		public string Edition { get; set; }
		public int Pages { get; set; }
		[Required]
        [Display(Name = "No Of Copies")]
        public int NoOfCopies { get; set; }
		[Required]
        [Display(Name = "No Of Borrowed Copies")]

        public int NoOfBorrowedCopies { get; set; }
		[NotMapped]
        [Display(Name = "No Of Available Copies")]

        public int NoOfAvailableCopies => NoOfCopies - NoOfBorrowedCopies ;
        [Display(Name = "Shelf Number")]

        public int ShelfNumber { get; set; }
		public List<MemberBook> MembersBooks { get; set; }

		//ID , title , author , publisher,PublishingDate, Category ,Edition, pages, NoOfCopies, Avilable,shelfNumber)
	}
    public class BookBL
    {
        public int BookID { get; set; }
        public string Title { get; set; }
    }
}

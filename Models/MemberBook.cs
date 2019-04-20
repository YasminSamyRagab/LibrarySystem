using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LibrarySystemV1._1.Models
{
	public class MemberBook
	{
		[ForeignKey("Member")]
		[Key]
		[Column(Order =1)]
		public string Id { get; set; }
		[ForeignKey("Book")]
		[Key]
		[Column(Order = 2)]
		public int BookID { get; set; }
		public Member Member { get; set; }
		public Book Book { get; set; }
        [Display(Name = "Borrow Count")]
        public int BorrowCount { get; set; }
        [Display(Name = "Read Count")]

        public int ReadCount { get; set; }
        [Display(Name = "Borrow Date")]
        [DataType(DataType.Date)]
        public DateTime? BorrowDate { get; set; }
        [Display(Name = "Return Date")]
        [DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }
		[Key]
		[Column(Order = 3)]
		public bool Borrow { get; set; }

	}
}
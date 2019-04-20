using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LibrarySystemV1._1.Models
{
	public class Employee
	{
		[Key]
		[ForeignKey("ApplicationUser")]
		public string Id { get; set; }
		[Required]
        [Display(Name = "Hire Date")]
        [DataType(DataType.Date)]
        public DateTime HireDate { get; set; }
		[Required]
		[Range(1000, 50000)]
		public int Salary { get; set; }
		public bool ChangedByBasicAdmin { get; set; }
		public bool ChangedByBasicAdminToAdmins { get; set; }
		public ApplicationUser ApplicationUser { get; set; }
	}
   public class empData
    {
       public Employee emp { get; set; }
        public RegisterViewModel remp { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace LibrarySystemV1._1.Models
{
	public class BasicAdmin
	{
		[Key]
		[ForeignKey("ApplicationUser")]
		public string Id { get; set; }
		[Required]
        [Display(Name = "Hire Date")]
        [DataType(DataType.Date)]
        public DateTime HireDate { get; set; }
		[Required]
		[Range(1000,50000)]
		public int Salary { get; set; }

		public ApplicationUser ApplicationUser { get; set; }
	}
}
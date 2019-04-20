using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LibrarySystemV1._1.Models
{
	public class Admin
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
		public ApplicationUser ApplicationUser { get; set; }
	}
    public class firstloginViewModels
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Old password")]
        public string olapassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string newpassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("newpassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string confirmnewpassword { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Upload Image")]
        public string photo { get; set; }
    }
    public class adData
    {
        public Admin ad { get; set; }
        public RegisterViewModel rad { get; set; }
    }
}
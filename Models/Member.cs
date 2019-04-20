using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LibrarySystemV1._1.Models
{
	public class Member
	{
		[Key]
		[ForeignKey("ApplicationUser")]
		public string Id { get; set; }
		public bool ChangedByBasicAdmin { get; set; }
		public bool ChangedByBasicAdminToAdmins { get; set; }
		public bool ChangedByBasicAdminToEmployees { get; set; }
		public bool IsPrevented { get; set; }
		public bool IsVerified { get; set; }
		public ApplicationUser ApplicationUser { get; set; }
		public List<MemberBook> MembersBooks { get; set; }
        [Display(Name = "Registeration Day")]
        [DataType(DataType.Date)]
        public DateTime RegisterationDay { get; set; }
    }
}
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace LibrarySystemV1._1.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
		[Required]
		[DisplayName("First Name")]
		[StringLength(20)]
		public string Fname { get; set; }
		[Required]
		[DisplayName("Last Name")]
		[StringLength(20)]
		public string Lname { get; set; }
		[Required]
		[DisplayName("Birth Date")]
        [DataType(DataType.Date)]
        public DateTime BDate { get; set; }
		[Required]
		public string Address { get; set; }
		public string Photo { get; set; }
	}

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
            this.Configuration.LazyLoadingEnabled = true;
        }

		public DbSet<BasicAdmin> BasicAdmins { get; set; }
		public DbSet<Admin> Admins { get; set; }
		public DbSet<Employee> Employees { get; set; }
		public DbSet<Member> Members { get; set; }
		public DbSet<Book> Books { get; set; }
		public DbSet<MemberBook> MembersBooks { get; set; }
		public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}
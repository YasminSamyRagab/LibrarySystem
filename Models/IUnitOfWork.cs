using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibrarySystemV1._1.Models
{
	public interface IUnitOfWork : IDisposable
	{
		ApplicationDbContext DbContext { get; }
		GenericRepository<ApplicationUser> ApplicationUserRepository { get; }
		GenericRepository<BasicAdmin> BasicAdminRepository { get; }
		GenericRepository<Admin> AdminRepository { get; }
		GenericRepository<Employee> EmployeeRepository { get; }
		GenericRepository<Member> MemberRepository { get; }
		GenericRepository<Book> BookRepository { get; }
		GenericRepository<MemberBook> MemberBookRepository { get; }
		void Save();
	}
}
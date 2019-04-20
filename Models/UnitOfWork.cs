using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibrarySystemV1._1.Models
{
	public class UnitOfWork : IUnitOfWork
	{
		//private ApplicationDbContext context = new ApplicationDbContext();
		private ApplicationDbContext context;
		private GenericRepository<ApplicationUser> applicationUserRepository;
		private GenericRepository<BasicAdmin> basicAdminRepository;
		private GenericRepository<Admin> adminRepository;
		private GenericRepository<Employee> employeeRepository;
		private GenericRepository<Member> memberRepository;
		private GenericRepository<Book> bookRepository;
		private GenericRepository<MemberBook> memberBookRepository;
		
		public UnitOfWork(ApplicationDbContext contextCons)
		{
			context = contextCons;
		}
		public ApplicationDbContext DbContext
		{
			get { return context; }
		}
		public GenericRepository<ApplicationUser> ApplicationUserRepository
		{
			get
			{

				if (this.applicationUserRepository == null)
				{
					this.applicationUserRepository = new GenericRepository<ApplicationUser>(context);
				}
				return applicationUserRepository;
			}
		}

		public GenericRepository<BasicAdmin> BasicAdminRepository
		{
			get
			{

				if (this.basicAdminRepository == null)
				{
					this.basicAdminRepository = new GenericRepository<BasicAdmin>(context);
				}
				return basicAdminRepository;
			}
		}
		public GenericRepository<Admin> AdminRepository
		{
			get
			{

				if (this.adminRepository == null)
				{
					this.adminRepository = new GenericRepository<Admin>(context);
				}
				return adminRepository;
			}
		}
		public GenericRepository<Employee> EmployeeRepository
		{
			get
			{

				if (this.employeeRepository == null)
				{
					this.employeeRepository = new GenericRepository<Employee>(context);
				}
				return employeeRepository;
			}
		}
		public GenericRepository<Member> MemberRepository
		{
			get
			{

				if (this.memberRepository == null)
				{
					this.memberRepository = new GenericRepository<Member>(context);
				}
				return memberRepository;
			}
		}
		public GenericRepository<Book> BookRepository
		{
			get
			{

				if (this.bookRepository == null)
				{
					this.bookRepository = new GenericRepository<Book>(context);
				}
				return bookRepository;
			}
		}
		public GenericRepository<MemberBook> MemberBookRepository
		{
			get
			{

				if (this.memberBookRepository == null)
				{
					this.memberBookRepository = new GenericRepository<MemberBook>(context);
				}
				return memberBookRepository;
			}
		}
		public void Save()
		{
			context.SaveChanges();
		}

		private bool disposed = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					context.Dispose();
				}
			}
			this.disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
using CommunityLibrary.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommunityLibrary.Repository
{

    public class LibraryDbContext :DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options): base(options)
        {
        }
        public DbSet<Book> Books { get; set; }

        public DbSet<Loans> Loan { get; set; }


    }
}

using CommunityLibrary.Models.DTOs;
using CommunityLibrary.Models.Entities;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace CommunityLibrary.Repository
{
    public class BooksRepository : IBooksRepository
    {

        private readonly LibraryDbContext _db;

        public BooksRepository(LibraryDbContext db)
        {
            _db = db;
        }
        public async Task<BookSearchResponse> getBooks(string? query, BookStatus? status, int limit, int offset, CancellationToken ct)
        {
            IQueryable<Book> books = _db.Books.AsNoTracking();

            

            if (!string.IsNullOrWhiteSpace(query))
            {
                books = books.Where(b => b.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
                        || b.Author.Contains(query, StringComparison.OrdinalIgnoreCase)
                        || b.Isbn.Contains(query, StringComparison.OrdinalIgnoreCase));
            }
            if (status.HasValue)
            {
                books = books.Where(b => b.Status == status);
            }
            var total = books.Count(); 
            var page =  await books
            .OrderBy(b => b.Title)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(ct);

            return new BookSearchResponse
            {
                Data = page.Select(MapToDto).ToList(),
                Pagination = new PaginationInfo { Limit = limit, Offset = offset, Total = total }
            };

        }
        public static BookDTO MapToDto(Book b) => new()
        {
            Id = b.Id,
            Title = b.Title,
            Author = b.Author,
            Isbn = b.Isbn,
            Status = b.Status == BookStatus.Available ? "Available" : "Borrowed",
            PublishedYear = b.PublishedYear
        };

        public IEnumerable<Book> getBooksbyID(Guid bookID)
        {
            var books = _db.Books.AsQueryable().Where(b => b.Id == bookID);

            return books;

        }

    }

}

using CommunityLibrary.Models.DTOs;
using CommunityLibrary.Models.Entities;

namespace CommunityLibrary.Repository
{
    public interface IBooksRepository
    {
        Task<BookSearchResponse> getBooks(string? query, BookStatus? status, int limit, int offset, CancellationToken ct);
        IEnumerable<Book> getBooksbyID(Guid bookID);
    }
}


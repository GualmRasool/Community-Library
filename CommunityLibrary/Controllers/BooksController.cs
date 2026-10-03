using CommunityLibrary.Models.DTOs;
using CommunityLibrary.Models.Entities;
using CommunityLibrary.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc;

namespace CommunityLibrary.Controllers
{
    
    [Route("/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class BooksController : ControllerBase
    {
        private readonly IBooksRepository _booksRepositry;

        public BooksController(IBooksRepository booksRepositry)        
        {
            _booksRepositry = booksRepositry;
        }
        /// <summary>
        /// Search for Books.
        /// </summary>
        /// <param name="query">
        /// Search term (Title, Author, or ISBN)
        /// </param>
        /// <param name="status">
        /// Filter by book status
        /// </param>
        /// <param name="limit">
        /// Maximum number of books to return
        /// </param>
        /// <param name="offset">
        /// Number of records to skip
        /// </param>
        /// <returns>List of books</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookSearchResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BookSearchResponse>> books(
            [FromQuery] string? query,
            [FromQuery] string status = "Available",
            [FromQuery] int limit = 20,
            [FromQuery] int offset = 0, CancellationToken ct = default)
        {
            if (query is { Length: > 255 })
            {
                return BadRequest(ErrorResponse.Create("INVALID_QUERY", "Query must be 255 characters or fewer."));
            }
            var validStatuses = new[] { "Available", "Borrowed", "All" };
            if (!Enum.TryParse<BookStatus>(status, true, out var bookStatus))
            {
                //throw new ArgumentException("Invalid status value");
                //400 -- bad request 
                return BadRequest(ErrorResponse.Create("INVALID_STATUS", $"Status must be one of: {string.Join(", ", validStatuses)}."));
                // or return Enumerable.Empty<Books>();
            }
            if (limit < 1 || limit > 100)
            {
                return BadRequest(ErrorResponse.Create("INVALID_LIMIT", "Limit must be between 1 and 100."));
            }
            if (offset < 0)
            {
                return BadRequest(ErrorResponse.Create("INVALID_OFFSET", "Offset must be 0 or greater."));
            }

            var books = await _booksRepositry.getBooks(query, bookStatus, limit, offset, ct);

           
            return Ok(books);
        }
        [HttpGet("test")]
        public ActionResult testCodeQL(string ID)
        {
            string query = $"Select * from user where id = {ID}";

            return Ok(query);
        }

    }
}

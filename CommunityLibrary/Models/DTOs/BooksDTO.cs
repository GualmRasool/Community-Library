using CommunityLibrary.Models.Entities;

namespace CommunityLibrary.Models.DTOs
{
    public class BookDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Isbn { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "available" | "borrowed"
        public int? PublishedYear { get; set; }
    }
    public class PaginationInfo
    {
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int Total { get; set; }
    }
    public class BookSearchResponse
    {
        public List<BookDTO> Data { get; set; } = new();
        public PaginationInfo Pagination { get; set; } = new();
    }
    public class ErrorDetail
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? Details { get; set; }
    }
    public class ErrorResponse
    {
        public ErrorDetail Error { get; set; } = new();

        public static ErrorResponse Create(string code, string message, object? details = null) =>
            new() { Error = new ErrorDetail { Code = code, Message = message, Details = details } };
    }

}

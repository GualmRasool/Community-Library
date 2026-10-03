using CommunityLibrary.Models.Entities;

namespace CommunityLibrary.Models.DTOs
{
    public class LoansDTO
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public string MemberId { get; set; } = string.Empty;
        public DateTime BorrowedAt { get; set; }
        public DateOnly DueDate { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public LoanStatus Status { get; set; } = LoanStatus.Active;
        public BookDTO? Book { get; set; }

    }
    public class LoanResponse
    {
        public LoansDTO Data { get; set; } = new();
    }
    public class LoansListResponse
    {
        public List<LoansDTO> Data { get; set; } = new();
        public PaginationInfo Pagination { get; set; } = new();
    }
}

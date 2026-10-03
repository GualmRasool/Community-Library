using System.ComponentModel.DataAnnotations;

namespace CommunityLibrary.Models.Entities
{
    public enum LoanStatus
    {
        Active,
        Overdue,
        Returned
    }
    public class Loans
    {
        public Guid Id { get; set; }

        public Guid BookId { get; set; }

        public Book? Book { get; set; }

        public string MemberId { get; set; } = string.Empty;

        public DateTime BorrowedAt { get; set; }

        public DateOnly DueDate { get; set; }

        public DateTime? ReturnedAt { get; set; }

        public LoanStatus Status { get; set; } = LoanStatus.Active;

        public string? Notes { get; set; }

        public void RefreshComputedStatus(DateOnly today)
        {
            if (Status == LoanStatus.Active && DueDate < today)
            {
                Status = LoanStatus.Overdue;
            }
        }

    }

}

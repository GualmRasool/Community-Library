using System.ComponentModel.DataAnnotations;

namespace CommunityLibrary.Models.Request
{
    public class BorrowRequest
    {
        [Required]
        public string MemberID { get; set; } = String.Empty;
    }
}
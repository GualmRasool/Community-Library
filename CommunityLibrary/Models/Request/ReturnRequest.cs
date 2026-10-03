using System.ComponentModel.DataAnnotations;

namespace CommunityLibrary.Models.Request
{
    public class ReturnRequest
    {
        [Required]
        public DateTime returnedAt { get; set; }
        public string? Notes { get; set; }
    }
}

namespace CommunityLibrary.Models.Entities
{
    public enum BookStatus
    {
        Available,
        Borrowed,
        All
    }
    public class Book
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Isbn { get; set; } = string.Empty;
        public BookStatus Status { get; set; } = BookStatus.Available;
        public int? PublishedYear { get; set; }
        public ICollection<Loans> Loans { get; set; } = new List<Loans>();
    }
}

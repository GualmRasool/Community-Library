using CommunityLibrary.Models.Entities;
using CommunityLibrary.Repository;

public static class DbSeeder
{
    public static readonly Guid Book1Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
    public static readonly Guid Book2Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440001");
    public static readonly Guid Book3Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440002");
    public static readonly Guid Book4Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440003");
    public static void Seed(LibraryDbContext context)
    {
        if (!context.Books.Any())
        {
            context.Books.AddRange(
                new Book
                {
                    Id = Book1Id,
                    Title = "My Titile 1",
                    Author = "Gulam Rasool",
                    Isbn = "978-0-7432-7356-5",
                    Status = BookStatus.Available,
                    PublishedYear = 2026
                },
                new Book
                {
                    Id = Book2Id,
                    Title = "Biogrphy",
                    Author = "Praveen",
                    Isbn = "978-0-06-093546-7",
                    Status = BookStatus.Available,
                    PublishedYear = 2000
                },
                new Book
                {
                    Id = Book3Id,
                    Title = "My Dairy",
                    Author = "John",
                    Isbn = "978-0-06-093546-7",
                    Status = BookStatus.Available,
                    PublishedYear = 2000
                }
             );

            context.SaveChanges();


        }
    }
}

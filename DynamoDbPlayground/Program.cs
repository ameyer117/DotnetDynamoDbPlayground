using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using DynamoDbPlayground.Models;


Console.WriteLine("Application started!");


var client = new AmazonDynamoDBClient();
var context = new DynamoDBContext(client, new DynamoDBContextConfig
{
    RetrieveDateTimeInUtc = true,
    ConsistentRead = true,
    DisableFetchingTableMetadata = true
});

var book = new Book
{
    AuthorId = Guid.NewGuid().ToString(),
    Id = Guid.NewGuid().ToString(),
    Title = "The Book Title",
    Published = DateTime.UtcNow,
    Genres = ["Fiction", "Fantasy"],
    Ratings = [5, 4, 5]
};

// Save the book to the database
await context.SaveAsync(book);

Console.WriteLine("Book saved to the database!");

// Load the book from the database
var loadedBook = await context.LoadAsync<Book>(book.AuthorId, book.Id);

Console.WriteLine("Book loaded from the database!");

Console.WriteLine($"AuthorId: {loadedBook.AuthorId}");
Console.WriteLine($"Id: {loadedBook.Id}");
Console.WriteLine($"Title: {loadedBook.Title}");
Console.WriteLine($"Published: {loadedBook.Published}");
Console.WriteLine($"Genres: {string.Join(", ", loadedBook.Genres)}");
Console.WriteLine($"Ratings: {string.Join(", ", loadedBook.Ratings)}");
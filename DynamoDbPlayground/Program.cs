using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using DynamoDbPlayground.Models;


Console.WriteLine("Application started!");


var client = new AmazonDynamoDBClient();
var context = new DynamoDBContext(client, new DynamoDBContextConfig
{
    RetrieveDateTimeInUtc = true,
    ConsistentRead = true,
});

// This is useful/necessary for more advanced queries
var table = Table.LoadTable(client, "Books");

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

// Update the book in the database
loadedBook.Title = "The Updated Book Title";
await context.SaveAsync(loadedBook);

// Get all Books with AuthorId of "f9995c92-7e38-4471-abea-75ce88c71928"
const string authorIdToQuery = "edfc627a-c4d9-497c-8be3-ae0ee5ce3ff8";
var books = await context.QueryAsync<Book>(authorIdToQuery).GetRemainingAsync();

Console.WriteLine($"Books loaded from the database for AuthorId: {authorIdToQuery} with a query!");

foreach (var b in books)
{
    Console.WriteLine($"AuthorId: {b.AuthorId}");
    Console.WriteLine($"Id: {b.Id}");
    Console.WriteLine($"Title: {b.Title}");
    Console.WriteLine($"Published: {b.Published}");
    Console.WriteLine($"Genres: {string.Join(", ", b.Genres)}");
    Console.WriteLine($"Ratings: {string.Join(", ", b.Ratings)}");
    Console.WriteLine();
}

// Do the same, but do a scan instead of a query
var secondAuthorIdToQuery = "e464b02a-7cd6-4843-8f89-84e040ef9e1b";
var booksScan = await context.ScanAsync<Book>(new List<ScanCondition>
{
    new ScanCondition(nameof(Book.AuthorId), ScanOperator.Equal, authorIdToQuery),
    new ScanCondition(nameof(Book.Id), ScanOperator.Equal, "2db5e378-f77b-4175-b3e4-8bbca902ec5c"),
}, new DynamoDBOperationConfig
{
    ConditionalOperator = ConditionalOperatorValues.Or
}).GetRemainingAsync();

// // You cannot do an OR operation with a scan on the Hash key (it will silently only return the second conditions results), so you need to do multiple scans and merge the results
// var booksScan2 = await context.ScanAsync<Book>(new List<ScanCondition>
// {
//     new ScanCondition(nameof(Book.AuthorId), ScanOperator.Equal, secondAuthorIdToQuery),
// }, new DynamoDBOperationConfig
// {
//     ConditionalOperator = ConditionalOperatorValues.Or
// }).GetRemainingAsync();

// booksScan.AddRange(booksScan2);

Console.WriteLine($"Books loaded from the database for AuthorId: {authorIdToQuery} with a scan!");

foreach (var b in booksScan)
{
    Console.WriteLine($"AuthorId: {b.AuthorId}");
    Console.WriteLine($"Id: {b.Id}");
    Console.WriteLine($"Title: {b.Title}");
    Console.WriteLine($"Published: {b.Published}");
    Console.WriteLine($"Genres: {string.Join(", ", b.Genres)}");
    Console.WriteLine($"Ratings: {string.Join(", ", b.Ratings)}");
    Console.WriteLine();
}

// Delete the oldest book with a query
var oldestBooksConfig = new QueryOperationConfig
{
    Filter = new QueryFilter(nameof(Book.AuthorId), QueryOperator.Equal, authorIdToQuery),
    Limit = 1,
    BackwardSearch = false, // Ascending order
    IndexName = "AuthorId-Published-index",
};

// Its important to note that we need to call GetNextSetAsync() instead of GetRemainingAsync() because we are using a Limit!!!
var oldestBooks = await context.FromQueryAsync<Book>(oldestBooksConfig).GetNextSetAsync();
Console.WriteLine($"Oldest book loaded from the database for AuthorId: {authorIdToQuery} with a query!");

foreach (var b in oldestBooks)
{
    Console.WriteLine($"AuthorId: {b.AuthorId}");
    Console.WriteLine($"Id: {b.Id}");
    Console.WriteLine($"Title: {b.Title}");
    Console.WriteLine($"Published: {b.Published}");
    Console.WriteLine($"Genres: {string.Join(", ", b.Genres)}");
    Console.WriteLine($"Ratings: {string.Join(", ", b.Ratings)}");
    Console.WriteLine();

    await context.DeleteAsync(b);
    Console.WriteLine("Oldest book deleted from the database!");
    Console.WriteLine();
}

// Load only the Published field (Projection) for all Books with AuthorId of "f9995c92-7e38-4471-abea-75ce88c71928"

QueryOperationConfig config = new QueryOperationConfig()
{
    KeyExpression = new Expression
    {
        ExpressionStatement = "AuthorId = :v_hashkey",
        ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
        {
            { ":v_hashkey", authorIdToQuery }
        }
    },
    AttributesToGet = ["Published"],
    Select = SelectValues.SpecificAttributes,
    ConsistentRead = true
};

Search search = table.Query(config);
List<Document> documentList = await search.GetNextSetAsync();

foreach (var doc in documentList)
{
    Console.WriteLine($"Published: {doc["Published"]}");
}

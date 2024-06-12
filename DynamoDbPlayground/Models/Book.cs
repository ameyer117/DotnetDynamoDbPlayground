using DynamoDbPlayground.Converters;

namespace DynamoDbPlayground.Models;

using Amazon.DynamoDBv2.DataModel;

[DynamoDBTable("Books")]
public class Book
{
    [DynamoDBHashKey] 
    public string AuthorId { get; set; } = default!;
    
    [DynamoDBRangeKey] 
    public string Id { get; set; } = default!;
    
    [DynamoDBProperty]
    public string Title { get; set; } = default!;
    
    [DynamoDBProperty]
    public DateTime Published { get; set; }

    [DynamoDBProperty]
    public List<string> Genres { get; set; } = [];

    // Dynamodb uses Sets by default, if we want to store a list of integers we need to use a custom converter
    [DynamoDBProperty(typeof(ListIntConverter))]
    public List<int> Ratings { get; set; } = [];
}
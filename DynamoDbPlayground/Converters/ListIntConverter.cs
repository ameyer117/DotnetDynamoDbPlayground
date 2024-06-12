namespace DynamoDbPlayground.Converters;

using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections.Generic;
using System.Linq;

public class ListIntConverter : IPropertyConverter
{
    public DynamoDBEntry ToEntry(object? value)
    {
        if (value is not List<int> list)
            throw new ArgumentException(
                $"Value to be saved to DynamoDB was expected to be a list of integers, received: {value?.GetType().Name}");
        
        var docList = new DynamoDBList();
        
        foreach (var item in list)
        {
            docList.Add(new Primitive(item.ToString(), true));
        }
        
        return docList;
    }

    public object FromEntry(DynamoDBEntry entry)
    {
        return entry switch
        {
            DynamoDBList docList => docList.Entries.Select(item => int.Parse((Primitive)item)).ToList(),
            PrimitiveList primitiveList => primitiveList.Entries.Select(item => int.Parse(item.AsString())).ToList(),
            _ => throw new ArgumentException(
                $"Entry from DynamoDB was expected to be a list of integers, received: {entry.GetType().Name}")
        };
    }
}

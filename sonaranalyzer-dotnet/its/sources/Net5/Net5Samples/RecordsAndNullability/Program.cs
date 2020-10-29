using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;


Author lord = new Author("Karen Lord")
{
    Website = "https://karenlord.wordpress.com/",
    RelatedAuthors = new()
};

lord.Books.AddRange(
    new Book[] 
    {
        new Book("The Best of All Possible Worlds", 2013, lord),
        new Book("The Galaxy Game", 2015, lord)
    }
);

lord.RelatedAuthors.AddRange(
    new Author[]
    {
        new ("Nalo Hopkinson"),
        new ("Ursula K. Le Guin"),
        new ("Orson Scott Card"),
        new ("Patrick Rothfuss")    
    }
);

Console.WriteLine($"Author: {lord.Name}");
Console.WriteLine($"Books: {lord.Books.Count}");
Console.WriteLine($"Related authors: {lord.RelatedAuthors.Count}");


public record Author(string Name)
{
    private List<Book> _books = new();

    public List<Book> Books => _books;

    public string? Website {get; init;}
    public string? Genre {get; init;}
    public List<Author>? RelatedAuthors {get; init;}
}

public record Book(string name, int Published, Author author);
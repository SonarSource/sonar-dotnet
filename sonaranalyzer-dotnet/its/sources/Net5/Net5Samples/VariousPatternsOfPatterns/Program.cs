using System;
using System.Collections.Generic;
using static AccessType;
using static ContentType;

Console.WriteLine("Mycroft information access 6.0");

var mycroftCaring = WhoDoesMycroftCareAbout();

foreach (var (user, content, season) in GetRandomData(20))
{
    Console.WriteLine($"User: {user.Name}; Content: {content.Name}; Season: {season}; Access: {UserManager.IsAccessOk(user, content, season)}");
}

Person[] GetUsers()
{
    return new Person[]
    {
        new Person("John Watson", AccessType.Adult),
        new Person("Mary (Morstan) Watson", AccessType.Adult),
        new Person("Rosamund Watson", AccessType.Child),
        new Person("Molly Hooper", AccessType.Adult),
        new OpenCaseFile("Jim Moriarty", AccessType.PoorlyDefined, 99),
        new OpenCaseFile("Sherlock Holmes", AccessType.Adult, 50),
        new OpenCaseFile("Eurus Holmes", AccessType.PoorlyDefined, 100),
        new Mycroft("Mycroft Holmes", AccessType.Adult){CaresAbout = mycroftCaring},
        new Person("Queen Elizabeth II", AccessType.Monarch)
    };
}

Content[] GetContents()
{
    return new Content[]
    {
        new Content("Holmes' children nursery rhymes", ContentType.ChildsPlay),
        new Content("Magna Carta", ContentType.Public),
        new Content("John's blog", ContentType.Public),
        new Content("Report on death of Charles Augustus Magnussen", ContentType.StateSecret),
        new Content("Meeting of Moriarty and Holmes at Sherrinford", ContentType.StateSecret),
        new Content("Lady Adler", ContentType.ForHerEyesOnly),
    };
}

IEnumerable<(Person, Content, int)> GetRandomData(int count)
{
    Random random = new();
    Person[] users = GetUsers();
    Content[] content = GetContents();

    for (int i = 0; i < count; i++)
    {
        int userIndex = random.Next(users.Length);
        int contentIndex = random.Next(content.Length);
        int seasonIndex = random.Next(1,5);
        var user = users[userIndex];

        if (user is Mycroft m && m.CaresAbout is not object)
        {
            Console.WriteLine("Mycroft dissapoints us again.");
        }
        
        yield return (user, content[contentIndex], seasonIndex);
    }
}

string? WhoDoesMycroftCareAbout()
{
    string?[] caresAbout = {"Himself", "Dear brother", null};
    Random random = new();
    return caresAbout[random.Next(0,3)];
}

public class UserManager
{
    public static bool IsAccessOk(Person user, Content content, int season)
    {
        return IsAccessOkOfficial(user, content, season) | IsAccessOKAskMycroft(user);
    }

    public static bool IsAccessOkOfficial(Person user, Content content, int season) => (user, content, season) switch 
    {
        // Tuple + property patterns
        ({Type: Child}, {Type: ChildsPlay}, _)          => true,
        ({Type: Child}, _, _)                           => false,
        (_ , {Type: Public}, _)                         => true,
        ({Type: Monarch}, {Type: ForHerEyesOnly}, _)    => true,
        // Tuple + type patterns
        (OpenCaseFile f, {Type: ChildsPlay}, 4) when f.Name == "Sherlock Holmes"  => true,
        // Property and type patterns
        {Item1: OpenCaseFile {Type: var type}, Item2: {Name: var name}} 
            when type == PoorlyDefined && name.Contains("Sherrinford") && season >= 3 => true,
        // Tuple and type patterns
        (OpenCaseFile, var c, 4) when c.Name.Contains("Sherrinford")              => true,
        // Tuple, Type, Property and logical patterns 
        (OpenCaseFile {RiskLevel: >50 and <100 }, {Type: StateSecret}, 3) => true,
        _                                               => false,
    };

    public static bool IsAccessOKAskMycroft(Person person) => person switch
    {
        // Type pattern
        OpenCaseFile f when f.Name == "Jim Moriarty"    => true,
        // Simple type pattern
        Mycroft                                         => true,
        _                                               => false,
    };
}

public record Person (string Name, AccessType Type);

public record OpenCaseFile (string Name, AccessType Type, int RiskLevel) : Person(Name, Type);

public record Mycroft (string Name, AccessType Type): Person(Name, Type)
{
    public string? CaresAbout { get; set; }
};

public record Content(string Name, ContentType Type);

public enum AccessType
{
    Child,
    Adult,
    PoorlyDefined,
    Monarch,
}

public enum ContentType
{
    ChildsPlay,
    Public,
    StateSecret,
    ForHerEyesOnly
}

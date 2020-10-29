using System;
using System.Linq;
using static System.Console;

var user = "Lion-O";
var password = "jaga";
var rememberMe = true;
LoginResourceRecord lrr1 = new(user, password, rememberMe);
var lrr2 = new LoginResourceRecord(user, password, rememberMe);
var lrc1 = new LoginResourceClass(user, password, rememberMe);
var lrc2 = new LoginResourceClass(user, password, rememberMe);

WriteLine($"Test record equality -- lrr1 == lrr2 : {lrr1 == lrr2}");
WriteLine($"Test class equality  -- lrc1 == lrc2 : {lrc1 == lrc2}");
WriteLine($"Print lrr1 hash code -- lrr1.GetHashCode(): {lrr1.GetHashCode()}");
WriteLine($"Print lrr2 hash code -- lrr2.GetHashCode(): {lrr2.GetHashCode()}");
WriteLine($"Print lrc1 hash code -- lrc1.GetHashCode(): {lrc1.GetHashCode()}");
WriteLine($"Print lrc2 hash code -- lrc2.GetHashCode(): {lrc2.GetHashCode()}");
WriteLine($"{nameof(LoginResourceRecord)} implements IEquatable<T>: {lrr1 is IEquatable<LoginResourceRecord>} ");
WriteLine($"{nameof(LoginResourceClass)}  implements IEquatable<T>: {lrr1 is IEquatable<LoginResourceClass>}");
WriteLine($"Print {nameof(LoginResourceRecord)}.ToString -- lrr1.ToString(): {lrr1.ToString()}");
WriteLine($"Print {nameof(LoginResourceClass)}.ToString  -- lrc1.ToString(): {lrc1.ToString()}");

public record LoginResourceRecord(string Username, string Password, bool RememberMe);

public class LoginResourceClass
{
    public LoginResourceClass(string username, string password, bool rememberMe)
    {
        Username = username;
        Password = password;
        RememberMe = rememberMe;
    }

    public string Username { get; init; }
    public string Password { get; init; }
    public bool RememberMe { get; init; }
}
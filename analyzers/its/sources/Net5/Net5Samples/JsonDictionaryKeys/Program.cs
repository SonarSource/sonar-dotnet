using System;
using System.Collections.Generic;
using System.Text.Json;

Dictionary<int, string> numbers = new ()
{
    {0, "zero"},
    {1, "one"},
    {2, "two"},
    {3, "three"},
    {5, "five"},
    {8, "eight"},
    {13, "thirteen"},
    {21, "twenty one"},
    {34, "thirty four"},
    {55, "fifty five"},
};

var json = JsonSerializer.Serialize<Dictionary<int, string>>(numbers);

Console.WriteLine(json);

var dictionary = JsonSerializer.Deserialize<Dictionary<int, string>>(json);

Console.WriteLine(dictionary[55]);
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

Employee janeEmployee = new()
{
    Name = "Jane Doe",
    YearsEmployed = 10
};

Employee johnEmployee = new()
{
    Name = "John Smith"
};

janeEmployee.Reports = new List<Employee> { johnEmployee };
johnEmployee.Manager = janeEmployee;

JsonSerializerOptions options = new()
{
    // NEW: globally ignore default values when writing null or default
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    // NEW: globally allow reading and writing numbers as JSON strings
    NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString,
    // NEW: globally support preserving object references when (de)serializing
    ReferenceHandler = ReferenceHandler.Preserve,
    IncludeFields = true, // NEW: globally include fields for (de)serialization
    WriteIndented = true,};

string serialized = JsonSerializer.Serialize(janeEmployee, options);
Console.WriteLine($"Jane serialized: {serialized}");

Employee janeDeserialized = JsonSerializer.Deserialize<Employee>(serialized, options);
Console.Write("Whether Jane's first report's manager is Jane: ");
Console.WriteLine(janeDeserialized.Reports[0].Manager == janeDeserialized);

public class Employee
{
    // NEW: Allows use of non-public property accessor.
    // Can also be used to include fields "per-field", rather than globally with JsonSerializerOptions.
    [JsonInclude]
    public string Name { get; internal set; }

    public Employee Manager { get; set; }

    public List<Employee> Reports;

    public int YearsEmployed { get; set; }

    // NEW: Always include when (de)serializing regardless of global options
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public bool IsManager => Reports?.Count > 0;
}

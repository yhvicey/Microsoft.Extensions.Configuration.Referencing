using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.Variables.json")
    .AddJsonFile("appsettings.json")
    .Build()
    .ResolveReferences();

Console.WriteLine($"Value of Key1: {configuration["Key1"]}");
Console.WriteLine($"Value of Key2: {configuration["Key2"]}");
Console.WriteLine($"Value of Key3: {configuration["Key3"]}");
Console.WriteLine($"Value of non-existing key Key4: {configuration["Key4"]}");
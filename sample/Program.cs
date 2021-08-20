using Microsoft.Extensions.Configuration;

var environment = args.FirstOrDefault();

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.Variables.json")
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .Build()
    .ResolveReferences();

Console.WriteLine($"Value of Key1: {configuration["Key1"]}");
Console.WriteLine($"Value of Key2: {configuration["Key2"]}");
Console.WriteLine($"Value of Key3: {configuration["Key3"]}");
Console.WriteLine($"Value of non-existing key Key4: {configuration["Key4"]}");
Console.WriteLine($"Value of key Key5 in environment {environment ?? "(empty)"}: {configuration["Key5"]}");
using Microsoft.Extensions.Configuration;

var environment = args.FirstOrDefault();

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.Variables.json")
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .Build()
    .ResolveReferences(); // This method call resolves all reference tokens in '$(PATH:TO:OTHER:CONFIG:VALUE)' format

Console.WriteLine($"Value of Key1: {configuration["Key1"]}"); // 123
Console.WriteLine($"Value of Key2: {configuration["Key2"]}"); // False
Console.WriteLine($"Value of Key3: {configuration["Key3"]}"); // 123,False
Console.WriteLine($"Value of non-existing key Key4: {configuration["Key4"]}"); // (empty)
Console.WriteLine($"Value of Key5 in environment {environment ?? "(empty)"}: {configuration["Key5"]}"); // "Prod" for Production, "Dev" for Development, "Local" for default case
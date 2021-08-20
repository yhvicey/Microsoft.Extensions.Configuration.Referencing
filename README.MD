# Microsoft.Extensions.Configuration.Referencing

[![Build Status](https://dev.azure.com/vicey/GithubProjectsCICD/_apis/build/status/yhvicey.Microsoft.Extensions.Configuration.Referencing?branchName=main)](https://dev.azure.com/vicey/GithubProjectsCICD/_build/latest?definitionId=11&branchName=main)

This packages allows users of `Microsoft.Extensions.Configuration.*` packages to reference config values within same configuration instance, e.g.:

```json
// appsettings.Variables.json
{
  "Variables": {
    "Value1": 123,
    "Value2": false
  }
}

// appsettings.json
{
  "Key1": "$(Variables:Value1)", // Will be resolved to 123
  "Key2": "$(Variables:Value2)", // Will be resolved to false
  "Key3": "$(Variables:Value1),$(Variables:Value2)", // Will be resolved to 123,False
  "Key4": "$(Variables:Value4)", // Will be resolved to null
  "Key5": "$(Variables:Value5,Local)" // Will be resolved to different values in different environments, with default value "Local"
}

// appsettings.Development.json
{
  "Variables": {
    "Value5": "Dev"
  }
}

// appsettings.Production.json
{
  "Variables": {
    "Value5": "Prod"
  }
}
```

References should be in format `$(PATH:TO:OTHER:CONFIG:VALUE)`, it will replace this token with value of `configuration["PATH:TO:OTHER:CONFIG:VALUE"]`.

If you'd like to provide a default value for it, append `,DefaultValue` to inner part: `$(PATH:TO:OTHER:CONFIG:VALUE,DefaultValue)`, it will lookup the value of `configuration["PATH:TO:OTHER:CONFIG"]` to replace the token, and if the value not exists, default value `DefaultValue` will be used instead.

Usage:

```csharp
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
```

# Microsoft.Extensions.Configuration.Referencing

[![Build Status](https://dev.azure.com/vicey/GithubProjectsCICD/_apis/build/status/yhvicey.Microsoft.Extensions.Configuration.Referencing?branchName=main)](https://dev.azure.com/vicey/GithubProjectsCICD/_build/latest?definitionId=11&branchName=main)

This packages allows users of `Microsoft.Extensions.Configuration.*` packages to reference config values within same configuration instance, e.g.:

```json
// appsettings.Variables.json
{
  "Variables": {
    "Value1": 123,
    "Value2": false,
    "Value3": "789"
  }
}

// appsettings.json
{
  // Will be resolved to 123
  "Key1": "$(Variables:Value1)",
  // Will be resolved to false
  "Key2": "$(Variables:Value2)",
  // Will be resolved to "789"
  "Key3": "$(Variables:Value3)",
  // Will be resolved to null
  "Key4": "$(Variables:Value4)"
}
```

Usage:

```csharp
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.Variables.json")
    .AddJsonFile("appsettings.json")
    .Build()
    .ResolveReferences(); // This method call resolves all reference tokens in '$(PATH:TO:CONFIG)' format

Console.WriteLine($"Value of Key1: {configuration["Key1"]}");
Console.WriteLine($"Value of Key2: {configuration["Key2"]}");
Console.WriteLine($"Value of Key3: {configuration["Key3"]}");
Console.WriteLine($"Value of non-existing key Key4: {configuration["Key4"]}");
```

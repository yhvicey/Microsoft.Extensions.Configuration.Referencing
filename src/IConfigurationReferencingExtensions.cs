using System;
using System.Text.RegularExpressions;

namespace Microsoft.Extensions.Configuration
{
    public static class IConfigurationReferencingExtensions
    {
        private static readonly Regex ReferenceMatchingPattern = new Regex(@"\$\(([\w:]+?)(,([^)]*))?\)", RegexOptions.Compiled);

        public static IConfiguration ResolveReferences(
            this IConfiguration configuration,
            Regex? matchingPattern = null,
            Func<Match, string>? configPathSelector = null,
            Func<Match, string>? defaultValueSelector = null)
        {
            matchingPattern ??= ReferenceMatchingPattern;
            configPathSelector ??= match => match.Groups[1].Value;
            defaultValueSelector ??= match => match.Groups[3].Value;

            foreach (var kvp in configuration.AsEnumerable())
            {
                if (kvp.Value == null)
                {
                    continue;
                }
                var replacedValue = matchingPattern.Replace(kvp.Value, match =>
                {
                    var configPath = configPathSelector(match);
                    var defaultValue = defaultValueSelector(match);
                    return configuration[configPath] ?? defaultValue;
                });
                try
                {
                    configuration[kvp.Key] = replacedValue;
                }
                catch (InvalidOperationException)
                {
                    if (configuration is IConfigurationRoot configurationRoot)
                    {
                        foreach (var provider in configurationRoot.Providers)
                        {
                            try
                            {
                                provider.Set(kvp.Key, replacedValue);
                            }
                            catch (InvalidOperationException)
                            {
                            }
                        }
                    }
                }
            }

            return configuration;
        }
    }
}
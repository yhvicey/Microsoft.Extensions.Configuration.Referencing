using System;
using System.Text.RegularExpressions;

namespace Microsoft.Extensions.Configuration
{
    public static class IConfigurationReferencingExtensions
    {
        private static readonly Regex ReferenceMatchingPattern = new Regex(@"\$\(([\w:]+?)\)", RegexOptions.Compiled);

        public static IConfiguration ResolveReferences(
            this IConfiguration configuration,
            Regex? matchingPattern = null,
            Func<Match, string>? configPathSelector = null)
        {
            matchingPattern ??= ReferenceMatchingPattern;
            configPathSelector ??= match => match.Groups[1].Value;
            foreach (var kvp in configuration.AsEnumerable())
            {
                if (kvp.Value == null)
                {
                    continue;
                }
                var match = matchingPattern.Match(kvp.Value);
                if (!match.Success)
                {
                    continue;
                }
                var configPath = configPathSelector(match);
                var configValue = configuration[configPath];
                try
                {
                    configuration[kvp.Key] = configValue;
                }
                catch (InvalidOperationException)
                {
                    if (configuration is IConfigurationRoot configurationRoot)
                    {
                        foreach (var provider in configurationRoot.Providers)
                        {
                            try
                            {
                                provider.Set(kvp.Key, configValue);
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
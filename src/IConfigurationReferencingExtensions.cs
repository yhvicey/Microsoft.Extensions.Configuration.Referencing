using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.Extensions.Configuration
{
    public static class IConfigurationReferencingExtensions
    {
        private static readonly Regex ReferenceMatchingPattern = new Regex(@"\$\(([\w:]+?)(,([^)]*))?\)", RegexOptions.Compiled);

        /// <summary>
        /// Maximum number of resolution passes to prevent infinite loops from circular references.
        /// </summary>
        private const int MaxResolutionPasses = 10;

        public static IConfiguration ResolveReferences(
            this IConfiguration configuration,
            Regex? matchingPattern = null,
            Func<Match, string>? configPathSelector = null,
            Func<Match, string>? defaultValueSelector = null)
        {
            matchingPattern ??= ReferenceMatchingPattern;
            configPathSelector ??= match => match.Groups[1].Value;
            defaultValueSelector ??= match => match.Groups[3].Value;

            // Multi-pass resolution: repeat until no unresolved references remain.
            // This handles chained references like C -> B -> A regardless of enumeration order.
            for (var pass = 0; pass < MaxResolutionPasses; pass++)
            {
                var hasUnresolvedReferences = false;

                foreach (var kvp in configuration.AsEnumerable())
                {
                    if (kvp.Value == null || !matchingPattern.IsMatch(kvp.Value))
                    {
                        continue;
                    }

                    var replacedValue = matchingPattern.Replace(kvp.Value, match =>
                    {
                        var configPath = configPathSelector(match);
                        var defaultValue = defaultValueSelector(match);
                        return configuration[configPath] ?? defaultValue;
                    });

                    // Check if any references remain unresolved after substitution
                    if (matchingPattern.IsMatch(replacedValue))
                    {
                        hasUnresolvedReferences = true;
                    }

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

                if (!hasUnresolvedReferences)
                {
                    break;
                }
            }

            return configuration;
        }
    }
}
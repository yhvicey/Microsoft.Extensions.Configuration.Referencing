using System;
using System.Text.RegularExpressions;

namespace Microsoft.Extensions.Configuration
{
    public static class IConfigurationReferencingExtensions
    {
        private static readonly Regex ReferenceMatchingPattern = new Regex(@"\$\(([\w:]+?)\)", RegexOptions.Compiled);

        public static IConfiguration ResolveReferences(this IConfiguration configuration)
        {
            foreach (var kvp in configuration.AsEnumerable())
            {
                if (kvp.Value == null)
                {
                    continue;
                }
                var match = ReferenceMatchingPattern.Match(kvp.Value);
                if (!match.Success)
                {
                    continue;
                }
                var configPath = match.Groups[1].Value;
                try
                {
                    configuration[kvp.Key] = configuration[configPath];
                }
                catch (InvalidOperationException)
                {
                    if (configuration is IConfigurationRoot configurationRoot)
                    {
                        foreach (var provider in configurationRoot.Providers)
                        {
                            try
                            {
                                provider.Set(kvp.Key, configuration[configPath]);
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
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Microsoft.Extensions.Configuration.Referencing.Tests
{
#pragma warning disable CS8620 // Nullable mismatch in Dictionary<string, string> vs Dictionary<string, string?>
    public class ResolveReferencesTests
    {
        private static IConfiguration BuildConfig(Dictionary<string, string?> data)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(data)
                .Build();
        }

        [Fact]
        public void SimpleReference_ResolvesCorrectly()
        {
            var config = BuildConfig(new Dictionary<string, string>
            {
                ["Database:Server"] = "myserver.database.windows.net",
                ["ConnectionString"] = "Server=$(Database:Server);Database=mydb",
            });

            config.ResolveReferences();

            config["ConnectionString"].Should().Be("Server=myserver.database.windows.net;Database=mydb");
        }

        [Fact]
        public void MultipleReferencesInOneValue_ResolveCorrectly()
        {
            var config = BuildConfig(new Dictionary<string, string>
            {
                ["Host"] = "myserver",
                ["Port"] = "5432",
                ["ConnectionString"] = "Host=$(Host);Port=$(Port)",
            });

            config.ResolveReferences();

            config["ConnectionString"].Should().Be("Host=myserver;Port=5432");
        }

        [Fact]
        public void ChainedReferences_ResolveCorrectly()
        {
            // C references B, B references A — requires multi-pass
            var config = BuildConfig(new Dictionary<string, string>
            {
                ["A"] = "hello",
                ["B"] = "$(A)-world",
                ["C"] = "$(B)!",
            });

            config.ResolveReferences();

            config["B"].Should().Be("hello-world");
            config["C"].Should().Be("hello-world!");
        }

        [Fact]
        public void NestedReferenceInValue_ResolvesCorrectly()
        {
            // ConnectionString contains $(InstrumentationKey) which is a config key.
            // Serilog arg references $(ConnectionString).
            // This is the real-world scenario that motivated multi-pass resolution.
            var config = BuildConfig(new Dictionary<string, string>
            {
                ["ApplicationInsights:InstrumentationKey"] = "abc-123",
                ["ApplicationInsights:ConnectionString"] = "InstrumentationKey=$(ApplicationInsights:InstrumentationKey);Endpoint=https://example.com",
                ["Serilog:WriteTo:0:Args:connectionString"] = "$(ApplicationInsights:ConnectionString)",
            });

            config.ResolveReferences();

            config["ApplicationInsights:ConnectionString"].Should().Be("InstrumentationKey=abc-123;Endpoint=https://example.com");
            config["Serilog:WriteTo:0:Args:connectionString"].Should().Be("InstrumentationKey=abc-123;Endpoint=https://example.com");
        }

        [Fact]
        public void MissingReference_ResolvesToDefaultValue()
        {
            var config = BuildConfig(new Dictionary<string, string>
            {
                ["Value"] = "prefix-$(Missing:Key,fallback)-suffix",
            });

            config.ResolveReferences();

            config["Value"].Should().Be("prefix-fallback-suffix");
        }

        [Fact]
        public void MissingReferenceWithoutDefault_ResolvesToEmptyString()
        {
            var config = BuildConfig(new Dictionary<string, string>
            {
                ["Value"] = "prefix-$(Missing:Key)-suffix",
            });

            config.ResolveReferences();

            config["Value"].Should().Be("prefix--suffix");
        }

        [Fact]
        public void NullValue_IsSkipped()
        {
            var config = BuildConfig(new Dictionary<string, string?>
            {
                ["Key"] = null,
                ["Other"] = "$(Key)",
            });

            config.ResolveReferences();

            // Key stays null, Other resolves to empty (missing reference)
            config["Key"].Should().BeNull();
            config["Other"].Should().Be("");
        }

        [Fact]
        public void NoReferences_ValueUnchanged()
        {
            var config = BuildConfig(new Dictionary<string, string>
            {
                ["Plain"] = "no references here",
            });

            config.ResolveReferences();

            config["Plain"].Should().Be("no references here");
        }

        [Fact]
        public void CircularReference_DoesNotInfiniteLoop()
        {
            // A references B, B references A — circular
            var config = BuildConfig(new Dictionary<string, string>
            {
                ["A"] = "$(B)",
                ["B"] = "$(A)",
            });

            // Should not throw or hang — limited by MaxResolutionPasses
            config.ResolveReferences();

            // Values will still contain unresolved references after max passes
            // but the method should return without hanging
        }

        [Fact]
        public void ThreeLevelChain_ResolvesCorrectly()
        {
            var config = BuildConfig(new Dictionary<string, string>
            {
                ["L1"] = "base",
                ["L2"] = "$(L1)-mid",
                ["L3"] = "$(L2)-end",
                ["L4"] = "$(L3)-final",
            });

            config.ResolveReferences();

            config["L2"].Should().Be("base-mid");
            config["L3"].Should().Be("base-mid-end");
            config["L4"].Should().Be("base-mid-end-final");
        }

        [Fact]
        public void SelfReference_DoesNotInfiniteLoop()
        {
            var config = BuildConfig(new Dictionary<string, string>
            {
                ["A"] = "$(A)",
            });

            // Should not hang
            config.ResolveReferences();
        }

        [Fact]
        public void ResolveReferences_ReturnsSameInstance()
        {
            var config = BuildConfig(new Dictionary<string, string>
            {
                ["Key"] = "value",
            });

            var result = config.ResolveReferences();

            result.Should().BeSameAs(config);
        }
    }
}

﻿namespace Nancy.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Nancy.Bootstrapper;
    using Nancy.Testing;
    using Nancy.Tests.xUnitExtensions;
    using Xunit;

    public class AutoThingsRegistrations : IRegistrations
    {
        public IEnumerable<TypeRegistration> TypeRegistrations
        {
            get
            {
                ThrowWhenNoAppStartupsFixtureRuns();

                return Enumerable.Empty<TypeRegistration>();
            }
        }

        public IEnumerable<CollectionTypeRegistration> CollectionTypeRegistrations
        {
            get
            {
                ThrowWhenNoAppStartupsFixtureRuns();

                return Enumerable.Empty<CollectionTypeRegistration>();
            }
        }

        public IEnumerable<InstanceRegistration> InstanceRegistrations
        {
            get
            {
                ThrowWhenNoAppStartupsFixtureRuns();

                return Enumerable.Empty<InstanceRegistration>();
            }
        }

        private static void ThrowWhenNoAppStartupsFixtureRuns()
        {
            if ( Environment.StackTrace.Contains(typeof(NoAppStartupsFixture).FullName))
            {
                throw new Exception();
            }
        }
    }

    public class NoAppStartupsFixture
    {
        [Fact]
        public async Task When_AutoRegistration_Is_Enabled_Should_Throw()
        {
            var ex = await RecordAsync.Exception(async () =>
            {
                // Given
                var bootstrapper = new ConfigurableBootstrapper(config =>
                {
                    config.Module<NoAppStartupsModule>();
                    config.Dependency<INoAppStartupsTestDependency>(typeof(AutoDependency));
                });
                var browser = new Browser(bootstrapper);

                // When
                await browser.Get("/");
            });

            //Then
            ex.ShouldNotBeNull();
        }

        [Fact]
        public async Task When_AutoRegistration_Is_Disabled_Should_Not_Throw()
        {
            // Given
            var bootstrapper = new ConfigurableBootstrapper(config =>
            {
                config.DisableAutoRegistrations();
                config.Module<NoAppStartupsModule>();
                config.Dependency<INoAppStartupsTestDependency>(typeof(AutoDependency));
            });
            var browser = new Browser(bootstrapper);

            // When
            var result = await browser.Get("/");

            // Then
            result.Body.AsString().ShouldEqual("disabled auto registration works");
        }

        public interface INoAppStartupsTestDependency
        {
            string GetStuff();
        }

        public class AutoDependency : INoAppStartupsTestDependency
        {
            public string GetStuff()
            {
                return "disabled auto registration works";
            }
        }

        public class NoAppStartupsModule : NancyModule
        {
            public NoAppStartupsModule(INoAppStartupsTestDependency dependency)
            {
                Get("/", args => dependency.GetStuff());
            }
        }
    }
}
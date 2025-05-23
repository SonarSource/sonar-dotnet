﻿using System.Linq;
using Xunit;
using Shouldly;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using System.Data.Entity;

namespace AutoMapper.UnitTests.Projection
{
    public class CustomProjectionStringToString : AutoMapperSpecBase
    {
        public class TestContext : DbContext
        {
            public TestContext(): base() => Database.SetInitializer<TestContext>(new DatabaseInitializer());
            public DbSet<Source> Sources { get; set; }
        }
        const string _badGreeting = "GRRRRR";
        public class DatabaseInitializer : DropCreateDatabaseAlways<TestContext>
        {
            protected override void Seed(TestContext testContext)
            {
                testContext.Sources.Add(new Source { Greeting = _badGreeting });
                base.Seed(testContext);
            }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(x => {
            x.CreateProjection<string, string>().ConvertUsing(s => _niceGreeting);
            x.CreateProjection<Source, Target>();
            x.CreateProjection<SourceChild, TargetChild>();
        });
        const string _niceGreeting = "Hello";
        [Fact]
        public void Direct_assignability_shouldnt_trump_custom_projection()
        {
            using (var context = new TestContext())
            {
                ProjectTo<Target>(context.Sources).Single().Greeting.ShouldBe(_niceGreeting);
            }
        }
        public class Source
        {
            public int Id { get; set; }
            public string Greeting { get; set; }
            public int Number { get; set; }
            public SourceChild Child { get; set; }
        }
        public class SourceChild
        {
            public int Id { get; set; }
            public string Greeting { get; set; }
        }
        class Target
        {
            public string Greeting { get; set; }
            public int? Number { get; set; }
            public TargetChild Child { get; set; }
        }
        class TargetChild
        {
            public string Greeting { get; set; }
        }
    }
    public class CustomProjectionCustomClasses : AutoMapperSpecBase
    {
        public class TestContext : DbContext
        {
            public TestContext() : base() => Database.SetInitializer<TestContext>(new DatabaseInitializer());
            public DbSet<Source> Sources { get; set; }
        }
        public class DatabaseInitializer : DropCreateDatabaseAlways<TestContext>
        {
            protected override void Seed(TestContext testContext)
            {
                testContext.Sources.Add(new Source());
                base.Seed(testContext);
            }
        }
        const string _niceGreeting = "Hello";
        protected override MapperConfiguration Configuration => new MapperConfiguration(x =>
        {
            x.CreateProjection<Source, Target>().ConvertUsing(s => new Target { Greeting = _niceGreeting });
            x.CreateProjection<SourceChild, TargetChild>();
        });
        [Fact]
        public void Should_work()
        {
            using (var context = new TestContext())
            {
                ProjectTo<Target>(context.Sources).Single().Greeting.ShouldBe(_niceGreeting);
            }
        }
        public class Source
        {
            public int Id { get; set; }
            public string Greeting { get; set; }
            public int Number { get; set; }
            public SourceChild Child { get; set; }
        }
        public class SourceChild
        {
            public int Id { get; set; }
            public string Greeting { get; set; }
        }
        class Target
        {
            public string Greeting { get; set; }
            public int? Number { get; set; }
            public TargetChild Child { get; set; }
        }
        class TargetChild
        {
            public string Greeting { get; set; }
        }
    }
    public class CustomProjectionChildClasses : AutoMapperSpecBase
    {
        public class TestContext : DbContext
        {
            public TestContext() : base() => Database.SetInitializer<TestContext>(new DatabaseInitializer());
            public DbSet<Source> Sources { get; set; }
        }
        public class DatabaseInitializer : DropCreateDatabaseAlways<TestContext>
        {
            protected override void Seed(TestContext testContext)
            {
                testContext.Sources.Add(new Source { Child = new SourceChild { } });
                base.Seed(testContext);
            }
        }
        const string _niceGreeting = "Hello";
        protected override MapperConfiguration Configuration => new MapperConfiguration(x =>
        {
            x.CreateProjection<Source, Target>();
            x.CreateProjection<SourceChild, TargetChild>().ConvertUsing(s => new TargetChild { Greeting = _niceGreeting });
        });
        [Fact]
        public void Should_work()
        {
            using (var context = new TestContext())
            {
                ProjectTo<Target>(context.Sources).Single().Child.Greeting.ShouldBe(_niceGreeting);
            }
        }
        public class Source
        {
            public int Id { get; set; }
            public string Greeting { get; set; }
            public int Number { get; set; }
            public SourceChild Child { get; set; }
        }
        public class SourceChild
        {
            public int Id { get; set; }
            public string Greeting { get; set; }
        }
        class Target
        {
            public string Greeting { get; set; }
            public int? Number { get; set; }
            public TargetChild Child { get; set; }
        }
        class TargetChild
        {
            public string Greeting { get; set; }
        }
    }
}
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CSharp9
{
    record Record
    {
        private string name = "csharp9"; // Noncompliant {{Define a constant instead of using this literal 'csharp9' 11 times.}}
        //                    ^^^^^^^^^

        public static readonly string NameReadonly = "csharp9";
        //                                           ^^^^^^^^^ Secondary

        string Name { get; } = "csharp9";
        //                     ^^^^^^^^^ Secondary

        void Method()
        {
            var x = "csharp9";
            //      ^^^^^^^^^ Secondary

            void NestedMethod()
            {
                var y = "csharp9";
                //      ^^^^^^^^^ Secondary
            }
        }

        [DebuggerDisplay("csharp9", Name = "csharp9", TargetTypeName = "csharp9")] // Compliant - in attribute -> ignored
        record InnerRecord
        {
            private string name = "csharp9";
            //                    ^^^^^^^^^ Secondary

            public static readonly string NameReadonly = "csharp9";
            //                                           ^^^^^^^^^ Secondary

            string Name { get; } = "csharp9";
            //                     ^^^^^^^^^ Secondary

            void Method()
            {
                var x = "csharp9";
                //      ^^^^^^^^^ Secondary

                [Conditional("DEBUG")] // Compliant - in attribute -> ignored
                static void NestedMethod()
                {
                    var y = "csharp9";
                    //      ^^^^^^^^^ Secondary
                }
            }
        }

        record PositionalRecord(string Name)
        {
            private string name = "csharp9";
            //                    ^^^^^^^^^ Secondary
        }
    }
}

namespace CSharp10
{
    record struct RecordStruct
    {
        public RecordStruct() { }

        private string name = "csharp10"; // Noncompliant

        public static readonly string NameReadonly = "csharp10";
        //                                           ^^^^^^^^^^ Secondary

        string Name { get; } = "csharp10";
        //                     ^^^^^^^^^^ Secondary

        void Method()
        {
            var x = "csharp10";
            //      ^^^^^^^^^^ Secondary
            void NestedMethod()
            {
                var y = "csharp10";
                //      ^^^^^^^^^^ Secondary
            }
        }

        [DebuggerDisplay("csharp10", Name = "csharp10", TargetTypeName = "csharp10")] // Compliant - in attribute -> ignored
        record struct InnerRecordStruct
        {
            public InnerRecordStruct() { }

            private string name = "csharp10";
            //                    ^^^^^^^^^^ Secondary

            public static readonly string NameReadonly = "csharp10"; // Secondary

            string Name { get; } = "csharp10"; // Secondary

            void Method()
            {
                var x = "csharp10"; // Secondary

                [Conditional("foobar")] // Compliant - in attribute -> ignored
                static void NestedMethod()
                {
                    var y = "csharp10"; // Secondary
                }
            }
        }

        record struct PositionalRecordStruct(string Name)
        {
            private string name = "csharp10";
            //                    ^^^^^^^^^^ Secondary
        }
    }
}

namespace CSharp11
{
    public class FooNonCompliant
    {
        private string NameOne = """csharp11"""; // Noncompliant {{Define a constant instead of using this literal '""csharp11""' 4 times.}}

        private string NameTwo = """csharp11"""; // Secondary

        public const string NameConst = """csharp11"""; // Secondary

        public static readonly string NameReadonly = """csharp11"""; // Secondary

    }

    public class FooLessThanFiveCharacters
    {
        private string NameOne = """fo"""; // Compliant (less than 5 characters)

        private string NameTwo = """fo""";

        public const string NameConst = """fo""";

        public static readonly string NameReadonly = """fo""";
    }

    public class FooNonCompliantStringInterpolation
    {
        public string NameOne = $"{"BarBar" // Noncompliant {{Define a constant instead of using this literal 'BarBar' 4 times.}}
            }";

        public string NameTwo = $"{"BarBar" // Secondary
            }";

        public static string NameThree = "BarBar"; // Secondary

        public static readonly string NameReadonly = $"{"BarBar"}"; // Secondary

    }
}

// https://sonarsource.atlassian.net/browse/NET-2276
public class EfCoreMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_CustomerOrders_OldStatus",
            table: "CustomerOrders");

        migrationBuilder.RenameColumn(
            name: "OldStatus",
            table: "CustomerOrders",
            newName: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_CustomerOrders_Status",
            table: "CustomerOrders",
            column: "Status");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_CustomerOrders_Status",
            table: "CustomerOrders");

        migrationBuilder.RenameColumn(
            name: "Status",
            table: "CustomerOrders",
            newName: "OldStatus");

        migrationBuilder.CreateIndex(
            name: "IX_CustomerOrders_OldStatus",
            table: "CustomerOrders",
            column: "OldStatus");
    }
}

// https://sonarsource.atlassian.net/browse/NET-3281
namespace EntityFrameworkModelConfiguration
{
    public class Entity { }

    public class Context : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var columnNames = new[] { "ColumnName", "ColumnName", "ColumnName", "ColumnName" }; // Compliant - model configuration
            var sharedNames = new[] { "SharedName", "SharedName" }; // Compliant - model configuration

            modelBuilder.Entity<Entity>(builder =>
            {
                var lambdaColumnNames = new[] { "LambdaColumn", "LambdaColumn", "LambdaColumn", "LambdaColumn" }; // Compliant - model configuration
            });

            void ConfigureEntity()
            {
                var localFunctionColumnNames = new[] { "LocalColumn", "LocalColumn", "LocalColumn", "LocalColumn" }; // Compliant - model configuration
            }
        }

        public void OnModelCreating()
        {
            var values = new[]
            {
                "NotAnOverride", // Noncompliant {{Define a constant instead of using this literal 'NotAnOverride' 4 times.}}
                "NotAnOverride", // Secondary
                "NotAnOverride", // Secondary
                "NotAnOverride"  // Secondary
            };
        }

        public void Other()
        {
            var values = new[]
            {
                "SharedName", // Noncompliant {{Define a constant instead of using this literal 'SharedName' 4 times.}}
                "SharedName", // Secondary
                "SharedName", // Secondary
                "SharedName"  // Secondary
            };
        }
    }

    public abstract class BaseAppContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder) { }
    }

    public class DerivedAppContext : BaseAppContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var columnNames = new[] { "InheritedColumn", "InheritedColumn", "InheritedColumn", "InheritedColumn" }; // Compliant - model configuration
        }
    }

    public class EntityConfiguration : IEntityTypeConfiguration<Entity>
    {
        public void Configure(EntityTypeBuilder<Entity> builder)
        {
            var columnNames = new[] { "ColumnName", "ColumnName", "ColumnName", "ColumnName" }; // Compliant - model configuration
        }

        public void Configure(string value)
        {
            var values = new[]
            {
                "NotInterfaceImplementation", // Noncompliant {{Define a constant instead of using this literal 'NotInterfaceImplementation' 4 times.}}
                "NotInterfaceImplementation", // Secondary
                "NotInterfaceImplementation", // Secondary
                "NotInterfaceImplementation"  // Secondary
            };
        }
    }

    public class ExplicitEntityConfiguration : IEntityTypeConfiguration<Entity>
    {
        void IEntityTypeConfiguration<Entity>.Configure(EntityTypeBuilder<Entity> builder)
        {
            var columnNames = new[] { "ExplicitColumn", "ExplicitColumn", "ExplicitColumn", "ExplicitColumn" }; // Compliant - model configuration
        }
    }

    public abstract class NotADbContext
    {
        protected abstract void OnModelCreating(object modelBuilder);
    }

    public class FakeContext : NotADbContext
    {
        protected override void OnModelCreating(object modelBuilder)
        {
            var values = new[]
            {
                "NotADbContext", // Noncompliant {{Define a constant instead of using this literal 'NotADbContext' 4 times.}}
                "NotADbContext", // Secondary
                "NotADbContext", // Secondary
                "NotADbContext"  // Secondary
            };
        }
    }

    public abstract class DbContextWithCustomOnModelCreating : DbContext
    {
        protected virtual void OnModelCreating(string value) { }
    }

    public class CustomOnModelCreatingContext : DbContextWithCustomOnModelCreating
    {
        protected override void OnModelCreating(string value)
        {
            var values = new[]
            {
                "CustomOverload", // Noncompliant {{Define a constant instead of using this literal 'CustomOverload' 4 times.}}
                "CustomOverload", // Secondary
                "CustomOverload", // Secondary
                "CustomOverload"  // Secondary
            };
        }
    }

    public interface IStartupThing
    {
        void Configure(string value);
    }

    public class NotEfConfiguration : IStartupThing
    {
        public void Configure(string value)
        {
            var values = new[]
            {
                "NotEfConfigure", // Noncompliant {{Define a constant instead of using this literal 'NotEfConfigure' 4 times.}}
                "NotEfConfigure", // Secondary
                "NotEfConfigure", // Secondary
                "NotEfConfigure"  // Secondary
            };
        }
    }
}

namespace CSharp13
{
    class EscapeSequence
    {
        private string backslash = "Filename\u001B" // Noncompliant
                + "Filename\e"                      // Secondary
                + "Filename\u001b"                  // Secondary
                + "Filename\e";                     // Secondary
    }

    partial class PartialClass
    {
        private string some = "csharp13";           // FN NET-3597
        public partial string Hello => "csharp13";
        public partial string World { get; }
    }

    partial class PartialClass
    {
        private const string name = "csharp13";
        public partial string Hello { get; }
        public partial string World => "csharp13";
    }
}

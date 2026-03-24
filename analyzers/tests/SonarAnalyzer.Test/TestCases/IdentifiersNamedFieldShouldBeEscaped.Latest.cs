// The structure of this file must mirror IdentifiersNamedFieldShouldBeEscaped.CSharp9-13.cs.
// Both files must be kept in sync — add new test cases to both files.
// Annotations differ only where C# 14 emits compiler diagnostics instead of S8367:
//   Local variable declarations named 'field': CS9273 fires here; S8367 fires in CSharp9-13.
//   Field references to 'field' in accessors:   CS9258 fires here; S8367 fires in CSharp9-13.
using System;

public class Node { public Node field; public Node a; }
public class S { public static Node field; }

namespace Noncompliant
{
    public class LocalVariableInAccessors
    {
        public int Value
        {
            get
            {
                int field = 0; // Error [CS9273]
                return field; // Error [CS9258]
            }
            set
            {
                int field = value; // Error [CS9273]
            }
        }
    }

    public class LocalVariableInInit
    {
        public int Value
        {
            get => 0;
            init
            {
                int field = value; // Error [CS9273]
            }
        }
    }

    public class ClassFieldReferencedInAccessor
    {
        private int field;

        public int Value
        {
            get => field; // Error [CS9258]
            set => field = value; // Error [CS9258]
        }

        public int ExpressionBodied => field; // Error [CS9258]
    }

    public class BaseClassFieldReferencedInDerivedAccessor
    {
        protected int field;
    }

    public class DerivedClassFieldReferencedInAccessor : BaseClassFieldReferencedInDerivedAccessor
    {
        public int Value
        {
            get => field; // Error [CS9258]
            set => field = value; // Error [CS9258]
        }

        public int ExpressionBodied => field; // Error [CS9258]
    }

    public class ParameterNamedFieldInAccessor
    {
        public int StaticLocalFunction
        {
            get
            {
                static int LocalFunction(int field) // Error [CS9273]
                    => 0;
                return 0;
            }
        }

        public int LocalFunction
        {
            get
            {
                int LocalFunction(int field) // Error [CS9273]
                {
                    return field; // Error [CS9258]
                }
                return 0;
            }
        }

        public int Lambda
        {
            get
            {
                Func<int, int> f = (field) // Error [CS9273]
                    => field; // Error [CS9258]
                return 0;
            }
        }

        public int AnonymousDelegate
        {
            get
            {
                Func<int, int> f = delegate(int field) // Error [CS9273]
                {
                    return field; // Error [CS9258]
                };
                return 0;
            }
        }
    }

    public class MemberReferenceInAccessor
    {
        private Node field;

        public Node Value
        {
            get
            {
                _ = field.a;   // Error [CS9258]
                _ = field?.a;  // Error [CS9258]
                _ = field!.a;  // Error [CS9258]
                return default;
            }
        }
    }

    public class IndexerAccessInAccessor
    {
        private int[] field;

        public int[] Value
        {
            get
            {
                _ = field[0];   // Error [CS9258]
                _ = field?[0];  // Error [CS9258]
                return null;
            }
        }
    }

    public class ExpressionBodiedLambdaParameter
    {
        public System.Func<int, int> Value => (int field) => 0; // Error [CS9273]
    }

    namespace StaticTypeNamedField
    {
        public static class field { public static int i; }

        public class AccessViaTypeName
        {
            public int Value
            {
                get
                {
                    _ = field  // Error [CS9258]
                        .i;    // Error [CS1061]
                    return 0;
                }
            }
        }
    }

    public class ClassMethodReferencedInAccessor
    {
        private int field() => 42;

        public int Value
        {
            get => field(); // Error [CS9258, CS0149]
        }
    }

    public class ClassEventReferencedInAccessor
    {
        private event Action field;

        public int Value
        {
            get
            {
                field?.Invoke(); // Error [CS9258, CS0023]
                return 0;
            }
        }
    }

    namespace NamespaceNamedField
    {
        namespace field
        {
            public class C { public static int i; }
        }

        public class AccessViaNamespaceName
        {
            public int Value
            {
                get
                {
                    _ = field  // Error [CS9258]
                        .C.i;  // Error [CS1061]
                    return 0;
                }
            }
        }
    }

    public class TypeParameterReferencedInAccessor<field>
    {
        public Type Value
        {
            get => typeof(field);
        }
    }
}

namespace Compliant
{
    public class LocalVariableInAccessors
    {
        public int Value
        {
            get
            {
                int @field = 0;
                return @field;
            }
            set
            {
                int @field = value;
            }
        }
    }

    public class LocalVariableInInit
    {
        public int Value
        {
            get => 0;
            init
            {
                int @field = value;
            }
        }
    }

    public class ClassFieldReferencedInAccessor
    {
        private int field;

        public int Value
        {
            get => this.field;
            set => this.field = value;
        }

        public int ExpressionBodied => this.field;
    }

    public class BaseClassFieldReferencedInDerivedAccessor
    {
        protected int field;
    }

    public class DerivedClassFieldReferencedInAccessor : BaseClassFieldReferencedInDerivedAccessor
    {
        public int Value
        {
            get => base.field;
            set => base.field = value;
        }

        public int ExpressionBodied => base.field;
    }

    public class NonPropertyAccessors
    {
        private int field;

        public int this[int i]
        {
            get => field;
            set => field = value;
        }

        public event Action E
        {
            add { field++; }
            remove { field--; }
        }
    }

    public class ParameterNamedFieldInAccessor
    {
        public int StaticLocalFunction
        {
            get
            {
                static int LocalFunction(int @field)
                    => 0;
                return 0;
            }
        }

        public int LocalFunction
        {
            get
            {
                int LocalFunction(int @field)
                {
                    return @field;
                }
                return 0;
            }
        }

        public int Lambda
        {
            get
            {
                Func<int, int> f = (@field)
                    => @field;
                return 0;
            }
        }

        public int AnonymousDelegate
        {
            get
            {
                Func<int, int> f = delegate(int @field)
                {
                    return @field;
                };
                return 0;
            }
        }
    }

    public class MemberReferenceInAccessor
    {
        private Node field;
        Node x;
        Node[] xs;

        public Node Value
        {
            get
            {
                _ = x.field;
                _ = x?.field;
                _ = xs[0].field;
                _ = xs[0]?.field;
                _ = S.field;
                _ = x.a.a.field;
                _ = x.field.a;
                _ = x.field.a.field.a;
                return default;
            }
        }
    }

    public class IndexerAccessInAccessor
    {
        private int[] field;

        public int Value
        {
            get
            {
                _ = this.field[0];
                _ = this.field?[0];
                return 0;
            }
        }
    }

    public class ClassMethodReferencedInAccessor
    {
        private int field() => 42;

        public int Value
        {
            get => this.field(); // Compliant - qualified
        }
    }

    public class ClassEventReferencedInAccessor
    {
        private event Action field;

        public int Value
        {
            get
            {
                this.field?.Invoke(); // Compliant - qualified
                return 0;
            }
        }
    }

    namespace NamespaceNamedField
    {
        namespace field
        {
            public class C { public static int i; }
        }

        public class AccessViaNamespaceName
        {
            public int Value
            {
                get
                {
                    _ = @field.C.i;
                    return 0;
                }
            }
        }
    }

    public class TypeParameterReferencedInAccessor<field>
    {
        public Type Value
        {
            get => typeof(@field);
        }
    }

}

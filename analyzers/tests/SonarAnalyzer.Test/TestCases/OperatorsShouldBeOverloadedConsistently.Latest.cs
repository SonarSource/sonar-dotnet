public record Compliant // Compliant as == operator cannot be overloaded for records.
{
    public static object operator +(Compliant a, Compliant b) => new object();
    public static object operator -(Compliant a, Compliant b) => new object();
    public static object operator *(Compliant a, Compliant b) => new object();
    public static object operator /(Compliant a, Compliant b) => new object();
    public static object operator %(Compliant a, Compliant b) => new object();
}

public record OnlyPlus // Compliant
{
    public static object operator +(OnlyPlus a, OnlyPlus b) => new object();
}

public record OnlyMinus // Compliant
{
    public static object operator -(OnlyMinus a, OnlyMinus b) => new object();
}

public record OnlyMultiply // Compliant
{
    public static object operator -(OnlyMultiply a, OnlyMultiply b) => new object();
}

public record OnlyDevide // Compliant
{
    public static object operator -(OnlyDevide a, OnlyDevide b) => new object();
}

public record OnlyReminder // Compliant
{
    public static object operator -(OnlyReminder a, OnlyReminder b) => new object();
}

// Issues are not raised in Interface implementations.
// Interfaces are used to describe the operators supported by a type in a fine grained way
public interface IPlus<TSelf> where TSelf : IPlus<TSelf>
{
    static virtual TSelf operator +(TSelf a, TSelf b) => a;
}
public interface IMinus<TSelf> where TSelf : IMinus<TSelf>
{
    static abstract TSelf operator -(TSelf a, TSelf b);
}
public interface IMulti
{
    static IMulti operator *(IMulti a, IMulti b) => a;
}

public class OperatorsInExtensionNoEqualsOperator // FN https://sonarsource.atlassian.net/browse/NET-2774

{
    public int left;
    public int right;

    public OperatorsInExtensionNoEqualsOperator(int l, int r)
    {
        this.left = l;
        this.right = r;
    }
}

public class EqualsMethodsInExtensionsEqualsOperatorInClass // Noncompliant
{
    public static object operator ==(EqualsMethodsInExtensionsEqualsOperatorInClass a, EqualsMethodsInExtensionsEqualsOperatorInClass b) => new object();
    public static object operator !=(EqualsMethodsInExtensionsEqualsOperatorInClass a, EqualsMethodsInExtensionsEqualsOperatorInClass b) => new object();
}

public class EqualsOperatorsInExtensionsEqualsMethodsInClass // FN https://sonarsource.atlassian.net/browse/NET-2774
{
    public override bool Equals(object obj) => false;
    public override int GetHashCode() => 0;
}

public class SingleOperatorsInClassEqualsOperatorsInExtensions // Noncompliant FP https://sonarsource.atlassian.net/browse/NET-2774
{
    public static object operator +(SingleOperatorsInClassEqualsOperatorsInExtensions a, SingleOperatorsInClassEqualsOperatorsInExtensions b) => new object();
    public static object operator -(SingleOperatorsInClassEqualsOperatorsInExtensions a, SingleOperatorsInClassEqualsOperatorsInExtensions b) => new object();

    public override bool Equals(object obj) => false;
    public override int GetHashCode() => 0;
}

public class EqualsOperatorsInClassSingleOperatorsInExtensions // Noncompliant
{
    public static object operator ==(EqualsOperatorsInClassSingleOperatorsInExtensions a, EqualsOperatorsInClassSingleOperatorsInExtensions b) => new object();
    public static object operator !=(EqualsOperatorsInClassSingleOperatorsInExtensions a, EqualsOperatorsInClassSingleOperatorsInExtensions b) => new object();
}


public static class Extensions
{
    extension(OperatorsInExtensionNoEqualsOperator)
    {
        public static object operator +(OperatorsInExtensionNoEqualsOperator a, OperatorsInExtensionNoEqualsOperator b) => new object();
        public static object operator -(OperatorsInExtensionNoEqualsOperator a, OperatorsInExtensionNoEqualsOperator b) => new object();
    }

    extension(EqualsMethodsInExtensionsEqualsOperatorInClass)
    {
        public static bool Equals(object obj) => false;
        public static int GetHashCode() => 0;
    }

    extension(EqualsOperatorsInExtensionsEqualsMethodsInClass)
    {
        public static object operator ==(EqualsOperatorsInExtensionsEqualsMethodsInClass a, EqualsOperatorsInExtensionsEqualsMethodsInClass b) => new object();
        public static object operator !=(EqualsOperatorsInExtensionsEqualsMethodsInClass a, EqualsOperatorsInExtensionsEqualsMethodsInClass b) => new object();
    }

    extension(SingleOperatorsInClassEqualsOperatorsInExtensions)
    {
        public static object operator ==(SingleOperatorsInClassEqualsOperatorsInExtensions a, SingleOperatorsInClassEqualsOperatorsInExtensions b) => new object();
        public static object operator !=(SingleOperatorsInClassEqualsOperatorsInExtensions a, SingleOperatorsInClassEqualsOperatorsInExtensions b) => new object();
    }

    extension(EqualsOperatorsInClassSingleOperatorsInExtensions)
    {
        public static object operator +(EqualsOperatorsInClassSingleOperatorsInExtensions a, EqualsOperatorsInClassSingleOperatorsInExtensions b) => new object();
        public static object operator -(EqualsOperatorsInClassSingleOperatorsInExtensions a, EqualsOperatorsInClassSingleOperatorsInExtensions b) => new object();
    }
}

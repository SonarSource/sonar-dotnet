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

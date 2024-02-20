namespace CSharpLatest.CSharp10Features;

internal class AllowAsyncMethodBuilderAttributeOnMethods
{
    [AsyncMethodBuilder(builderType: typeof(RecordClass))]
    public async void SomeMethod()
    {
    }
}

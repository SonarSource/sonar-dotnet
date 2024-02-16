namespace CSharpLatest.CSharp10
{
    internal class AllowAsyncMethodBuilderAttributeOnMethods
    {
        [AsyncMethodBuilder(builderType: typeof(RecordClass))]
        public async void SomeMethod()
        {
        }
    }
}

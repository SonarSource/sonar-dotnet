namespace Net6
{
    internal class AllowAsyncMethodBuilderAttributeOnMethods
    {
        [AsyncMethodBuilder(builderType: typeof(RecordClass))]
        public async void SomeMethod()
        {
        }
    }
}

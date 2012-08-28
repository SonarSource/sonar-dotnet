public class Test
{
	public static readonly string TestCaseBuilderAttributeName = typeof(TestCaseBuilderAttribute).FullName;

	private void test()
	{
		// The following comment must not be used for CPD
		string message = "Hello World";
	}
}

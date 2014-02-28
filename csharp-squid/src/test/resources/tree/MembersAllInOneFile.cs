public class Test
{

	// ========== Constructor ==========
	public Test() 
	{
		string message = "";
	}


	// ========== Static Constructor ==========
	static Test() 
	{
		string message = "";
	}


	// ========== Finalizer ==========
	~ Test() 
	{
		string message = "";
	}


	// ========== Properties ==========
    public int Amount
    {
      get { return fAmount; }
    }

    public String Currency
    {
      get { return fCurrency; }
      set { fCurrency = value; }
    }

    public String Name { get; set; }


	// ========== Indexers ==========
	public int this[int index]
	{
		get {
		  return 0;
		}
		set;
	}
	

	// ========== Events ==========
	public event Action E1
	{
		add { value = value; }
		remove { E += Handler; E -= Handler; }
	}
	

	// ========== Methods ==========
	private void Foo()
	{
		string message = "Hello World";
	}
	
	private void Bar(String aParameter)
	{
		string message = aParameter;
	}
	

	// ========== Operator ==========
	public static A operator +(A first, A second)
	{
		string message = "";
	}
	
}

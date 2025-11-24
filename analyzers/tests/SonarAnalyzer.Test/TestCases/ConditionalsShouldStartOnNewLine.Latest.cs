using System;

class NullConditionalAssignmnet
{
    class MyClass
    {
        public bool Valid { get; set; }
    }

    public void TestMethod()
    {
        var obj = new MyClass();

        if(true)
        {
        } obj?.Valid = true;
    }
}

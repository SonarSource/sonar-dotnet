namespace CSharpLatest.CSharp10
{
    internal class LambdaAttributes
    {
        public void Example()
        {
            //Lambda Attributes 
            Action a =[MyAttribute<int>("asd")] () => { Console.WriteLine("Hello world."); };
        }
    }
}

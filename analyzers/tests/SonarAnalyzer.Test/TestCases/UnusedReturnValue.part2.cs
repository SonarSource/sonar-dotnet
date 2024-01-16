namespace SonarCheck
{
    public partial class Partial
    {
        void CallGetValue1()
        {
            new CallbackUser(myVal => GetValue1());
        }

        void CallGetValue2()
        {
            GetValue2();
        }
    }
}

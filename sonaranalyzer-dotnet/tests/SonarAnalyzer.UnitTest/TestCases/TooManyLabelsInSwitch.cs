namespace Tests.Diagnostics
{
    public class TooManyLabelsInSwitch
    {
        public enum MyEnum
        {
            A,
            B,
            C,
            D
        }

        public TooManyLabelsInSwitch(int n, MyEnum en)
        {
            switch (n)
            {
                case 0:
                    break;
                default:
                    break;
            }

            switch (n)
            {
                case 0:
                case 1:
                    break;
                default:
                    break;
            }

            switch (en)
            {
                case MyEnum.A:
                    break;
                case MyEnum.B:
                    break;
                case MyEnum.C:
                    break;
                case MyEnum.D:
                    break;
                default:
                    break;
            }

            switch (n) // Noncompliant {{Consider reworking this 'switch' to reduce the number of 'case's from 3 to at most 2.}}
//          ^^^^^^
            {
                case 0:
                case 1:
                    break;
                case 2:
                    break;
                default:
                    break;
            }
        }
    }
}

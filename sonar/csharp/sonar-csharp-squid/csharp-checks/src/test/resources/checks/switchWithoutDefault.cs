using System;

class Program
{
    static void Main(string[] args)
    {
        switch (0) // Non-Compliant
        {
        }

        switch (0) // Non-Compliant
        {
            case 0:
                break;
            case 1:
                break;
        }

        switch (0) // Compliant
        {
            default:
                break;
        }

        switch (0) // Compliant
        {
            default:
            case 0:
            case 1:
                break;
        }

        switch (0) // Compliant
        {
            default:
            default:
            case 0:
            case 1:
                break;
        }

        switch (0) // Compliant
        {
            case 0:
                break;
            case 1:
            case 2:
            default:
            case 3:
                break;
            case 4:
                break;
        }
    }
}

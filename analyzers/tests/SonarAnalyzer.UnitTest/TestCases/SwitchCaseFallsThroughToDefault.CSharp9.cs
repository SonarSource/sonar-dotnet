int x = int.Parse(args[0]);
object y = args[1];

switch (x)
{
    case > 1 and < 5: // Noncompliant
    case < 1: // Noncompliant
    case > 5: // Noncompliant
    default:
        handleTheRest();
        break;
}

switch (y)
{
    case int: // Noncompliant
    case not int: // Noncompliant
    default:
        handleTheRest();
        break;
}

static void handleTheRest() { }

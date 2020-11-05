switch (args[0])
{
    case "a":
        break;
    default: // Noncompliant {{Move this 'default:' case to the beginning or end of this 'switch' statement.}}
//  ^^^^^^^^
        break;
    case "b":
        break;
}

switch (args[0])
{
    default: // Compliant - first section
        break;

    case "a":
        break;
}

switch (args[0])
{
    case "a":
        break;
    default: // Compliant - last section
        break;
}

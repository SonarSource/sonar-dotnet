namespace EmptyProject;

public partial class UnusedPrivateMember
{
    private string currentHeading = "Initial heading";
    private string? newHeading;
    private string checkedMessage = "Not changed yet";

    private string unused; // Noncompliant

    private void UpdateHeading()
    {
        currentHeading = $"{newHeading}!!!";
    }

    private void CheckChanged()
    {
        checkedMessage = $"Last changed at {DateTime.Now}";
    }
}

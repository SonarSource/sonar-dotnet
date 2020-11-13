public record OverrideOnlyGetHashCode // Compliant - `Equals can't be overridden for records`
{
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

public class FieldKeyword
{
    public string Name
    {
        get
        {
            { // Noncompliant
                return field;
            }
        }
        set;
    }
}

public class LabeledJumps
{
    public void SwitchSection(int i)
    {
        sw: switch (i)
        {
            case 1:
            {
                break sw;
            }
        }
    }

    public void LabeledBlock()
    {
        int i = 0;
        label: { i++; }
    }
}

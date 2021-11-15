class Controller
{
    bool Access()
    {
        string access_level = "user";
        if (access_level != "⁣user") // Noncompliant {{Vulnerable character U+2063 detected}}
        //                   ^
        {
            return true;
        }
        return false;
    }
}

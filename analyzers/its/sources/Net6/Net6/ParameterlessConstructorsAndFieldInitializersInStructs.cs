namespace Net6
{
    public struct Struct
    {
        // Parameterless constructors and field initializers in structs
        public string Name { get; set; } = "Initialized";
        public string Description { get; set; }

        // Parameterless constructors and field initializers in structs
        public Struct()
        {
            Description = "This is initialized too";
        }
    }
}

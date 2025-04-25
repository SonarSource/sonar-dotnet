using System;

namespace  Nancy
{
    // Shim of a Nancy module
    // https://github.com/NancyFx/Nancy/wiki/Exploring-the-nancy-module
    public class NancyModule
    {
        public virtual void Get<T>(string path, Func<dynamic, T> action) { }
    }
}

public class SampleModule : Nancy.NancyModule
{
    public SampleModule()
    {
        Get("/", _ => "Hello World!"); // Compliant
    }
}

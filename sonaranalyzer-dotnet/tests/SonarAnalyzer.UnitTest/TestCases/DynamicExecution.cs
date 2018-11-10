namespace Test
{
    using System;
    using System.Configuration.Assemblies;
    using System.Globalization;
    using System.Reflection;
    using System.Security;
    using System.Security.Policy;

    public class TestReflection
    {
        public static void Run(string typeName, string methodName, string fieldName, string propertyName, string moduleName,
            byte[] data, string name, string path, AssemblyName assemblyName,
            Evidence evidence, SecurityContextSource contextSource)
        {
            //Assembly.Load(...) // Questionable
            Assembly.Load(assemblyName);
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^     {{Make sure that this dynamic injection or execution of code is safe.}}
            Assembly.Load(data);                        // Noncompliant
            Assembly.Load(assemblyName, evidence);      // Noncompliant
            Assembly.Load(data, data);                  // Noncompliant
            Assembly.Load(name, evidence);              // Noncompliant
            Assembly.Load(data, data, evidence);        // Noncompliant
            Assembly.Load(data, data, contextSource);   // Noncompliant

            //Assembly.LoadFile(...) // Questionable
            Assembly.LoadFile(path);                // Noncompliant
            Assembly.LoadFile(path, evidence);      // Noncompliant

            //Assembly.LoadFrom(...) // Questionable
            Assembly.LoadFrom(path);                // Noncompliant
            Assembly.LoadFrom(path, evidence);      // Noncompliant
            Assembly.LoadFrom(path, data, System.Configuration.Assemblies.AssemblyHashAlgorithm.MD5);           // Noncompliant
            Assembly.LoadFrom(path, evidence, data, System.Configuration.Assemblies.AssemblyHashAlgorithm.None);// Noncompliant

            //Assembly.LoadWithPartialName(...) // Questionable + deprecated
            Assembly.LoadWithPartialName(name);             // Noncompliant
            Assembly.LoadWithPartialName(name, evidence);   // Noncompliant

            //Assembly.ReflectionOnlyLoad(...)  ' This is OK as the resulting type is not executable.
            Assembly.ReflectionOnlyLoad(data);
            Assembly.ReflectionOnlyLoad(name);

            Assembly assembly = typeof(TestReflection).Assembly;

            // Review this code to make sure that the module, type, method and field are safe
            Type type = assembly.GetType(typeName);
//                      ^^^^^^^^^^^^^^^^^^^^^^^^^^     {{Make sure that this dynamic injection or execution of code is safe.}}


            type = assembly.GetType(typeName, false);                   // Noncompliant
            type = assembly.GetType(typeName, false, false);            // Noncompliant
            Module module = assembly.GetModule(moduleName);             // Noncompliant

            type = Type.GetType(typeName);                              // Noncompliant
            type = Type.GetType(typeName, true);                        // Noncompliant
            type = Type.GetType(typeName, true, true);                  // Noncompliant

            type = type.GetNestedType(typeName);                        // Noncompliant
            type = type.GetNestedType(typeName, BindingFlags.Instance); // Noncompliant

            type = type.GetInterface(typeName);                         // Noncompliant
            type = type.GetInterface(typeName, false);                  // Noncompliant

            MethodInfo method = type.GetMethod(methodName);                 // Noncompliant
            method = type.GetMethod(methodName, BindingFlags.IgnoreReturn); // Noncompliant
            method = type.GetMethod(methodName, new Type[] { });            // Noncompliant

            FieldInfo field = type.GetField(fieldName);                 // Noncompliant
            field = type.GetField(fieldName, BindingFlags.SetField);    // Noncompliant

            PropertyInfo property = type.GetProperty(propertyName);                 // Noncompliant
            property = type.GetProperty(propertyName, BindingFlags.SetProperty);    // Noncompliant


            // Review this code to make sure that the modules, types, methods and fields are used safely
            Module[] modules = assembly.GetModules();           // Noncompliant
            modules = assembly.GetLoadedModules();              // Noncompliant
            modules = assembly.GetLoadedModules(false);         // Noncompliant

            Type[] types = assembly.GetTypes();                 // Noncompliant
            types = assembly.GetExportedTypes();                // Noncompliant

            // Only available in NET Core 2.1+
            //types = assembly.GetForwardedTypes(); // Questionable

            types = type.GetNestedTypes();                      // Noncompliant
            types = type.GetNestedTypes(BindingFlags.Public);   // Noncompliant

            MethodInfo[] methods = type.GetMethods();           // Noncompliant
            FieldInfo[] fields = type.GetFields();              // Noncompliant
            PropertyInfo[] properties = type.GetProperties();   // Noncompliant
            MemberInfo[] members = type.GetMembers();           // Noncompliant
            members = type.GetMember(methodName);               // Noncompliant
            members = type.GetDefaultMembers();                 // Noncompliant

            //type.InvokeMember(...); // Questionable
            type.InvokeMember(methodName, BindingFlags.Public, null, null, null); // Noncompliant
            type.InvokeMember(methodName, BindingFlags.Public, null, null, null, CultureInfo.CurrentCulture); // Noncompliant
            type.InvokeMember(methodName, BindingFlags.Public, null, null, null, null, CultureInfo.CurrentCulture, null); // Noncompliant

            assembly.CreateInstance(typeName);                  // Noncompliant
            assembly.CreateInstance(typeName, false);           // Noncompliant
            assembly.CreateInstance(typeName, false, BindingFlags.Public, null, null, CultureInfo.CurrentCulture, null); // Noncompliant

            type = Type.ReflectionOnlyGetType(typeName, true, true); // This is OK as the resulting type is not executable.
        }

        public void ActivatorTests(string name, ActivationContext activationContext)
        {
            const string fixedType = "fixedType";
            Activator.CreateComInstanceFrom(name, fixedType);       // Noncompliant
            Activator.CreateComInstanceFrom(name, fixedType, new byte[] { }, AssemblyHashAlgorithm.MD5); // Noncompliant

            var type = typeof(Exception);
            Activator.CreateInstance(type); // Don't report - constructed from type
            Activator.CreateInstance(type, null); // Don't report - constructed from type
            Activator.CreateInstance(activationContext);        // Noncompliant
            Activator.CreateInstance(name, fixedType);          // Noncompliant
            Activator.CreateInstance(name, fixedType, null);    // Noncompliant

            Activator.CreateInstanceFrom(name, fixedType);      // Noncompliant
            Activator.CreateInstanceFrom(AppDomain.CurrentDomain, name, fixedType); // Noncompliant
            Activator.CreateInstance(activationContext);        // Noncompliant

            Activator.CreateInstance<Exception>(); // OK - known type
        }

        public void AdditionalTests(Assembly assembly, Evidence evidence)
        {
            const string constantName = "fixedName";

          
            // Hard-coded names for Assembly.Load are not ok...
            var asm = Assembly.Load(constantName);          // Noncompliant
            asm = Assembly.Load(constantName, evidence);    // Noncompliant
            Assembly.LoadFile(constantName);                // Noncompliant
            Assembly.LoadFrom(constantName, evidence);      // Noncompliant
            Assembly.LoadWithPartialName(constantName);     // Noncompliant

            // ...but hard-coded names for other methods are ok
            var type = assembly.GetType(constantName);
            type = assembly.GetType(constantName, false);

            type = Type.GetType(constantName);

            type.GetNestedType(constantName);
            type.GetInterface(constantName);
            type.GetMethod(constantName);
            type.GetField(constantName);
            type.GetProperty(constantName);
            assembly.GetModule(constantName);
            type.GetMember(constantName);

            type.InvokeMember(constantName, BindingFlags.NonPublic, null, null, null);

            type = type.GetType(); // OK - method on Object
            var obj1 = this.GetType();              // OK - this/Me
            var obj2 = typeof(object).GetMethods(); // OK - typeof/GetType
        }
    }
}

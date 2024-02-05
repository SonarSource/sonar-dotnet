using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

public interface IValid
{
    void VoidNoArg();
    void VoidArgInt(int count);
    void VoidArgStr(string name);

    Task TaskNoArg();
    Task TaskArg(int count);

    Task<int> TaskIntNoArg();
    Task<int> TaskIntArg(int count);

    Task<string> TaskStrNoArg();
    Task<string> TaskStrArg(int count);
}

public interface IEmpty
{
    // This is invalid and will throw
}

public interface IInheritsEmptyWithValid : IEmpty
{
    void Valid();
}

public interface IInheritsValidIsEmpty : IValid
{
    // Do not add anything => still valid
}

public interface IInheritsValidIsEmpty2 : IInheritsValidIsEmpty
{
    // Do not add anything => still valid - another level of nesting
}

public interface IInheritsEmptyIsEmpty : IEmpty
{
    // This is invalid and will throw
}

public interface IInheritsInvalid : IInvalid
{
    void SomethingValid();
}

public interface IInvalid : IMoreArguments
{
    // Just rename to have nice name for general tests below
}

public interface IMoreArguments
{
    void Valid();
    void Method(int first, string second);
}

public interface IReturnsInt
{
    void Valid();
    int Method();
}

public interface IReturnsTaskArray
{
    void Valid();
    Task[] Method();
}

public interface IReturnsObject
{
    void Valid();
    object Method();
}

public interface IGenericInterface<T>
{
    void Valid();
    void Method(T arg);
}

public interface IGenericMethod
{
    void Valid();
    void Method<T>(T arg);
}

public interface IProperty
{
    void Valid();
    int Value { get; }
}

public interface IIndexer
{
    void Valid();
    int this[int index] { get; set; }
}

public interface IEvent
{
    void Valid();
    event EventHandler<EventArgs> Event;
}

public class NotInterfaceClass { }
public struct NotInterfaceStruct { }

public class UseDurableEntityClient
{
    private readonly IDurableEntityClient client;
    private readonly EntityId id;

    public async Task UnrelatedMethods()
    {
        await client.ReadEntityStateAsync<IInvalid>(id);    // T is a type of the returned data. It's not an entity interface.
        await client.SignalEntityAsync(id, "name");         // Always compliant, same name but not generic
    }

    public async Task Compliant()
    {
        await client.SignalEntityAsync<IValid>(id, x => { });
        await client.SignalEntityAsync<IInheritsEmptyWithValid>(id, x => { });
        await client.SignalEntityAsync<IInheritsValidIsEmpty>(id, x => { });
        await client.SignalEntityAsync<IInheritsValidIsEmpty2>(id, x => { });
    }

    public async Task Overloads()
    {
        await client.SignalEntityAsync<IInvalid>(id, x => { });                 // Noncompliant
        await client.SignalEntityAsync<IInvalid>("id", x => { });               // Noncompliant
        await client.SignalEntityAsync<IInvalid>(id, DateTime.Now, x => { });   // Noncompliant
        await client.SignalEntityAsync<IInvalid>("id", DateTime.Now, x => { }); // Noncompliant
        //           ^^^^^^^^^^^^^^^^^^^^^^^^^^^
    }

    public async Task Reasons()
    {
        await client.SignalEntityAsync<IEmpty>(id, x => { });                   // Noncompliant {{Use valid entity interface. IEmpty is empty.}}
        await client.SignalEntityAsync<IInheritsEmptyIsEmpty>(id, x => { });    // Noncompliant {{Use valid entity interface. IInheritsEmptyIsEmpty is empty.}}
        await client.SignalEntityAsync<IInheritsInvalid>(id, x => { });         // Noncompliant {{Use valid entity interface. IInheritsInvalid contains method "Method" with 2 parameters. Zero or one are allowed.}}
        await client.SignalEntityAsync<IInvalid>(id, x => { });                 // Noncompliant {{Use valid entity interface. IInvalid contains method "Method" with 2 parameters. Zero or one are allowed.}}
        await client.SignalEntityAsync<IMoreArguments>(id, x => { });           // Noncompliant {{Use valid entity interface. IMoreArguments contains method "Method" with 2 parameters. Zero or one are allowed.}}
        await client.SignalEntityAsync<IReturnsInt>(id, x => { });              // Noncompliant {{Use valid entity interface. IReturnsInt contains method "Method" with invalid return type. Only "void", "Task" and "Task<T>" are allowed.}}
        await client.SignalEntityAsync<IReturnsTaskArray>(id, x => { });        // Noncompliant {{Use valid entity interface. IReturnsTaskArray contains method "Method" with invalid return type. Only "void", "Task" and "Task<T>" are allowed.}}
        await client.SignalEntityAsync<IReturnsObject>(id, x => { });           // Noncompliant {{Use valid entity interface. IReturnsObject contains method "Method" with invalid return type. Only "void", "Task" and "Task<T>" are allowed.}}
        await client.SignalEntityAsync<IGenericInterface<int>>(id, x => { });   // Noncompliant {{Use valid entity interface. IGenericInterface is generic.}}
        await client.SignalEntityAsync<IGenericMethod>(id, x => { });           // Noncompliant {{Use valid entity interface. IGenericMethod contains generic method "Method".}}
        await client.SignalEntityAsync<IProperty>(id, x => { });                // Noncompliant {{Use valid entity interface. IProperty contains property "Value". Only methods are allowed.}}
        await client.SignalEntityAsync<IIndexer>(id, x => { });                 // Noncompliant {{Use valid entity interface. IIndexer contains property "this[]". Only methods are allowed.}}
        await client.SignalEntityAsync<IEvent>(id, x => { });                   // Noncompliant {{Use valid entity interface. IEvent contains event "Event". Only methods are allowed.}}
        await client.SignalEntityAsync<NotInterfaceClass>(id, x => { });        // Noncompliant {{Use valid entity interface. NotInterfaceClass is not an interface.}}
        await client.SignalEntityAsync<NotInterfaceStruct>(id, x => { });       // Noncompliant {{Use valid entity interface. NotInterfaceStruct is not an interface.}}
    }

    public async Task WithTypeParameter<T>()
    {
        await client.SignalEntityAsync<T>(id, x => { });    // Compliant, we can't tell what T is
    }

    public async Task FromDurableClient(IDurableClient inheritedClient)
    {
        await inheritedClient.SignalEntityAsync<IInvalid>(id, x => { });                 // Noncompliant
    }
}

public class UseDurableOrchestrationContext
{
    private readonly IDurableOrchestrationContext context;
    private readonly EntityId id;

    public void UnrelatedMethods()
    {
        context.GetInput<IInvalid>();
    }

    public void Overloads()
    {
        context.CreateEntityProxy<IInvalid>(id);        // Noncompliant {{Use valid entity interface. IInvalid contains method "Method" with 2 parameters. Zero or one are allowed.}}
        context.CreateEntityProxy<IInvalid>("key");     // Noncompliant
        //      ^^^^^^^^^^^^^^^^^^^^^^^^^^^
    }

    public void Errors()
    {
        context.CreateEntityProxy<>("key");             // Error [CS0305]: Using the generic method group 'CreateEntityProxy' requires 1 type arguments
        context.CreateEntityProxy<Undefined>("key");    // Error [CS0246]: The type or namespace name 'Undefined' could not be found
        undefined.CreateEntityProxy<IInvalid>("key");   // Error [CS0103]: The type or namespace name 'undefined' could not be found
    }
}

public class UseDurableEntityContext
{
    private readonly IDurableEntityContext context;
    private readonly EntityId id;

    public void Overloads()
    {
        context.SignalEntity<IInvalid>(id, x => { });                 // Noncompliant
        context.SignalEntity<IInvalid>("id", x => { });               // Noncompliant
        context.SignalEntity<IInvalid>(id, DateTime.Now, x => { });   // Noncompliant
        context.SignalEntity<IInvalid>("id", DateTime.Now, x => { }); // Noncompliant
    }
}

public class AnotherType
{
    public void SignalEntityAsync<T>() { }
    public void SignalEntityAsync<TFirst, TSecond>() { }

    private static void Use(AnotherType client)
    {
        client.SignalEntityAsync<IInvalid>();           // Compliant, wrong type
        client.SignalEntityAsync<IInvalid, IInvalid>(); // For coverage
    }
}

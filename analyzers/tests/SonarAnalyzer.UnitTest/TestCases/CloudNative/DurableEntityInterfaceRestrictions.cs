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

public interface IInheritsEmptyWithValid : IEmpty
{
    void Valid();
}

public interface IInheritsValidIsEmpty : IValid
{
    // Do not add anything => still valid
}

public interface IEmpty
{
    // This is invalid and will throw
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

public class UseDurableClient
{
    private readonly IDurableClient client;
    private readonly EntityId id;

    public async Task UnrelatedMethods()
    {
        await client.ReadEntityStateAsync<IInvalid>(id);
        await client.StartNewAsync<IInvalid>("name", null);
        await client.SignalEntityAsync(id, "name");             // Always compliant, same name but not generic
    }

    public async Task Compliant()
    {
        await client.SignalEntityAsync<IValid>(id, x => { });
        await client.SignalEntityAsync<IInheritsEmptyWithValid>(id, x => { });
        await client.SignalEntityAsync<IInheritsValidIsEmpty>(id, x => { });
    }

    public async Task Overloads()
    {
        await client.SignalEntityAsync<IInvalid>(id, x => { });                 // FIXME Non-compliant
        await client.SignalEntityAsync<IInvalid>("id", x => { });               // FIXME Non-compliant
        await client.SignalEntityAsync<IInvalid>(id, DateTime.Now, x => { });   // FIXME Non-compliant
        await client.SignalEntityAsync<IInvalid>("id", DateTime.Now, x => { }); // FIXME Non-compliant
        //                             ******** FIXME
    }

    public async Task Reasons()
    {
        await client.SignalEntityAsync<IEmpty>(id, x => { });                   // FIXME Non-compliant {{FIXME}}
        await client.SignalEntityAsync<IInheritsEmptyIsEmpty>(id, x => { });    // FIXME Non-compliant {{FIXME}}
        await client.SignalEntityAsync<IInheritsInvalid>(id, x => { });         // FIXME Non-compliant {{FIXME}}
        await client.SignalEntityAsync<IInvalid>(id, x => { });                 // FIXME Non-compliant {{FIXME}}
        await client.SignalEntityAsync<IMoreArguments>(id, x => { });           // FIXME Non-compliant {{FIXME}}
        await client.SignalEntityAsync<IReturnsInt>(id, x => { });              // FIXME Non-compliant {{FIXME}}
        await client.SignalEntityAsync<IReturnsTaskArray>(id, x => { });        // FIXME Non-compliant {{FIXME}}
        await client.SignalEntityAsync<IReturnsObject>(id, x => { });           // FIXME Non-compliant {{FIXME}}
        await client.SignalEntityAsync<IGenericInterface<int>>(id, x => { });   // FIXME Non-compliant {{FIXME}}
        await client.SignalEntityAsync<IGenericMethod>(id, x => { });           // FIXME Non-compliant {{FIXME}}
        await client.SignalEntityAsync<IProperty>(id, x => { });                // FIXME Non-compliant {{FIXME}}
        await client.SignalEntityAsync<IIndexer>(id, x => { });                 // FIXME Non-compliant {{FIXME}}
        await client.SignalEntityAsync<IEvent>(id, x => { });                   // FIXME Non-compliant {{FIXME}}
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
        context.CreateEntityProxy<IInvalid>(id);        // FIXME Non-compliant {{FIXME}}
        context.CreateEntityProxy<IInvalid>("key");     // FIXME Non-compliant
    }
}

// FIXME: Defined in another assembly

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

public interface IStaticVirtualMembersInInterfaces
{
    static virtual bool Method(string firstParam, string secondParam) => firstParam == secondParam;
}

public interface IStaticVirtualMembersInInterfacesCompliant
{
    static virtual void Method(string firstParam) => Console.WriteLine(firstParam);
}

public class UseDurableEntityContext
{
    private readonly IDurableEntityContext context;
    private readonly EntityId id;

    public void Overloads()
    {
        context.SignalEntity<IStaticVirtualMembersInInterfaces>(id, x => { });          // Noncompliant {{Use valid entity interface. IStaticVirtualMembersInInterfaces contains method "Method" with 2 parameters. Zero or one are allowed.}}
        context.SignalEntity<IStaticVirtualMembersInInterfacesCompliant>(id, x => { }); // Compliant
    }
}

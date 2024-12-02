using Moq;
using System;
using System.Threading.Tasks;

public interface IReproInterface
{
    event EventHandler SomethingHappened;
}

// https://github.com/SonarSource/sonar-dotnet/issues/9697
public class Repro
{
    public async Task AsyncMethod()
    {
        await Task.Yield();

        var mock = new Mock<IReproInterface>();

        mock.Raise(i => i.SomethingHappened += null, new EventArgs()); // Noncompliant FP
    }
}

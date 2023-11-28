using MediatR;

namespace Repro
{
    public abstract class StateBase<T, TCommand> where T : Model where TCommand : IRequest<T>
    {
    }

    public class Model : BaseModel<int>
    {
    }

    public abstract class BaseModel<T> { }


}

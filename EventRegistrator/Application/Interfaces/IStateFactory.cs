using EventRegistrator.Application.Objects.Enums;

namespace EventRegistrator.Application.Interfaces
{
    public interface IStateFactory
    {
        IState CreateState(StateType stateType);
    }
}
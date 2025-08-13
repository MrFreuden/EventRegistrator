using EventRegistrator.Application.Enums;

namespace EventRegistrator.Application.Interfaces
{
    public interface IStateFactory
    {
        IState CreateState(StateType stateType);
    }
}
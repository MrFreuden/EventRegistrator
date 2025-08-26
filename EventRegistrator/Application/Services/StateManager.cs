using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Enums;
using EventRegistrator.Application.Factories;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.States;
using EventRegistrator.Domain.DTO;
using EventRegistrator.Domain.Models;
using System.Windows.Input;

namespace EventRegistrator.Application.Services
{
    public class StateManager : IStateManager
    {
        public void ClearState(UserAdmin user)
        {
            user.ClearStateHistory();
        }

        public void RevertState(UserAdmin user)
        {
            user.RevertState();
        }

        public void TransitionToState(UserAdmin user, IState state)
        {
            user.SetCurrentState(state);
        }
    }
    public interface IStateManager
    {
        void TransitionToState(UserAdmin user, IState state);
        void RevertState(UserAdmin user);
        void ClearState(UserAdmin user);
    }
    public abstract class BaseState : IState
    {
        protected readonly IStateManager _stateManager;
        protected readonly IStateFactory _stateFactory;

        protected BaseState(IStateManager stateManager, IStateFactory stateFactory)
        {
            _stateManager = stateManager;
            _stateFactory = stateFactory;
        }

        protected abstract Task<StateResult> ProcessInput(MessageDTO message, UserAdmin user);
        protected abstract bool ShouldChangeState(StateResult result);
        protected abstract StateType GetNextStateType(StateResult result);
        public abstract Task<Response> Handle(MessageDTO message, UserAdmin user);

        public virtual async Task<List<Response>> Execute(MessageDTO message, UserAdmin user)
        {
            var result = await ProcessInput(message, user);
            if (ShouldChangeState(result))
            {
                var nextStateType = GetNextStateType(result);
                if (nextStateType == StateType.Revert)
                {
                    user.RevertState();
                    return [await user.State.Handle(message, user)];
                }
                else
                {
                    var newState = _stateFactory.CreateState(nextStateType);
                    user.SetCurrentState(newState);
                    return [await newState.Handle(message, user)];
                }
            }
            return result.Responses;
        }
    }

    public class StateResult
    {
        public List<Response> Responses { get; set; }
        public object Data { get; set; }
        public bool ShouldTransition { get; set; }
        public StateType? NextStateType { get; set; }
    }

    public class CommandResult
    {
    }
}

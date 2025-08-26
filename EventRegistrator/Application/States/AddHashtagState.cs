using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Enums;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Services;
using EventRegistrator.Domain.DTO;
using EventRegistrator.Domain.Models;
using EventRegistrator.Infrastructure.Utils;

namespace EventRegistrator.Application.States
{
    public class AddHashtagState : BaseState
    {
        public AddHashtagState(IStateManager stateManager, IStateFactory stateFactory) : base(stateManager, stateFactory)
        {
        }

        public override async Task<Response> Handle(MessageDTO message, UserAdmin user)
        {
            return new Response { ChatId = message.ChatId, Text = Constants.AskForHashtag, MessageToEditId = null };
        }

        protected override StateType GetNextStateType(StateResult result)
        {
            return StateType.Revert;
        }

        protected override bool ShouldChangeState(StateResult result)
        {
            return result.ShouldTransition;
        }

        protected override async Task<StateResult> ProcessInput(MessageDTO message, UserAdmin user)
        {
            user.GetTargetChat(user.CurrentContext.TargetChatId.Value).AddHashtag(new Hashtag(message.Text));

            return new StateResult
            {
                Responses = [new Response { ChatId = message.ChatId, Text = "Хэштег добавлен" }],
                ShouldTransition = true
            };
        }    
    }
}
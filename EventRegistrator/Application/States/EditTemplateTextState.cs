using EventRegistrator.Application.Commands;
using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Enums;
using EventRegistrator.Application.Factories;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Services;
using EventRegistrator.Domain.DTO;
using EventRegistrator.Domain.Models;
using EventRegistrator.Infrastructure.Utils;

namespace EventRegistrator.Application.States
{
    public class EditTemplateTextState : BaseState
    {
        private readonly ICommandFactory _commandFactory;
        public EditTemplateTextState(IStateManager stateManager, IStateFactory stateFactory, ICommandFactory commandFactory) : base(stateManager, stateFactory)
        {
            _commandFactory = commandFactory;
        }

        public override async Task<Response> Handle(MessageDTO message, UserAdmin user)
        {
            return new Response { ChatId = message.ChatId, Text = Constants.AskForNewTemplate, MessageToEditId = null };
        }

        protected override StateType GetNextStateType(StateResult result)
        {
            return StateType.Revert;
        }

        protected override async Task<StateResult> ProcessInput(MessageDTO message, UserAdmin user)
        {
            ICommand command;
            if (user.CurrentContext.EventId.HasValue)
            {
                command = _commandFactory.CreateCommand(CommandType.EditEventTemplate);
            }
            else
            {
                command = _commandFactory.CreateCommand(CommandType.EditHashtagTemplate);
            }

            var responses = await command.Execute(message, user);

            return new StateResult
            {
                Responses = responses,
                Data = null,
                ShouldTransition = true
            };
        }

        protected override bool ShouldChangeState(StateResult result)
        {
            return result.ShouldTransition;
        }
    }
}

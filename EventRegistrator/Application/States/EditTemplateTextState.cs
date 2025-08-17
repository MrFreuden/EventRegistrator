using EventRegistrator.Application.Commands;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Objects.DTOs;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.States
{
    public class EditTemplateTextState : IState
    {
        private readonly EditTemplateTextCommand _editTemplateTextCommand;
        public EditTemplateTextState()
        {
            _editTemplateTextCommand = new EditTemplateTextCommand();
        }
        public async Task<List<Response>> Execute(MessageDTO message, UserAdmin user)
        {
            user.IsAsked = false;
            return await _editTemplateTextCommand.Execute(message, user);
        }

        public async Task<Response> Handle(MessageDTO message, UserAdmin user)
        {
            user.IsAsked = true;
            return new Response { ChatId = message.ChatId, Text = Constants.AskForNewTemplate, MessageToEditId = user.LastMessageId };
        }
    }
}

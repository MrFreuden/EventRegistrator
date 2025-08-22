using EventRegistrator.Application.Commands;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Objects.DTOs;
using EventRegistrator.Application.Services;
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
            return await _editTemplateTextCommand.Execute(message, user);
        }

        public async Task<Response> Handle(MessageDTO message, UserAdmin user)
        {
            return new Response { ChatId = message.ChatId, Text = Constants.AskForNewTemplate, MessageToEditId = null };
        }
    }
}

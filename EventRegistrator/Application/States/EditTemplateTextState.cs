using EventRegistrator.Application.Commands;
using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Services;
using EventRegistrator.Domain.DTO;
using EventRegistrator.Domain.Models;
using EventRegistrator.Infrastructure.Utils;

namespace EventRegistrator.Application.States
{
    public class EditTemplateTextState : IState
    {
        private readonly EditUserTemplateTextCommand _editTemplateTextCommand;
        public EditTemplateTextState()
        {
            _editTemplateTextCommand = new EditUserTemplateTextCommand();
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

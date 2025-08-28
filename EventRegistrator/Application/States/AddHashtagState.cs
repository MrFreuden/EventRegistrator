using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Domain.DTO;
using EventRegistrator.Domain.Models;
using EventRegistrator.Infrastructure.Utils;

namespace EventRegistrator.Application.States
{
    public class AddHashtagState : IState
    {
        public async Task<List<Response>> Execute(MessageDTO message, UserAdmin user)
        {
            user.GetTargetChat(user.CurrentContext.TargetChatId.Value).AddHashtag(new Hashtag(message.Text));
            user.RevertState();
            user.LastMessageId = null;
            return [await user.State.Handle(message, user)];
        }

        public async Task<Response> Handle(MessageDTO message, UserAdmin user)
        {
            return new Response { ChatId = message.ChatId, Text = Constants.AskForHashtag, MessageToEditId = null };
        }
    }
}
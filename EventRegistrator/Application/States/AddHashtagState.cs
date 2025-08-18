using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Objects.DTOs;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.States
{
    public class AddHashtagState : IState
    {
        public async Task<List<Response>> Execute(MessageDTO message, UserAdmin user)
        {
            user.GetTargetChat(user.CurrentContext.TargetChatId.Value).AddHashtag(new Hashtag(message.Text));
            user.State = user.StateHistory.Pop();
            return [await user.State.Handle(message, user)];
        }

        public async Task<Response> Handle(MessageDTO message, UserAdmin user)
        {
            user.IsAsked = true;
            return new Response { ChatId = message.ChatId, Text = Constants.AskForHashtag, MessageToEditId = null };
        }
    }
}
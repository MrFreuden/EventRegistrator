using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Services;
using EventRegistrator.Domain.DTO;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.Commands
{
    public class EditTemplateTextCommand : ICommand
    {
        public async Task<List<Response>> Execute(MessageDTO message, UserAdmin user)
        {
            var hashtag = user.GetTargetChat(user.CurrentContext.TargetChatId.Value).GetHashtagByName(user.CurrentContext.HashtagName);
            hashtag.EditTemplateText(message.Text);
            user.RevertState();
            user.LastMessageId = null;

            return [await user.State.Handle(message, user)];
        }
    }
}

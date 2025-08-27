using EventRegistrator.Application.Commands.Attributes;
using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Domain.DTO;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.Commands
{
    [Command("EditTemplate", "Редактирование шаблона")]
    public class EditUserTemplateTextCommand : ICommand
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

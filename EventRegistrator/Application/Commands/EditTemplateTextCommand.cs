using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.Commands
{
    public class EditTemplateTextCommand : ICommand
    {
        public async Task<List<Response>> Execute(MessageDTO message, UserAdmin user)
        {
            user.IsAsked = false;
            var hashtag = user.GetTargetChat().GetHashtagByName("sws");
            hashtag.EditTemplateText(message.Text);
            return [new Response { ChatId = message.ChatId, Text = hashtag.TemplateText }];
        }
    }
}

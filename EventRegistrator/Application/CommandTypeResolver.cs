using EventRegistrator.Application.Objects.Enums;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application
{
    public static class CommandTypeResolver
    {
        private const char _hashtag = '#';
        public static CommandType? DetermineCommandType(MessageDTO message, UserAdmin user)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (message.IsEdit && IsReplyToPostMessage(message, user))
                return CommandType.CancelRegistration;
            if (IsFromChannel(message, user) && IsHasHashtag(message, user))
                return CommandType.CreateEvent;
            if (IsReplyToPostMessage(message, user))
                return CommandType.Register;
            if (message.Text.Equals(Constants.Cancel))
                return CommandType.CancelRegistrations;
            if (message.Text.Equals(Constants.Pagination))
                return CommandType.StartPagination;
            return null;
        }

        public static StateType? DetermineStateType(MessageDTO message, UserAdmin user)
        {
            if (message.Text.Equals(Constants.EditTemplateText))
                return StateType.EditTemplateText;
            
            return null;
        }

        private static bool IsHasHashtag(MessageDTO message, UserAdmin user)
        {
            var lastPart = message.Text.Split(
                new[] { "\r\n", "\n", "\r" },
                StringSplitOptions.None
            ).Last();
            if (user.ContainsHashtag(lastPart.Trim(_hashtag)))
            {
                return true;
            }
            return false;
        }

        private static bool IsFromChannel(MessageDTO message, UserAdmin user)
        {
            if (message.ForwardFromChat != null)
            {
                return user.ContainsChannel(message.ForwardFromChat.Id);
            }

            return false;
        }

        private static bool IsReplyToPostMessage(MessageDTO message, UserAdmin user)
        {
            if (message.ReplyToMessage != null && message.ReplyToMessage.ForwardFromChat != null)
            {
                return user.ContainsChannel(message.ReplyToMessage.ForwardFromChat.Id);
            }

            return false;
        }

        
    }
}

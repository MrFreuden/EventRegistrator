using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;
using EventRegistrator.Infrastructure;
using Telegram.Bot.Types;

namespace EventRegistrator.Application
{
    public class TargetChatMessageHandler
    {
        private readonly IUserRepository _userRepository;

        public async Task<MessageDTO> HandleEdit(Message message)
        {
            if (IsReplyToPostMessage(message))
            {
                var user = _userRepository.GetUserByTargetChat(message.Chat.Id);
                var lastEvent = user.GetLastEvent();
                lastEvent.RemoveRegistrations(message.Id);
                await _messageSender.UnLikeMessage(user.TargetChatId, message.Id);

                await _messageSender.EditEventData(user.PrivateChatId, lastEvent.PrivateMessageId, lastEvent);

                var text = TimeSlotParser.UpdateTemplateText(user.TempleText, lastEvent.GetSlots());
                lastEvent.TemplateText = text;
                await _messageSender.EditFirstComment(user.TargetChatId, lastEvent.CommentMessageId, lastEvent.TemplateText);
                return await ProcessOnEventRegistration(message);
            }
            return new MessageDTO { ChatId = message.Chat.Id, Text = Constants.Error };
        }

        public async Task<MessageDTO> Handle(Message message)
        {
            if (IsFromChannel(message) && IsHasHashtag(message))
            {
                return await ProcessNewEvent(message);
            }
            else if (IsReplyToPostMessage(message))
            {
                return await ProcessOnEventRegistration(message);
            }
            return new MessageDTO { ChatId = message.Chat.Id, Text = Constants.Error };
        }

        private async Task<MessageDTO> ProcessOnEventRegistration(Message message)
        {
            var user = _userRepository.GetUserByTargetChat(message.Chat.Id);
            var lastEvent = user.GetLastEvent();
            var map = TimeSlotParser.GetMaper(lastEvent.TemplateText);
            var regs = TimeSlotParser.ParseRegistrationMessage(message.Text, message.From.Id, message.Date, map, message.Id);

            var result = AddRegistrations(lastEvent.GetSlots(), regs);
            if (result == true)
            {
                if (lastEvent.PrivateMessageId == default)
                {
                    var text = TimeSlotParser.UpdateTemplateText(user.TempleText, lastEvent.GetSlots());
                    lastEvent.TemplateText = text;
                    var m = await _messageSender.SendEventData(user.PrivateChatId, lastEvent);
                    lastEvent.PrivateMessageId = m.MessageId;
                }
                else
                {
                    var text = TimeSlotParser.UpdateTemplateText(user.TempleText, lastEvent.GetSlots());
                    lastEvent.TemplateText = text;
                    await _messageSender.EditEventData(user.PrivateChatId, lastEvent.PrivateMessageId, lastEvent);
                }
                await _messageSender.EditFirstComment(user.TargetChatId, lastEvent.CommentMessageId, lastEvent.TemplateText);
                await _messageSender.LikeMessage(user.TargetChatId, message.Id);

                return new MessageDTO
                {
                    ChatId = user.TargetChatId,
                    Text = lastEvent.TemplateText,
                    MessageToEditId = lastEvent.CommentMessageId,
                    Like = true
                };`
            }
            else
            {
                Console.WriteLine("Ошибка добавления во временной слот");
            }
        }

        private async Task<MessageDTO> ProcessNewEvent(Message message)
        {
            var user = _userRepository.GetUserByTargetChat(message.Chat.Id);
            var newEvent = new Event(new Guid(), "SWS", message.ForwardFromChat.Id, message.Id, user.HashtagName, user.TempleText);
            var slots = TimeSlotParser.ExtractTimeSlotsFromTemplate(user.TempleText, message.Date);

            newEvent.AddSlots(slots);
            user.AddEvent(newEvent);

            return new MessageDTO { 
                ChatId = message.Chat.Id, 
                Text = user.TempleText, 
                MessageToReplyId = message.Id, 
                ButtonData = (Constants.Cancel, Constants.Cancel) };
        }

        private bool AddRegistrations(List<TimeSlot> slots, List<Registration> registrations)
        {
            foreach (var registration in registrations)
            {
                var slot = TimeSlotParser.FindMatchingTimeSlot(slots, registration);
                if (slot == default || slot.AddRegistration(registration) == false)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsHasHashtag(Message message)
        {
            var parts = message.Text.Split(
                new[] { "\r\n", "\n", "\r" },
                StringSplitOptions.None
            );
            return parts[parts.Length - 1].Contains(_userRepository.GetUserByTargetChat(message.Chat.Id).HashtagName);
        }

        private bool IsFromChannel(Message message)
        {
            var user = _userRepository.GetUserByTargetChat(message.Chat.Id);
            if (message.ForwardFromChat != null)
            {
                return message.ForwardFromChat.Id == user.ChannelId;
            }
            return false;
        }

        private bool IsReplyToPostMessage(Message message)
        {
            var user = _userRepository.GetUserByTargetChat(message.Chat.Id);
            if (message.ReplyToMessage != null && message.ReplyToMessage.ForwardFromChat != null)
            {
                return message.ReplyToMessage.ForwardFromChat.Id == user.ChannelId;
            }

            return false;
        }
    }
}

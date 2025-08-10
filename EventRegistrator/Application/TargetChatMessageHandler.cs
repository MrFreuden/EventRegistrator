using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;
using System.Text;
using Telegram.Bot.Types;

namespace EventRegistrator.Application
{
    public class TargetChatMessageHandler
    {
        private readonly IUserRepository _userRepository;

        public TargetChatMessageHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<MessageDTO>> HandleEdit(Message message)
        {
            var messages = new List<MessageDTO>();
            if (IsReplyToPostMessage(message))
            {
                var user = _userRepository.GetUserByTargetChat(message.Chat.Id);
                var lastEvent = user.GetLastEvent();
                lastEvent.RemoveRegistrations(message.Id);

                var unlikeMessage = new MessageDTO
                {
                    ChatId = user.TargetChatId,
                    MessageToEditId = message.Id,
                    UnLike = true,
                };

                messages.Add(unlikeMessage);

                var eventDataPrivateUpdateMessage = new MessageDTO
                {
                    ChatId = user.PrivateChatId,
                    Text = EventFormatter.FormatRegistrationsInfo(lastEvent),
                    MessageToEditId = lastEvent.PrivateMessageId,
                };

                messages.Add(eventDataPrivateUpdateMessage);

                var text = TimeSlotParser.UpdateTemplateText(user.TempleText, lastEvent.GetSlots());
                lastEvent.TemplateText = text;

                var firstCommentUpdateMessage = new MessageDTO
                {
                    ChatId = user.TargetChatId,
                    Text = lastEvent.TemplateText,
                    MessageToEditId = lastEvent.CommentMessageId,
                };
                messages.Add(firstCommentUpdateMessage);

                return await ProcessOnEventRegistration(message);
            }
            messages.Add(new MessageDTO { ChatId = message.Chat.Id, Text = Constants.Error });
            return messages;
        }

        public async Task<List<MessageDTO>> Handle(Message message)
        {
            if (IsFromChannel(message) && IsHasHashtag(message))
            {
                return new List<MessageDTO> { await ProcessNewEvent(message) };
            }
            else if (IsReplyToPostMessage(message))
            {
                return await ProcessOnEventRegistration(message);
            }
            return new List<MessageDTO> { new MessageDTO { ChatId = message.Chat.Id, Text = Constants.Error } };
        }

        private async Task<List<MessageDTO>> ProcessOnEventRegistration(Message message)
        {
            var messages = new List<MessageDTO>();

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
                    //var m = await _messageSender.SendEventData(user.PrivateChatId, lastEvent);
                    //lastEvent.PrivateMessageId = m.MessageId;

                    var eventDataMessage = new MessageDTO
                    {
                        ChatId = user.PrivateChatId,
                        Text = EventFormatter.FormatRegistrationsInfo(lastEvent),
                        SaveMessageIdCallback = id => { lastEvent.PrivateMessageId = id; }
                    };
                    messages.Add(eventDataMessage);
                }
                else
                {
                    var text = TimeSlotParser.UpdateTemplateText(user.TempleText, lastEvent.GetSlots());
                    lastEvent.TemplateText = text;

                    var eventDataPrivateUpdateMessage = new MessageDTO
                    {
                        ChatId = user.PrivateChatId,
                        Text = lastEvent.TemplateText,
                        MessageToEditId = lastEvent.PrivateMessageId,
                    };
                    
                    messages.Add(eventDataPrivateUpdateMessage);
                }

                var firstCommentUpdateMessage = new MessageDTO
                {
                    ChatId = user.TargetChatId,
                    Text = lastEvent.TemplateText,
                    MessageToEditId = lastEvent.CommentMessageId,
                    ButtonData = (Constants.Cancel, Constants.Cancel),
                };

                var likeMessage = new MessageDTO
                {
                    ChatId = user.TargetChatId,
                    MessageToEditId = message.Id,
                    Like = true,
                };

                messages.Add(firstCommentUpdateMessage);
                messages.Add(likeMessage);
                return messages;
            }
            else
            {
                Console.WriteLine("Ошибка добавления во временной слот");
                return messages;
            }
        }

        private async Task<MessageDTO> ProcessNewEvent(Message message)
        {
            var user = _userRepository.GetUserByTargetChat(message.Chat.Id);
            var newEvent = new Event(new Guid(), "SWS", message.ForwardFromChat.Id, message.Id, user.HashtagName, user.TempleText);
            var slots = TimeSlotParser.ExtractTimeSlotsFromTemplate(user.TempleText, message.Date);

            newEvent.AddSlots(slots);
            user.AddEvent(newEvent);

            return new MessageDTO
            {
                ChatId = message.Chat.Id,
                Text = user.TempleText,
                MessageToReplyId = message.Id,
                ButtonData = (Constants.Cancel, Constants.Cancel),
                SaveMessageIdCallback = id => { newEvent.CommentMessageId = id; }
            };
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

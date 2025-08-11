using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;
using Telegram.Bot.Types;

namespace EventRegistrator.Application.Services
{
    public class RegistrationService
    {
        private readonly IUserRepository _userRepository;

        public RegistrationService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public List<AntwortDTO> Register(MessageDTO message)
        {
            var user = _userRepository.GetUserByTargetChat(message.ChatId);
            var lastEvent = user.GetLastEvent();
            var map = TimeSlotParser.GetMaper(lastEvent.TemplateText);
            var regs = TimeSlotParser.ParseRegistrationMessage(message.Text, message.UserId.Value, message.Created, map, message.Id);

            var result = AddRegistrations(lastEvent.GetSlots(), regs);
            if (result == true)
            {
                var messages = GetUpdateMessages(user, lastEvent);
                messages.Add(CreateLikeMessage(lastEvent.TargetChatId, message.Id));
                return messages;
            }
            else
            {
                Console.WriteLine("Ошибка добавления во временной слот");
                return [new AntwortDTO { ChatId = message.ChatId, Text = Constants.Error }];
            }
        }

        public List<AntwortDTO> HandleRegistrationEdit(MessageDTO message)
        {
            var user = _userRepository.GetUserByTargetChat(message.ChatId);
            var lastEvent = user.GetLastEvent();

            var messages = Unregister(user, lastEvent, message.Id);

            var text = TimeSlotParser.UpdateTemplateText(lastEvent.TemplateText, lastEvent.GetSlots());
            lastEvent.TemplateText = text;

            messages.Add(GetUpdateCommentMessage(lastEvent));
            messages.AddRange(Register(message));

            return messages;
        }

        private List<AntwortDTO> Unregister(UserAdmin user, Event lastEvent, int messageId)
        {
            lastEvent.RemoveRegistrations(messageId);

            var unlikeMessage = CreateUnlikeMessage(lastEvent.TargetChatId, messageId);

            var privateMessage = GetEventDataPrivateMessage(user.PrivateChatId, lastEvent);

            return [unlikeMessage, privateMessage];
        }

        private List<AntwortDTO> GetUpdateMessages(UserAdmin user, Event lastEvent)
        {
            var text = TimeSlotParser.UpdateTemplateText(lastEvent.TemplateText, lastEvent.GetSlots());
            lastEvent.TemplateText = text;

            var eventDataPrivateMessage = GetEventDataPrivateMessage(user.PrivateChatId, lastEvent);

            var firstCommentUpdateMessage = new AntwortDTO
            {
                ChatId = lastEvent.TargetChatId,
                Text = lastEvent.TemplateText,
                MessageToEditId = lastEvent.CommentMessageId,
                ButtonData = (Constants.Cancel, Constants.Cancel),
            };
            
            return [eventDataPrivateMessage, firstCommentUpdateMessage];
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

        private AntwortDTO GetUpdateCommentMessage(Event lastEvent)
        {
            var firstCommentUpdateMessage = new AntwortDTO
            {
                ChatId = lastEvent.TargetChatId,
                Text = lastEvent.TemplateText,
                MessageToEditId = lastEvent.CommentMessageId,
            };
            return firstCommentUpdateMessage;
        }

        private AntwortDTO GetEventDataPrivateMessage(long chatId, Event lastEvent)
        {
            if (lastEvent.PrivateMessageId == default)
            {
                var eventDataMessage = new AntwortDTO
                {
                    ChatId = chatId,
                    Text = TextFormatter.FormatRegistrationsInfo(lastEvent),
                    SaveMessageIdCallback = id => { lastEvent.PrivateMessageId = id; }
                };
                return eventDataMessage;
            }
            else
            {
                var eventDataPrivateUpdateMessage = new AntwortDTO
                {
                    ChatId = chatId,
                    //Text = lastEvent.TemplateText,
                    Text = TextFormatter.FormatRegistrationsInfo(lastEvent),
                    MessageToEditId = lastEvent.PrivateMessageId,
                };
                return eventDataPrivateUpdateMessage;
            }
        }

        private AntwortDTO CreateLikeMessage(long chatId, int messageId)
        {
            return new AntwortDTO
            {
                ChatId = chatId,
                MessageToEditId = messageId,
                Like = true,
            };
        }

        private AntwortDTO CreateUnlikeMessage(long chatId, int messageId)
        {
            return new AntwortDTO
            {
                ChatId = chatId,
                MessageToEditId = messageId,
                UnLike = true,
            };
        }
    }
}

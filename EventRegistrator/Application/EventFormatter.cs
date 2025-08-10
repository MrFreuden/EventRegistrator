using EventRegistrator.Domain.Models;
using EventRegistrator.Infrastructure;
using System.Text;

namespace EventRegistrator.Application
{
    public static class EventFormatter
    {
        public static string FormatRegistrationsInfo(Event lastEvent)
        {
            var slots = lastEvent.GetSlots() ?? new List<TimeSlot>();

            if (slots.Count == 0)
                return "Нет доступных временных слотов";

            var sb = new StringBuilder();
            sb.AppendLine($"Запись на: {lastEvent.Title}");
            sb.AppendLine();

            slots = slots.OrderBy(s => s.Time).ToList();

            foreach (var slot in slots)
            {
                sb.AppendLine($"{slot.Time:HH:mm}");

                var registrations = GetRegistrationsFromTimeSlot(slot);

                if (registrations.Any())
                {
                    foreach (var registration in registrations)
                    {
                        sb.AppendLine(registration.Name);
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static List<Registration> GetRegistrationsFromTimeSlot(TimeSlot slot)
        {
            var registrationsField = typeof(TimeSlot).GetField("_currentRegistrations",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (registrationsField != null)
            {
                var registrations = registrationsField.GetValue(slot) as List<Registration>;
                return registrations ?? new List<Registration>();
            }

            return new List<Registration>();
        }

        public static string GetAllUsersInfo(UserRepository userRepository)
        {
            var sb = new StringBuilder();
            var users = userRepository.GetAllUsers();
            
            if (users.Count == 0)
            {
                return "Нет зарегистрированных пользователей.";
            }
            
            sb.AppendLine($"Всего пользователей: {users.Count}");
            sb.AppendLine();
            
            foreach (var user in users)
            {
                sb.AppendLine($"===== Пользователь ID: {user.Id} =====");
                sb.AppendLine($"Приватный чат ID: {user.PrivateChatId}");
                sb.AppendLine($"Целевой чат ID: {user.TargetChatId}");
                sb.AppendLine($"Канал ID: {user.ChannelId}");
                sb.AppendLine($"Название канала: {user.ChannelName ?? "Не указано"}");
                sb.AppendLine($"Хэштег: {user.HashtagName ?? "Не указано"}");
                sb.AppendLine($"Режим ожидания ввода: {(user.IsAsked ? "Да" : "Нет")}");
                
                sb.AppendLine("Шаблон сообщения:");
                sb.AppendLine(user.TempleText);
                sb.AppendLine();
                
                // Получаем информацию о событиях пользователя через рефлексию
                // (так как _events приватное поле)
                var eventsField = typeof(UserAdmin).GetField("_events", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (eventsField != null)
                {
                    var events = eventsField.GetValue(user) as List<Event>;
                    if (events != null && events.Any())
                    {
                        sb.AppendLine($"События ({events.Count}):");
                        foreach (var evt in events)
                        {
                            sb.AppendLine($"  - ID: {evt.Id}");
                            sb.AppendLine($"    Название: {evt.Title}");
                            sb.AppendLine($"    Канал ID: {evt.ChannelId}");
                            sb.AppendLine($"    Пост ID: {evt.PostId}");
                            sb.AppendLine($"    ID сообщения в привате: {evt.PrivateMessageId}");
                            sb.AppendLine($"    ID сообщения-комментария: {evt.CommentMessageId}");
                            
                            var slots = evt.GetSlots();
                            if (slots != null && slots.Any())
                            {
                                sb.AppendLine($"    Временные слоты ({slots.Count}):");
                                foreach (var slot in slots.OrderBy(s => s.Time))
                                {
                                    var regs = GetRegistrationsFromTimeSlot(slot);
                                    sb.AppendLine($"      - {slot.Time:HH:mm} (Занято: {slot.CurrentRegistrationCount}/{slot.MaxCapacity})");
                                    
                                    if (regs.Any())
                                    {
                                        foreach (var reg in regs)
                                        {
                                            sb.AppendLine($"        * {reg.Name} (ID: {reg.UserId}, Сообщение: {reg.MessageId})");
                                        }
                                    }
                                }
                            }
                            sb.AppendLine();
                        }
                    }
                    else
                    {
                        sb.AppendLine("У пользователя нет событий.");
                    }
                }
                
                sb.AppendLine();
                sb.AppendLine("------------------------------");
                sb.AppendLine();
            }
            
            return sb.ToString();
        }
    }
}

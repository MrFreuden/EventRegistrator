using EventRegistrator.Application.Commands.Attributes;
using System.Reflection;

namespace EventRegistrator.Application.Commands
{
    public class CommandRegistry
    {
        private readonly Dictionary<string, Type> _slashCommands;
        private readonly Dictionary<string, Type> _callbackCommands;

        public CommandRegistry()
        {
            _slashCommands = new Dictionary<string, Type>();
            _callbackCommands = new Dictionary<string, Type>();
            DiscoverCommands();
        }

        private void DiscoverCommands()
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Находит все классы с атрибутом [Command]
            var commandTypes = assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<CommandAttribute>() != null);

            foreach (var type in commandTypes)
            {
                var attr = type.GetCustomAttribute<CommandAttribute>();
                _slashCommands[attr.Name] = type;
            }

            // Находит все классы с атрибутом [CallbackCommand]
            var callbackTypes = assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<CallbackCommandAttribute>() != null);

            foreach (var type in callbackTypes)
            {
                var attr = type.GetCustomAttribute<CallbackCommandAttribute>();
                _callbackCommands[attr.Name] = type;
            }
        }

        public Type GetSlashCommand(string commandName)
        {
            return _slashCommands.TryGetValue(commandName, out var type) ? type : null;
        }
        
        public Type GetCallbackCommand(string commandName)
        {
            return _callbackCommands.TryGetValue(commandName, out var type) ? type : null;
        }
        
        public IEnumerable<string> GetAllSlashCommands() => _slashCommands.Keys;
        public IEnumerable<string> GetAllCallbackCommands() => _callbackCommands.Keys;
        
        public void PrintRegisteredCommands()
        {
            Console.WriteLine("Зарегистрированные слеш-команды:");
            foreach (var cmd in _slashCommands)
            {
                Console.WriteLine($"  {cmd.Key} -> {cmd.Value.Name}");
            }
            
            Console.WriteLine("Зарегистрированные callback-команды:");
            foreach (var cmd in _callbackCommands)
            {
                Console.WriteLine($"  {cmd.Key} -> {cmd.Value.Name}");
            }
        }
    }
}

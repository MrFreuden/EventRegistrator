namespace EventRegistrator.Application.Interfaces
{
    public interface ICommandFactory
    {
        ICommand CreateCommand(CommandType commandType);
    }
}

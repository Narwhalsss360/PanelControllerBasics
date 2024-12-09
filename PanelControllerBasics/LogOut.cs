using PanelController.Controller;
using PanelController.PanelObjects;
using PanelController.PanelObjects.Properties;

namespace PanelControllerBasics
{
    public class LogOut : IPanelAction
    {
        public Logger.Levels Level { get; set; } = Logger.Levels.Debug;

        public string Sender { get; set; } = "";

        public string Message { get; set; } = "";

        [ItemName]
        public string Name { get => $"LogOut ({Sender}) {Message}"; }

        [UserConstructor("Create an action that when ran, logs out a message to the PanelController Logger.")]
        public LogOut(Logger.Levels level,  string sender, string message)
        {
            Level = level;
            Sender = sender;
            Message = message;
        }

        public object? Run()
        {
            Logger.Log(Message, Level, $"{typeof(LogOut).Name}.{Sender}");
            return null;
        }
    }
}

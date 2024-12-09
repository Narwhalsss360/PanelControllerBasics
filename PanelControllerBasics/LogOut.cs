using PanelController.Controller;
using PanelController.PanelObjects;
using PanelController.PanelObjects.Properties;

namespace PanelControllerBasics
{
    public class LogOut : IPanelAction
    {
        [UserProperty]
        public Logger.Levels Level { get; set; } = Logger.Levels.Debug;

        [UserProperty]
        public string Sender { get; set; } = "";

        [UserProperty]
        public string Message { get; set; } = "";

        [ItemName]
        public string Name { get => $"LogOut ({Sender}) {Message}"; }

        public LogOut() { }

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

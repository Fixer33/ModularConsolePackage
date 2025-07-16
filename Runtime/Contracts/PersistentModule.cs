using UnityEngine.UIElements;

namespace ModularConsole.Contracts
{
    public abstract class PersistentModule : IConsoleModule
    {
        public virtual int ConsoleTabOrder => 0;
        public abstract string ModuleName { get; }
        public abstract void ConstructUI(VisualElement root);

        public PersistentModule()
        {
        }
    }
}
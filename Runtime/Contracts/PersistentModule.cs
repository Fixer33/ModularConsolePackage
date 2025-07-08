using UnityEngine.UIElements;

namespace ModularConsole.Contracts
{
    public abstract class PersistentModule : IConsoleModule
    {
        public abstract string ModuleName { get; }
        public abstract void ConstructUI(VisualElement root);

        internal PersistentModule()
        {
        }
    }
}
using UnityEngine.UIElements;

namespace ModularConsole.Contracts
{
    public interface IConsoleModule
    {
        public int ConsoleTabOrder { get; }
        public string ModuleName { get; }
        public bool NeedsUpdate => false;

        public void ConstructUI(VisualElement root);
        public void Update(){}
    }
}
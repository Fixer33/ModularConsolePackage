using UnityEngine.UIElements;

namespace ModularConsole.Contracts
{
    public interface IConsoleModule
    {
        public string ModuleName { get; }
        public bool NeedsUpdate => false;

        public void ConstructUI(VisualElement root);
        public void Update(){}
    }
}
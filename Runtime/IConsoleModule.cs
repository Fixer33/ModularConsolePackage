using UnityEngine.UIElements;

namespace ModularConsole
{
    public interface IConsoleModule
    {
        public string ModuleName { get; }
        
        public void ConstructUI(VisualElement root);
    }
}
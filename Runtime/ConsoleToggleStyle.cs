using UnityEngine.UIElements;

namespace ModularConsole
{
    /// <summary>
    /// Turns a regular <see cref="Toggle"/> into the pill-shaped toggle of the console style:
    /// adds a knob to the checkmark and mirrors the value into a USS class (see ModularConsoleStyles.uss)
    /// </summary>
    internal static class ConsoleToggleStyle
    {
        public const string TOGGLE_CLASS_NAME = "logFilterToggle";
        public const string SMALL_CLASS_NAME = "toggle--small";
        private const string ON_CLASS_NAME = "toggle--on";
        private const string KNOB_CLASS_NAME = "toggle__knob";
        private const string CHECKMARK_NAME = "unity-checkmark";

        public static void Apply(Toggle toggle, bool small = false)
        {
            toggle.AddToClassList(TOGGLE_CLASS_NAME);
            if (small)
                toggle.AddToClassList(SMALL_CLASS_NAME);

            var checkmark = toggle.Q(CHECKMARK_NAME);
            if (checkmark != null && checkmark.Q(className: KNOB_CLASS_NAME) == null)
            {
                var knob = new VisualElement { pickingMode = PickingMode.Ignore };
                knob.AddToClassList(KNOB_CLASS_NAME);
                checkmark.Add(knob);
            }

            toggle.UnregisterValueChangedCallback(OnValueChanged);
            toggle.RegisterValueChangedCallback(OnValueChanged);
            Refresh(toggle, toggle.value);
        }

        private static void OnValueChanged(ChangeEvent<bool> evt)
        {
            if (evt.currentTarget is Toggle toggle)
                Refresh(toggle, evt.newValue);
        }

        /// <summary>
        /// Call after changing <see cref="BaseField{T}.value"/> without notification, e.g. when a pooled tree row is rebound
        /// </summary>
        public static void Refresh(Toggle toggle, bool value)
        {
            toggle.EnableInClassList(ON_CLASS_NAME, value);
        }
    }
}

using System.Collections.Generic;


namespace FluentSensors.Persistence.Models
{
    // persisted position and size for one window
    // the dictionary key in window-state.json identifies which window this belongs to (e.g. "Main", "Widget")
    public class WindowState
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsMaximized { get; set; }

        // WidgetWindow only: whether it was open when the app last closed, and which sensors were pinned, so it can be
        // automatically restored with the same sensors on next launch
        public bool WasOpen { get; set; }
        public List<string> PinnedSensorIds { get; set; } = new();
    }
}
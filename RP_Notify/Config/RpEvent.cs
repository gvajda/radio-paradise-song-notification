using System;

namespace RP_Notify.Config
{
    public class RpEvent : EventArgs
    {
        public enum EventType
        {
            ConfigChange,
            StateChange,
            RpTrackingConfigChange
        }

        public EventType SentEventType { get; set; }
        public string ChangedFieldName { get; set; }
        public bool? BoolValue { get; set; }

        public RpEvent(EventType eventType, string changedFieldName, bool? boolValue = null)
        {
            SentEventType = eventType;
            ChangedFieldName = changedFieldName;
            BoolValue = boolValue;
        }
    }
}

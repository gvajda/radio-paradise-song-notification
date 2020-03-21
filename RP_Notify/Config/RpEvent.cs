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
        public object Content { get; set; }

        public RpEvent(EventType eventType, string changedFieldName, object content)
        {
            SentEventType = eventType;
            ChangedFieldName = changedFieldName;
            Content = content;
        }
    }
}

using System;

namespace RP_Notify.Config
{
    public class RpConfigurationChangeEvent : EventArgs
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

        public RpConfigurationChangeEvent(EventType eventType, string changedFieldName, object content)
        {
            SentEventType = eventType;
            ChangedFieldName = changedFieldName;
            Content = content;
        }
    }
}

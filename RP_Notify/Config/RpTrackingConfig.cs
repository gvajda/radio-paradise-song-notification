using RP_Notify.RpApi.ResponseModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace RP_Notify.Config
{
    public class RpTrackingConfig
    {
        public event EventHandler<RpConfigurationChangeEvent> RpTrackingConfigChangeHandler = delegate { };

        private string activePlayerId;
        private IList<Player> players;

        public string ActivePlayerId
        {
            get => activePlayerId;
            set
            {
                if (activePlayerId != value)
                {
                    activePlayerId = value;
                    RaiseFieldChangeEvent(nameof(ActivePlayerId), value);
                }
            }
        }

        public IList<Player> Players
        {
            get => FormatSource(players);
            set
            {
                if (value != null && ComparePlayerList(players, value))
                {
                    players = value;
                    RaiseFieldChangeEvent(nameof(Players), value);
                }
            }
        }

        public RpTrackingConfig()
        {
            ActivePlayerId = null;
            Players = new List<Player>();
        }

        private void RaiseFieldChangeEvent(string fieldName, params object[] flexibleValue)
        {
            object value = flexibleValue[0] != null
                ? flexibleValue[0]
                : "null";

            RpTrackingConfigChangeHandler.Invoke(this, new RpConfigurationChangeEvent(RpConfigurationChangeEvent.EventType.RpTrackingConfigChange, fieldName, value));
        }

        private bool ComparePlayerList(IList<Player> oldPlayerList, IList<Player> newPlayerList)
        {
            bool newIsEmpty = newPlayerList == null || !newPlayerList.Any();
            bool oldIsEmpty = oldPlayerList == null || !oldPlayerList.Any();

            if (newIsEmpty && oldIsEmpty)
            {
                return false;
            }

            if ((newIsEmpty && !oldIsEmpty)
                || (!newIsEmpty && oldIsEmpty))
            {
                return true;
            }

            var pl1ex2 = oldPlayerList.Select(p => String.Concat(p.PlayerId, p.Chan))
                .Except(newPlayerList.Select(p => String.Concat(p.PlayerId, p.Chan)))
                .Any();
            var pl2ex1 = newPlayerList.Select(p => String.Concat(p.PlayerId, p.Chan))
                .Except(oldPlayerList.Select(p => String.Concat(p.PlayerId, p.Chan)))
                .Any();

            return pl1ex2 || pl2ex1;
        }

        private IList<Player> FormatSource(IList<Player> input)
        {
            if (input == null)
            {
                return new List<Player>();
            }

            foreach (Player player in input)
            {
                player.Source = CustomFormat(player.Source);
            }
            return input;
        }

        private string CustomFormat(string inputString)
        {
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;
            var breakdown = inputString.Split(" ".ToCharArray());
            var formatted = breakdown.Select(word =>
                word = word.Any(char.IsUpper)
                    ? word
                    : textInfo.ToTitleCase(word));
            return string.Join(" ", formatted);
        }
    }
}

using RP_Notify.API.ResponseModel;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace RP_Notify.RP_Tracking
{
    public class RpTrackingConfig
    {
        public bool Enabled { get; set; }

        public string ActivePlayerId { get; set; }

        private IList<Player> players;
        public IList<Player> Players
        {
            get => players;
            set => players = FormatSource(value);
        }

        public RpTrackingConfig()
        {
            ActivePlayerId = null;
            Enabled = false;
            Players = new List<Player>();
        }

        public bool IsValidPlayerId()
        {
            return Enabled && !string.IsNullOrEmpty(ActivePlayerId) && Players.Any(p => p.PlayerId == ActivePlayerId);
        }

        public IList<Player> FormatSource(IList<Player> input)
        {
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace RP_Notify.PlayerWatchers
{
    internal class PlayerWatcherProvider : IPlayerWatcherProvider

    {
        private readonly Func<IEnumerable<IPlayerWatcher>> _playerWatcherCreators;

        public PlayerWatcherProvider(Func<IEnumerable<IPlayerWatcher>> playerWatcherCreators)
        {
            _playerWatcherCreators = playerWatcherCreators;
        }

        public IPlayerWatcher GetWatcher(RegisteredPlayer registeredPlayer)
        {
            return _playerWatcherCreators()
                .Where(x => x.PlayerWatcherType == registeredPlayer)
                .FirstOrDefault();
        }
    }


}

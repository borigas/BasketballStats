using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasketballStats.Shared.DataContracts;

namespace BasketballStats.Shared.Contracts
{
    public interface IGameClockEngine
    {
        void StartClock(Game game);

        void StopClock(Game game);

        GameTime GetGameTime(Game game);

        void SetEllapsedTime(Game game, int periodIndex, TimeSpan timeSpan);

        void AdvancePeriod(Game game);
    }
}

using Lite;
using Lite.Caching;

namespace TestPhotonApp
{
    public class TestGameCache : RoomCacheBase
    {
        /// <summary>
        /// The singleton instance.
        /// </summary>
        public static readonly TestGameCache Instance = new TestGameCache();

        protected override Room CreateRoom(string roomId, params object[] args)
        {
            return new TestGame(roomId);
        }
    }
}

using UnityEngine;

namespace RacingOnline
{
    public enum PlayerType
    {
        HOST,
        CLIENT
    }
    [CreateAssetMenu(menuName = "RacingOnline/Settings")]
    public class Settings : ScriptableObject
    {
        public string playerName;
        public PlayerType playerType;
        public int maxPlayers;
        public string sessionName;
        public string[] colorHexs;

        public void Reset()
        {
            playerName = string.Empty;
            playerType = PlayerType.HOST;
            maxPlayers = -1;
            sessionName = string.Empty;
        }
    }
}
using Fusion;

namespace RacingOnline.Race
{
    /// <summary>
    /// This entire script is referenced from the Fusion SDK's Karts example scene
    /// </summary>
    public static class TickHelper
    {
        public static float TickToSeconds(NetworkRunner runner, int ticks)
        {
            return ticks * runner.DeltaTime;
        }
    }
}
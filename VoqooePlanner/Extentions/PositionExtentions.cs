using EliteJournalReader;
using ODUtils.Models;

namespace VoqooePlanner.Extentions
{
   public static class PositionExtentions
    {
        public static SystemPosition ToSystemPosition(this Position pos)
        {
            return new SystemPosition() { X = pos.X, Y = pos.Y, Z = pos.Z };
        }
    }
}

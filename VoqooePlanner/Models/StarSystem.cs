using ODUtils.Models;

namespace VoqooePlanner.Models
{
    public sealed class StarSystem(VoqooeSystem s) : IComparable, IEquatable<object>
    {
        public long Address { get; } = s.Address;
        public string Name { get; } = s.Name;
        public Position Pos { get; } = new(s.X, s.Y, s.Z);

        public static bool operator ==(StarSystem? obj1, StarSystem? obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;
            if (obj1 is null)
                return false;
            if (obj2 is null)
                return false;
            return obj1.Name.Equals(obj2.Name);
        }

        public static bool operator !=(StarSystem? obj1, StarSystem? obj2) => !(obj1 == obj2);

        public int CompareTo(object? obj)
        {
            if (obj is not null && obj is StarSystem other && other.Name is not null && this.Name is not null)
            {
                return string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
            }

            return 1;
        }

        public bool Compare(VoqooeSystem? other)
        {
            return Equals(other);
        }

        public override bool Equals(object? obj)
        {
            return CompareTo(obj) == 0;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name?.GetHashCode(), Pos.GetHashCode());
        }
    }
}

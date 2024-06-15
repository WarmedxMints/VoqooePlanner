using EliteJournalReader;
using EliteJournalReader.Events;
using ODUtils.Models;

namespace VoqooePlanner.Models
{
    public sealed class StarSystem : IComparable, IEquatable<object>
    {
        public StarSystem(VoqooeSystem s)
        {
            Address = s.Address;
            Name = s.Name;
            Pos = new(s.X, s.Y, s.Z);
            StarType = (StarType)s.StarType;
        }

        public StarSystem(long address, string name, SystemPosition pos, StarType starType)
        {
            Address = address;
            Name = name;
            Pos = new(pos.X, pos.Y, pos.Z);
            StarType = starType;
        }

        public long Address { get; }
        public string Name { get; }
        public Position Pos { get; } 
        public StarType StarType { get; }
        public int BodyCount { get; set; }
        public List<SystemBody> SystemBodies { get; private set; } = [];

        public void AddBody(ScanEvent.ScanEventArgs args, bool ody = true)
        {
            var known = SystemBodies.FirstOrDefault(x => x.BodyID == args.BodyID);

            if (known != null)
            {
                known.UpdateFromScan(args);
                return;
            }

            var bodyToAdd = new SystemBody(args, this);
            bodyToAdd.CalcValues(ody);
            SystemBodies.Add(bodyToAdd);
        }


        public void UpdateBodyFromDSS(SAAScanCompleteEvent.SAAScanCompleteEventArgs args)
        {
            var known = SystemBodies.FirstOrDefault(x => x.BodyID == args.BodyID);

            if (known != null)
            {
                known.UpdateFromDSS(args);
                return;
            }
        }

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

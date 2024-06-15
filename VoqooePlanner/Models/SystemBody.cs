using EliteJournalReader;
using EliteJournalReader.Events;

namespace VoqooePlanner.Models
{
    public sealed class SystemBody
    {
        public StarSystem Owner { get; }
        public double OrbitalPeriod { get; private set; }
        public double RotationPeriod { get; private set; }
        public double Radius { get; private set; }
        public bool TidalLock { get; private set; }
        public string Volcanism { get; private set; }
        public StarType StarType { get; private set; }
        public PlanetClass PlanetClass { get; private set; }
        public double StellarMass { get; private set; }
        public double MassEM { get; private set; }
        public double SurfaceGravity { get; private set; }
        public double SurfacePressure { get; private set; }
        public double SurfaceTemp { get; private set; }
        public AtmosphereClass AtmosphereType { get; private set; }
        public int FssValue { get; private set; }
        public int MappedValueMin { get; private set; }
        public int MappedValue { get; private set; }
        public TerraformState TerraformState { get; private set; }
        public string BodyName { get; private set; }
        public long BodyID { get; }
        public double DistanceFromArrivalLs { get; private set; }
        public DiscoveryStatus Status { get; private set; }
        public bool Landable { get; private set; }
        public bool WasMapped { get; private set; }
        public bool WasDiscovered { get; private set; }
        public bool MappedByUser { get; private set; }
        public bool EffeicentMapped { get; private set; }
        public int GeologicalSignals { get; private set; }
        public int BiologicalSignals { get; private set; }

        public bool IsStar => StarType != StarType.Unknown;        
        public bool IsPlanet => PlanetClass != PlanetClass.Unknown;
        public bool Terraformable { get { return TerraformState is TerraformState.Terraformable or TerraformState.Terraforming or TerraformState.Terraformed; } }
        public SystemBody(ScanEvent.ScanEventArgs e, StarSystem owner)
        {
            Owner = owner;
            OrbitalPeriod = e.OrbitalPeriod ?? 0;
            RotationPeriod = e.RotationPeriod ?? 0;
            Radius = e.Radius ?? 0;
            TidalLock = e.TidalLock ?? false;
            Volcanism = string.IsNullOrEmpty(e.Volcanism) ? "No Volcanism" : e.Volcanism; 
            StarType = e.StarType;
            PlanetClass = e.PlanetClass;
            StellarMass = e.StellarMass ?? 0;
            MassEM = e.MassEM ?? 0;
            SurfaceGravity = e.SurfaceGravity ?? 0;
            SurfacePressure = e.SurfacePressure ?? 0;
            SurfaceTemp = e.SurfaceTemperature ?? 0;
            AtmosphereType = e.AtmosphereType;
            TerraformState = e.TerraformState;
            BodyName = e.BodyName;
            BodyID = e.BodyID;
            DistanceFromArrivalLs = e.DistanceFromArrivalLs;
            Landable = e.Landable ?? false;
            WasMapped = e.WasMapped ?? false;
        }

        public void UpdateFromScan(ScanEvent.ScanEventArgs e, bool ody = true)
        {
            BodyName = e.BodyName;
            DistanceFromArrivalLs = e.DistanceFromArrivalLs;
            StarType = e.StarType;
            StellarMass = e.StellarMass ?? 0;
            PlanetClass = e.PlanetClass;
            MassEM = e.MassEM ?? 0;
            TerraformState = e.TerraformState;
            AtmosphereType = e.AtmosphereType;
            Landable = e.Landable ?? false;
            SurfaceGravity = e.SurfaceGravity ?? 0;
            SurfacePressure = e.SurfacePressure ?? 0;
            SurfaceTemp = e.SurfaceTemperature ?? 0;
            Volcanism = e.Volcanism;
            TidalLock = e.TidalLock ?? false;
            OrbitalPeriod = e.OrbitalPeriod ?? 0;
            RotationPeriod = e.RotationPeriod ?? 0;
            Radius = e.Radius ?? 0;
           
            WasDiscovered = e.WasDiscovered ?? false;
            //Whan a body is mapped by the user a scan event is fired
            //For some reason that scan event can report the body as not mapped when it has been previously
            //So, if the user has just mapped a body, we ignore what the scan event has to say about this.
            if (!MappedByUser)
            {
                WasMapped = e.WasMapped ?? false;
            }

            CalcValues(ody);
        }

        public void CalcValues(bool odyssey)
        {
            if (IsStar)
            {
                FssValue = MappedValue = ODUtils.Maths.BodyValue.GetStarValue(StarType, StellarMass);

                return;
            }
            if (IsPlanet)
            {
                FssValue = ODUtils.Maths.BodyValue.GetBodyValue(StarType.Unknown, PlanetClass, StellarMass, MassEM, WasDiscovered, WasMapped, Terraformable, odyssey, false, false);

                MappedValueMin = ODUtils.Maths.BodyValue.GetBodyValue(StarType.Unknown, PlanetClass, StellarMass, MassEM, WasDiscovered, WasMapped, false, odyssey, true, true);
                MappedValue = ODUtils.Maths.BodyValue.GetBodyValue(StarType.Unknown, PlanetClass, StellarMass, MassEM, WasDiscovered, WasMapped, Terraformable, odyssey, true, true);
            }
        }

        public void UpdateFromDSS(SAAScanCompleteEvent.SAAScanCompleteEventArgs args, bool ody = true)
        {
            MappedByUser = true;
            EffeicentMapped = args.ProbesUsed <= args.EfficiencyTarget;
            MappedValue = ODUtils.Maths.BodyValue.GetBodyValue(StarType.Unknown, PlanetClass, StellarMass, MassEM, WasDiscovered, WasMapped, Terraformable, ody, true, EffeicentMapped);
        }
    }
}

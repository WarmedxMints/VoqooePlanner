using ODUtils.Extensions;
using VoqooePlanner.Models;

namespace VoqooePlanner.ViewModels.ModelViews
{
    public sealed class SystemBodyViewModel(SystemBody body)
    {
        private readonly SystemBody body = body;

        public long Id => body.BodyID;
        public string IdString => Id.ToString("N0");
        public string Name => body.BodyName;
        public double ArrivalValue => body.DistanceFromArrivalLs;
        public string Arrival => ArrivalValue.ToString("N0");
        public long UserValue => body.MappedByUser ? body.MappedValue : body.FssValue;
        public string StringValue => UserValue > 0 ? UserValue.ToString("N0") : string.Empty;
        public string BodyClass => body.IsPlanet ? body.PlanetClass.GetDescription() : body.IsStar ? body.StarType.GetDescription() : "-";
        public bool IsNonBody => !body.IsPlanet && !body.IsStar;
        public bool IsStar => body.IsStar;
        public bool IsPlanet => body.IsPlanet;
        public string OrbitalPeriod => $"{body.OrbitalPeriod:N1} d";
        public string RotationPeriod => $"{body.RotationPeriod:N1} d";
        public string Radius => $"{body.Radius:N0} km";
        public string EarthMasses => $"{body.MassEM}";
        public string Gravity => $"{body.SurfaceGravity:N3} G";
        public string AtmosphereType => body.AtmosphereType.GetDescription();
        public string SurfaceTemp => $"{body.SurfaceTemp:N0} K ({body.SurfaceTemp - 273.15:N0} °C)";
        public string SurfacePressure => $"{body.SurfacePressure:N0} pa ({body.SurfacePressure / 101325:N2} atm)";
        public string SolarMasses => body.StellarMass.ToString("N3");
        public string TidalLock => body.TidalLock ? "Yes" : "No";
        public string AxialTilt => $"{body.AxialTilt:N3}°";
        public string StarLuminosity => body.StarLuminosity.GetDescription();
        public string AbsoluteMagnitude => $"{body.AbsoluteMagnitude:N4}";
        public string Age_MY => body.Age_MY.ToString("N0");
        public string BodyNameLocal
        {
            get
            {
                if (string.IsNullOrEmpty(body.Owner.Name))
                {
                    return body.BodyName;
                }

                return body.BodyName.StartsWith(body.Owner.Name, StringComparison.OrdinalIgnoreCase) && body.BodyName.Length > body.Owner.Name.Length
                    ? body.BodyName.Remove(0, body.Owner.Name.Length + 1)
                    : body.BodyName;
            }
        }
    }
}

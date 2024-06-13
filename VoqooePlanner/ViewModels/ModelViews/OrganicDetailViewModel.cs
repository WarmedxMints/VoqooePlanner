using VoqooePlanner.Helpers;
using VoqooePlanner.Models;

namespace VoqooePlanner.ViewModels.ModelViews
{
    public sealed class OrganicDetailViewModel(OrganicDetails organicDetails) : ViewModelBase
    {
        private readonly OrganicDetails organicDetails = organicDetails;

        public string Name => organicDetails.Name;
        public string BodyName => organicDetails.BodyName;

        public string Species_Local => organicDetails.Species_Local;
        public string Codex => organicDetails.SpeciesCodex;

        private OrganicInfo organicInfo => OrganicValues.GetOrganicInfo(Codex, Species_Local);
        public long Value => organicInfo.Value;

        public string ValueString => Value == 0 ? "Unknown" : $"{organicInfo.Value:N0}";
    }
}

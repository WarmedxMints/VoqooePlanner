using EliteJournalReader;
using VoqooePlanner.Models;

namespace VoqooePlanner.DTOs
{
    public static class DTOExtensions
    {
        public static IQueryable<JournalEntryDTO> EventTypeCompare(this IQueryable<JournalEntryDTO> query, int cmdrId, IEnumerable<JournalTypeEnum> types)
        {
            return query.Where(x => x.CommanderID == cmdrId && types.Contains((JournalTypeEnum)x.EventTypeId));
        }

        public static IQueryable<VoqooeSystemDTO> CheckWhatToInclude(this IQueryable<VoqooeSystemDTO> query, NearBySystemsOptions options, int commanderId)
        {
            //Nothing to do here
            if (options == NearBySystemsOptions.None)
            {
                return query;
            }
                        //Filter out user visisted
            var ret =  query.Where(x => !options.HasFlag(NearBySystemsOptions.ExcludeUserVisited) || !x.CommanderVisits.Any(x => x.Id == commanderId))
                        //Filter out visted unless we want to include ELWs and sytems with value
                        .Where(x => !options.HasFlag(NearBySystemsOptions.ExcludeVisited)
                            || x.Visited == false
                            || (options.HasFlag(NearBySystemsOptions.IncludeUnvisitedELWs) && x.ContainsELW == true)
                            || (options.HasFlag(NearBySystemsOptions.IncludeUnvisitedValue) && x.Value > 0));

            return ret;
        }

        public static IQueryable<VoqooeSystemDTO> FilterStars(this IQueryable<VoqooeSystemDTO> query, IEnumerable<int> filters)
        {
            //Nothing to do here
            if (filters is null || filters.Contains(-1))
            {
                return query;
            }
            //Filter out stars 
            var ret = query.Where(x => filters.Contains(x.StarType));
            return ret;
        }

        public static SettingsDTO? GetSettingDTO(this IEnumerable<SettingsDTO> settings, string id)
        {
            return settings.FirstOrDefault(x => x.Id == id);
        }
    }
}
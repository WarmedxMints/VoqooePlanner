using System.ComponentModel.DataAnnotations;
namespace VoqooePlanner.DTOs
{
    public class SettingsDTO
    {
        [Key]
        public required string Id { get; set; }
        public int? IntValue { get; set; }
        public double? DoubleValue { get; set; }
        public string? StringValue { get; set; }

        public static bool SettingsDtoToBool(SettingsDTO? dto, bool defaulValue = false)
        {
            if (dto == null)
                return defaulValue;

            return dto.IntValue > 0 || defaulValue;
        }

        public static int SettingsDtoToInt(SettingsDTO? dto, int defaultValue = 0)
        {
            if (dto == null)
                return defaultValue;

            return dto.IntValue ?? defaultValue;
        }

        public static TEnum SettingsDtoToEnum<TEnum>(SettingsDTO? dto, TEnum defaultValue = default) where TEnum : struct
        {
            if (dto == null)
                return defaultValue;

            return dto.IntValue is null ? defaultValue : (TEnum)(object)dto.IntValue;
        }

        public static double SettingsDtoToDouble(SettingsDTO? dto, double defaultValue = 0)
        {
            if (dto == null)
                return defaultValue;

            return dto.DoubleValue ?? defaultValue;
        }

        public static T SettingDtoToObject<T>(SettingsDTO? dto, T defaultValue)
        {
            if(dto == null) 
                return defaultValue;

            if(dto.StringValue is null)
                return defaultValue;

            return ODUtils.IO.Json.DeserialiseJsonFromString<T>(dto.StringValue) ?? defaultValue;
        }

        public static SettingsDTO IntToSettingsDTO(string propertyName, int intVale)
        {
            return new SettingsDTO()
            {
                Id = propertyName,
                IntValue = intVale
            };
        }

        public static SettingsDTO DoubleToSettingsDTO(string propertyName, double doubleVale)
        {
            return new SettingsDTO()
            {
                Id = propertyName,
                DoubleValue = doubleVale
            };
        }

        public static SettingsDTO EnumToSettingsDto<TEnum>(string propertyName, TEnum enumValue) where TEnum : struct
        {
            return new SettingsDTO()
            {
                Id = propertyName,
                IntValue = (int)(object)enumValue
            };
        }

        public static SettingsDTO BoolToSettingsDTO(string propertyName, bool doubleVale)
        {
            return new SettingsDTO()
            {
                Id = propertyName,
                IntValue = doubleVale ? 1 : 0
            };
        }

        public static SettingsDTO ObjectToDto<T>(string propertyName, T objectToAdd)
        {
            if (objectToAdd is null)
                return new SettingsDTO()
                {
                    Id = propertyName,
                };

            var json = ODUtils.IO.Json.SerialiseJsonToString(objectToAdd);
            return new SettingsDTO()
            {
                Id = propertyName,
                StringValue = json
            };
        }
    }
}

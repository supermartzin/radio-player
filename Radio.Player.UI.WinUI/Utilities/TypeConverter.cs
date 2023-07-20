using System.Globalization;

namespace Radio.Player.Services.Contracts.Utilities
{
    public static class TypeConverter
    {
        public static DateTime ToDateTime(string dateTimeString, string format = null, DateTime? defaultValue = null)
        {
            if (dateTimeString == null)
                return defaultValue ?? DateTime.Now;

            if (!string.IsNullOrEmpty(format))
            {
                if (DateTime.TryParseExact(dateTimeString,
                                           format,
                                           CultureInfo.CurrentCulture,
                                           DateTimeStyles.None,
                                           out DateTime parsed))
                    return parsed;
            }
            else
            {
                if (DateTime.TryParse(dateTimeString, out var parsed))
                    return parsed;
            }
            
            return defaultValue ?? DateTime.Now;
        }

        public static int ToInt(string numberString, int defaultValue = 0)
        {
            if (numberString == null)
                return defaultValue;

            if (int.TryParse(numberString, out int parsed))
                return parsed;

            try
            {
                return Convert.ToInt32(numberString);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
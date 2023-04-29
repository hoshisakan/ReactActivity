namespace Application.Module
{
    public class DateTimeTool
    {
        public static int CompareBothOfTime(DateTime first, DateTime second)
        {
            return DateTime.Compare(first, second);
        }

        public static string ConvertToUnixTime(DateTime dateTime)
        {
            return ((DateTimeOffset)dateTime).ToUnixTimeSeconds().ToString();
        }

        public static DateTime UnixTimeStampToDateTime(string unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(Convert.ToDouble(unixTimeStamp)).ToLocalTime();
            return dateTime;
        }
    }
}
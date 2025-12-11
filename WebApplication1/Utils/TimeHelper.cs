namespace ClassMate.Api.Utils
{
    public static class TimeHelper
    {
        private static readonly TimeZoneInfo VietnamZone =
            TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        // Trên Windows dùng ID này, Linux thì "Asia/Ho_Chi_Minh"

        public static DateTime ToVietnamTime(this DateTime utcTime)
        {
            // Nếu lỡ truyền vào Kind = Unspecified hoặc Local thì nên ép về UTC trước
            if (utcTime.Kind == DateTimeKind.Unspecified)
            {
                utcTime = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);
            }
            else if (utcTime.Kind == DateTimeKind.Local)
            {
                utcTime = utcTime.ToUniversalTime();
            }

            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, VietnamZone);
        }
    }
}

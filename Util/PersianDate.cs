using System;
using System.Globalization;
using SG.Core.Models;

namespace SG.Core
{
    public static class PersianDate
    {
        /// <summary>Parses "yyyy/M/d" (Shamsi) to a Gregorian DateTime. Out-of-range month/day are clamped.</summary>
        public static bool TryParse(string shamsi, out DateTime result)
        {
            result = default(DateTime);
            if (string.IsNullOrWhiteSpace(shamsi)) return false;

            var parts = shamsi.Trim().Split('/');
            int y, m, d;
            if (parts.Length != 3 || !int.TryParse(parts[0], out y) || !int.TryParse(parts[1], out m) || !int.TryParse(parts[2], out d))
                return false;

            var pc = new PersianCalendar();
            try
            {
                if (m < 1 || m > 12) m = 12;
                if (d < 1) d = 1;
                int max = pc.GetDaysInMonth(y, m);
                if (d > max) d = max;
                result = pc.ToDateTime(y, m, d, 0, 0, 0, 0);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        /// <summary>True when the Shamsi date is today or earlier. Unparseable dates use <paramref name="valueIfInvalid"/>.</summary>
        public static bool IsExpired(string shamsiEndDate, DateTime today, bool valueIfInvalid = false)
        {
            DateTime end;
            if (!TryParse(shamsiEndDate, out end)) return valueIfInvalid;
            return end <= today;
        }

        /// <summary>Registration date (today) and expiry (one year later), both Shamsi. Old Date.GetRegEndDate.</summary>
        public static SGRegDate Registration(DateTime now)
        {
            return new SGRegDate { RegisterDate = Format(now), ExpireDate = Format(now.AddYears(1)) };
        }

        public static string Format(DateTime gregorian)
        {
            var pc = new PersianCalendar();
            return string.Format("{0}/{1}/{2}", pc.GetYear(gregorian), pc.GetMonth(gregorian), pc.GetDayOfMonth(gregorian));
        }
    }
}

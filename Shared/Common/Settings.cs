using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketballStats.Shared.Common
{
    public static class Settings
    {
        public static string CurrentUser { get; set; }

        private static DateTime _dateTimeOverride;
        private static bool _isDateTimeOverridden = false;
        public static DateTime CurrentTime
        {
            get
            {
                if (_isDateTimeOverridden)
                {
                    return _dateTimeOverride;
                }
                else
                {
                    return DateTime.UtcNow;
                }
            }
            set
            {
                _dateTimeOverride = value;
                _isDateTimeOverridden = true;
            }
        }

        public static void ResetTimeOverride()
        {
            _isDateTimeOverridden = false;
        }
    }
}

using System;

namespace Flipdish.Recruiting.WebHookReceiver.Helpers
{
    public enum EtaResponse
    {
        None = 0,
        InMinutes = 1,
        TodayAt = 2,
        TomorrowAt = 3,
        OnDayAt = 4,
        AtDateTime = 5,
        OnDay = 6
    }

    public static class EtaResponseMethods
    {

        // 19 Feb
        // Feb 19
        public static string GetDateString(DateTime requestedTime)
        {
            return requestedTime.ToString($"dd MMM");
        }

        // 14:00
        // 2:00 PM
        public static string GetClocksToString(DateTime requestedTime)
        {
            return requestedTime.ToString("HH:mm");
        }

        public static string GetPreOrdered(DateTime requestedTime)
        {
            if (requestedTime.Date == DateTime.Now.Date)
            {
                return $"PREORDER FOR {EtaResponse.TodayAt}: {requestedTime}";
            } 
            if (requestedTime.Date == DateTime.Now.AddDays(1).Date)
            {
                return $"PREORDER FOR {EtaResponse.TomorrowAt}: {requestedTime}";
            }
            if (requestedTime.Date.Minute < DateTime.Now.Date.Minute)
            {
                return $"PREORDER FOR  {EtaResponse.InMinutes}: {requestedTime}";
            }
            {
                return $"PREORDER FOR {EtaResponse.AtDateTime}: {requestedTime}";
            }
        }
    }
}

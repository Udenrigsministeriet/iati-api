using System;

namespace Um.DataServices.XmlFilePaging
{
    public static class Program
    {
        internal static void Main()
        {
            try
            {
                XmlFilePager.PerformPaging(XmlFilePager.XmlFilePagingSettings.ReadFromAppConfig());
            }
            catch (Exception ex)
            {
                XmlFilePager.LogXmlFilePagingExceptionToEventLog(ex);
            } 
        }
    }
}

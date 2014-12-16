using NUnit.Framework;
using System;
using System.Configuration;
using Um.DataServices.XmlFilePaging;
using System.Diagnostics;
using System.Threading;

namespace Um.DataServices.Test.Integration
{
    [TestFixture]
    public class XmlFilePagingTest
    {
        ManualResetEvent waitEvent = new ManualResetEvent(false);
        EventLogEntry eventLogEntry = null;

        [Test]
        public void TestSettingsFromAppConfigThrows()
        {
            Assert.Throws<ConfigurationErrorsException>(() => XmlFilePager.XmlFilePagingSettings.ReadFromAppConfig(),
                "App setting 'SourceDocumentUri' is not a valid URI. Value: ");

            ConfigurationManager.AppSettings["SourceDocumentUri"] = "http://example.com/a-doc.xml";

            Assert.Throws<ConfigurationErrorsException>(() => XmlFilePager.XmlFilePagingSettings.ReadFromAppConfig(),
                "App setting 'XmlElementToPage' must not be empty for blank.");

            ConfigurationManager.AppSettings["XmlElementToPage"] = "iati-activity";

            Assert.Throws<ConfigurationErrorsException>(() => XmlFilePager.XmlFilePagingSettings.ReadFromAppConfig(),
                "App setting 'OutputFolder' is not an existing directory. Value: ");

            ConfigurationManager.AppSettings["OutputFolder"] = @"c:\";

            Assert.Throws<ConfigurationErrorsException>(() => XmlFilePager.XmlFilePagingSettings.ReadFromAppConfig(),
                "App setting 'OutputFileBaseUrl' is not a valid absolute URI. Value: ");

            ConfigurationManager.AppSettings["OutputFileBaseUrl"] = "http://example.com/files/";
            Assert.DoesNotThrow(() => XmlFilePager.XmlFilePagingSettings.ReadFromAppConfig());

            var actual = XmlFilePager.XmlFilePagingSettings.ReadFromAppConfig();

            Assert.That(actual.SourceDocumentUri.ToString(), Is.EqualTo("http://example.com/a-doc.xml"));
            Assert.That(actual.XmlElementToPage, Is.EqualTo("iati-activity"));
            Assert.That(actual.OutputFileNameBase, Is.EqualTo("page"));
            Assert.That(actual.OutputFolder, Is.EqualTo(@"c:\"));
            Assert.That(actual.OutputFileBaseUri.ToString(), Is.EqualTo("http://example.com/files/"));
            Assert.That(actual.NumberOfPages, Is.EqualTo(7));
        }

        [Test]
        public void TestWriteErrorToEventLog()
        {
            var exception = new ConfigurationErrorsException("Um.DataServices.Test.Integration.TestWriteErrorToEventLog",
                new Exception("An inner exception."));

            var eventLog = new EventLog
                {
                    Log = XmlFilePager.EventlogName,
                    Source = XmlFilePager.EventlogSourceName,
                    EnableRaisingEvents = true,
                };
            eventLog.EntryWritten += eventLog_EntryWritten;

            XmlFilePager.LogXmlFilePagingExceptionToEventLog(exception);

            var actual = waitEvent.WaitOne(10000);
            Assert.IsTrue(actual);

            Assert.IsNotNull(eventLogEntry);
            Assert.That(eventLogEntry.Source, Is.EqualTo(XmlFilePager.EventlogSourceName));
            Assert.That(eventLogEntry.TimeGenerated, Is.GreaterThan(DateTime.Now.AddSeconds(-10)));
            Assert.That(eventLogEntry.EntryType, Is.EqualTo(EventLogEntryType.Error));
        }

        void eventLog_EntryWritten(object sender, EntryWrittenEventArgs e)
        {
            if (e.Entry.Source != XmlFilePager.EventlogSourceName)
                return;

            eventLogEntry = e.Entry;
            waitEvent.Set();
        }
    }
}

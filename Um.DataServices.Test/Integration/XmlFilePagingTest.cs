using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Assert.That(eventLogEntry.EntryType, Is.EqualTo(EventLogEntryType.Error))
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

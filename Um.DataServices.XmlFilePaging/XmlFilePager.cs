﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Um.DataServices.XmlFilePaging
{
    public class XmlFilePager
    {
        public const string EventlogSourceName = "Um.DataServices.XmlFilePaging";
        public const string EventlogName = "Application";

        internal static void Main()
        {
            // Get configuration from App.config.
            /*const string testFileUri = @"file://C:\temp\um\TestActitities.xml";
            const string fullFileUri = @"file://C:\temp\um\Activities.xml";
            const string webUri = @"http://iatiquery.um.dk/Activities.ashx";
            */


            // 1. Read the newest file from http://iatiquery.um.dk/Activities.ashx
            // 3. Calculate page size and page the document.
            // 4. Write pages to files.
            // 5. Write metadata JSON-file.

            try
            {
                PerformPaging(XmlFilePagingSettings.ReadFromAppConfig());
            }
            catch (Exception ex)
            {
                LogXmlFilePagingExceptionToEventLog(ex);
            }
        }

        public static void LogXmlFilePagingExceptionToEventLog(Exception ex)
        {
            if (!EventLog.SourceExists(EventlogSourceName))
                EventLog.CreateEventSource(EventlogSourceName, EventlogName);

            var logMessage = string.Format("En error was encountered while running xml file paging. {0}" + Environment.NewLine,
                ex.Message);
            var innerException = ex.InnerException;
            while (innerException != null)
            {
                logMessage += Environment.NewLine + innerException.Message;
                innerException = innerException.InnerException;
            }
            logMessage += ex.StackTrace;

            EventLog.WriteEntry(EventlogSourceName, logMessage, EventLogEntryType.Error);
        }

        public static void PerformPaging(XmlFilePagingSettings settings)
        {
            // 1. Read the xml document from the configured URI.
            var sourceElements = ReadRootAndElementsFromXmlDocument(settings.SourceDocumentUri, settings.XmlElementToPage);

            // 2. The page size is the number of XML elements that will be put in each file.
            var pageSize = CalculatePageSize(sourceElements.Elements.Count, settings.NumberOfFilesToSplitInto);

            if (pageSize == 0)
            {
                throw new ApplicationException(string.Format("The source document did not contain any elements with the specifed name." +
                                                             "Source document URL: {0}, Element name: {1}", 
                                                             settings.SourceDocumentUri, settings.XmlElementToPage));
            }

            // 3. Page the source document elements.
            var indexedPages = PageAndIndexElements(sourceElements, pageSize);

            // 4. Write the paged elements into files in the configured folder.
            var fileNames = new List<string>();
            foreach (var page in indexedPages)
            {
                var fileName = string.Format("{0}{1}.xml", settings.OutputFileNameBase, page.Key);

                // Store file names for metadata report.
                fileNames.Add(fileName);

                var filePath = Path.Combine(settings.OutputFolder, fileName);

                // We use a temporary file and rename it, in order to minimize 
                // the time that we lock the target file.
                var temporaryFile = Path.Combine(settings.OutputFolder, "temporary-page.xml");

                using (var streamWriter = new StreamWriter(temporaryFile))
                    page.Value.Save(streamWriter);

                if (File.Exists(filePath))
                    File.Delete(filePath);
                File.Move(temporaryFile, filePath);
            }

            // 5. Generate metadata result file.
            var fileUrlList = fileNames.Select(fn => settings.OutputFileBaseUri.Combine(fn).ToString()).ToList();
            var metadata = new PagingMetadata(
                DateTime.UtcNow, 
                settings.NumberOfFilesToSplitInto, 
                fileUrlList, 
                settings.XmlElementToPage, 
                sourceElements.Elements.Count);
            var jsonMetadata = JsonConvert.SerializeObject(metadata, Newtonsoft.Json.Formatting.Indented);
            var metadataFilePath = Path.Combine(settings.OutputFolder, string.Format("{0}.metadata.json", settings.OutputFileNameBase));
            File.WriteAllText(metadataFilePath, jsonMetadata);
        }

        public static DocumentParts ReadRootAndElementsFromXmlDocument(Uri documentSource, string elementName)
        {
            using (var reader = XmlReader.Create(documentSource.AbsoluteUri))
            {
                return ReadRootAndElementsFromXmlDocument(reader, elementName);
            }
        }

        public static DocumentParts ReadRootAndElementsFromXmlDocument(XmlReader reader, string elementName)
        {
            var document = XDocument.Load(reader);
            var elements = document.Descendants(elementName).ToList();
            return new DocumentParts(document.Root, elements);
        }

        public static int CalculatePageSize(int elementCount, int numberOfFilesToSplitInto)
        {
            return (int)Math.Ceiling(elementCount / (double)numberOfFilesToSplitInto);
        }

        public static IEnumerable<KeyValuePair<int, XDocument>> PageAndIndexElements(DocumentParts parts, int pageSize)
        {
            return parts.Elements.Page(pageSize)
                .Select((elements, i) => new { Index = i + 1, Document = CreateNewXDocumentFromExistingRoot(parts.DocumentRoot, elements) })
                .ToDictionary(i => i.Index, i => i.Document);
        }

        public static XDocument CreateNewXDocumentFromExistingRoot(XElement root, IEnumerable<XElement> fileElements)
        {
            var newRootNode = new XElement(root.Name);
            newRootNode.Add(root.Attributes());
            newRootNode.Add(fileElements);
            return new XDocument(newRootNode);
        }

        public class DocumentParts
        {
            public readonly XElement DocumentRoot;
            public readonly IList<XElement> Elements;

            public DocumentParts(XElement documentRoot, IList<XElement> elements)
            {
                DocumentRoot = documentRoot;
                Elements = elements;
            }
        }

        public class PagingMetadata
        {
            public readonly DateTime GeneratedDateTimeUtc;
            public readonly int PageFileCount;
            public readonly string PagedElementName;
            public readonly int TotalElementCount;
            public readonly List<string> PagedFileUrls;

            public PagingMetadata(DateTime generatedDateTimeUtc, int pageFileCount, List<string> pagedFileUrls, string pagedElementName, int totalElementCount)
            {
                GeneratedDateTimeUtc = generatedDateTimeUtc;
                PageFileCount = pageFileCount;
                PagedFileUrls = pagedFileUrls;
                PagedElementName = pagedElementName;
                TotalElementCount = totalElementCount;
            }
        }

        public class XmlFilePagingSettings
        {
            public string XmlElementToPage { get; set; } 
            public Uri SourceDocumentUri { get; set; }
            public string OutputFileNameBase { get; set; }
            public string OutputFolder { get; set; }
            public Uri OutputFileBaseUri { get; set; }
            public int NumberOfFilesToSplitInto { get; set; }

            public static XmlFilePagingSettings ReadFromAppConfig()
            {
                var settings = new XmlFilePagingSettings();

                Uri sourceDocumentUri;
                if (!Uri.TryCreate(ConfigurationManager.AppSettings["SourceDocumentUri"], UriKind.RelativeOrAbsolute, out sourceDocumentUri))
                    throw new ConfigurationErrorsException(string.Format("App setting 'SourceDocumentUri' is not a valid URI. Value: {0}",
                        ConfigurationManager.AppSettings["SourceDocumentUri"]));
                settings.SourceDocumentUri = sourceDocumentUri;

                settings.XmlElementToPage = ConfigurationManager.AppSettings["XmlElementToPage"];
                if (string.IsNullOrWhiteSpace(settings.XmlElementToPage))
                    throw new ConfigurationErrorsException(string.Format("App setting 'XmlElementToPage' must not be empty for blank."));

                settings.OutputFileNameBase = ConfigurationManager.AppSettings["OutputFileNameBase"];
                if (string.IsNullOrWhiteSpace(settings.OutputFileNameBase))
                    settings.OutputFileNameBase = "Page";

                settings.OutputFolder = ConfigurationManager.AppSettings["OutputFolder"];
                if (!Directory.Exists(settings.OutputFolder))
                    throw new ConfigurationErrorsException(string.Format("App setting 'OutputFolder' is not an existing directory. Value: {0}",
                        settings.OutputFolder));

                Uri outputFileBaseUri;
                if (!Uri.TryCreate(ConfigurationManager.AppSettings["OutputFileBaseUrl"], UriKind.Absolute, out outputFileBaseUri))
                    throw new ConfigurationErrorsException(string.Format("App setting 'OutputFileBaseUrl' is not a valid absolute URI. Value: {0}", 
                        ConfigurationManager.AppSettings["OutputFileBaseUrl"]));
                settings.OutputFileBaseUri = outputFileBaseUri;

                int numberOfFilesToSplitInto;
                if (!int.TryParse(ConfigurationManager.AppSettings["NumberOfPages"], out numberOfFilesToSplitInto))
                    settings.NumberOfFilesToSplitInto = 7; // Revert to default value.
                settings.NumberOfFilesToSplitInto = numberOfFilesToSplitInto;

                return settings;
            }
        }
    }

    public static class Extensions
    {
        public static IEnumerable<IEnumerable<T>> Page<T>(this IEnumerable<T> elements, int pageSize)
        {
            return elements
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / pageSize)
                .Select(x => x.Select(v => v.Value));
        }

        public static Uri Combine(this Uri uri, string relativePath)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            if (relativePath == null || relativePath.Trim().Length == 0)
                return uri;

            var uriUrl = uri.ToString().TrimEnd('/', '\\');
            var pathUrl = relativePath.TrimStart('/', '\\');

            return new Uri(string.Format("{0}/{1}", uriUrl, pathUrl), UriKind.RelativeOrAbsolute);
        }
    }
}

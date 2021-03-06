﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace Um.DataServices.XmlFilePaging
{
    public static class XmlFilePager
    {
        public const string EventlogSourceName = "Um.DataServices.XmlFilePaging";
        public const string EventlogName = "Application";

        public static void LogXmlFilePagingExceptionToEventLog(Exception ex)
        {
            if (!EventLog.SourceExists(EventlogSourceName))
            {
                EventLog.CreateEventSource(EventlogSourceName, EventlogName);
            }

            var logMessage =
                string.Format("En error was encountered while running xml file paging. {0}" + Environment.NewLine,
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
            // 1a. Read the xml document from the configured URI.
            var sourceElements = ReadRootAndElementsFromXmlDocument(settings.SourceDocumentUri,
                settings.XmlElementToPage, settings.HttpRequestTimeout);

            // 1b. Check for duplicate identifiers in the source file.
            var sourceActivityIdentifies = ReadRootAndElementsFromXmlDocument(settings.SourceDocumentUri,
                settings.XmlElementIdentifier, settings.HttpRequestTimeout);
            var distinctCount = sourceActivityIdentifies.Elements.Distinct().Count();
            if (sourceElements.Elements.Count != distinctCount)
            {
                throw new ApplicationException("The source file contains duplicates of <iati-identifier>.");
            }

            // 2. The page size is the number of XML elements that will be put in each file.
            var pageSize = CalculatePageSize(sourceElements.Elements.Count, settings.NumberOfPages);

            if (pageSize == 0)
            {
                throw new ApplicationException(
                    "The source document did not contain any elements with the specifed name." +
                    $"Source document URL: {settings.SourceDocumentUri}, Element name: {settings.XmlElementToPage}");
            }

            // 3. Page the source document elements.
            var indexedPages = PageAndIndexElements(sourceElements, pageSize);

            // 4a. Write the paged elements into files in the configured folder.
            var fileNames = new List<string>();
            foreach (var page in indexedPages)
            {
                var fileName = $"{settings.OutputFileNameBase}{page.Key}.xml";

                // Store file names for metadata report.
                fileNames.Add(fileName);

                var filePath = Path.Combine(settings.OutputFolder, fileName);

                // We use a temporary file and rename it, in order to minimize 
                // the time that we lock the target file.
                var temporaryFile = Path.Combine(settings.OutputFolder, "temporary-page.xml");

                using (var streamWriter = new StreamWriter(temporaryFile))
                {
                    page.Value.Save(streamWriter);
                }

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                File.Move(temporaryFile, filePath);
            }

            var fileCheckDict = new Dictionary<string, int>();
            foreach (var fileName in fileNames)
            {
                var fileUri = new Uri(Path.Combine(settings.OutputFolder, fileName));
                var elements =
                    ReadRootAndElementsFromXmlDocument(fileUri, "iati-identifier", settings.HttpRequestTimeout);
                fileCheckDict.Add(fileName, elements.Elements.Count);
            }

            // 4b. Also write the original file to the folder.
            var originalDocument = CreateNewXDocumentFromExistingRoot(sourceElements.DocumentRoot,
                sourceElements.Elements);
            var originalFileName = $"{settings.OutputFileNameBase}.xml";
            var originalFilePath = Path.Combine(settings.OutputFolder, originalFileName);
            var originalFileUrl = settings.OutputFileBaseUri.Combine(originalFileName).ToString();
            using (var streamWriter = new StreamWriter(originalFilePath))
            {
                originalDocument.Save(streamWriter);
            }

            // 5. Generate metadata result file.
            var pagedFileInfos = fileNames.Select(fn =>
                new PagedFileInfo
                {
                    Url = settings.OutputFileBaseUri.Combine(fn).ToString(),
                    ElementCount = fileCheckDict[fn]
                }
            ).ToList();
            var metadata = new PagingMetadata(
                DateTime.UtcNow,
                originalFileUrl,
                settings.NumberOfPages,
                pagedFileInfos,
                settings.XmlElementToPage,
                sourceElements.Elements.Count);
            var jsonMetadata = JsonConvert.SerializeObject(metadata, Formatting.Indented);
            var metadataFilePath = Path.Combine(settings.OutputFolder,
                $"{settings.OutputFileNameBase}.metadata.json");
            File.WriteAllText(metadataFilePath, jsonMetadata);
        }

        public static DocumentParts ReadRootAndElementsFromXmlDocument(Uri documentSource, string elementName,
            int httpRequestTimeout)
        {
            var request = WebRequest.Create(documentSource.AbsoluteUri);

            // ms/sek * sek/min * min 
            request.Timeout = 1000 * 60 * httpRequestTimeout;

            using (var response = request.GetResponse())
            {
                var stream = response.GetResponseStream();
                if (stream == null)
                {
                    throw new Exception(
                        "Unable to read IATI Activities XML document from service. Response stream is null.");
                }

                using (var reader = XmlReader.Create(stream))
                {
                    return ReadRootAndElementsFromXmlDocument(reader, elementName);
                }
            }
        }

        public static DocumentParts ReadRootAndElementsFromXmlDocument(XmlReader reader, string elementName)
        {
            var document = XDocument.Load(reader);
            var elements = document.Descendants(elementName).ToList();
            return new DocumentParts(document.Root, elements);
        }

        public static int CalculatePageSize(int elementCount, int numberOfPages)
        {
            return (int) Math.Ceiling(elementCount / (double) numberOfPages);
        }

        public static IEnumerable<KeyValuePair<int, XDocument>> PageAndIndexElements(DocumentParts parts, int pageSize)
        {
            return parts.Elements.Page(pageSize)
                .Select(
                    (elements, i) =>
                        new
                        {
                            Index = i + 1,
                            Document = CreateNewXDocumentFromExistingRoot(parts.DocumentRoot, elements)
                        })
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
            public readonly string OriginalFileUrl;
            public readonly string PagedElementName;
            public readonly List<PagedFileInfo> PagedFileInfo;
            public readonly int PageFileCount;
            public readonly int TotalElementCount;

            public PagingMetadata(DateTime generatedDateTimeUtc, string originalFileUrl, int pageFileCount,
                List<PagedFileInfo> pagedFileInfos, string pagedElementName, int totalElementCount)
            {
                GeneratedDateTimeUtc = generatedDateTimeUtc;
                OriginalFileUrl = originalFileUrl;
                PageFileCount = pageFileCount;
                PagedFileInfo = pagedFileInfos;
                PagedElementName = pagedElementName;
                TotalElementCount = totalElementCount;
            }
        }

        public class XmlFilePagingSettings
        {
            public string XmlElementToPage { get; set; }
            public string XmlElementIdentifier { get; set; }
            public Uri SourceDocumentUri { get; set; }
            public string OutputFileNameBase { get; set; }
            public string OutputFolder { get; set; }
            public Uri OutputFileBaseUri { get; set; }
            public int NumberOfPages { get; set; }

            public int HttpRequestTimeout { get; set; }

            public static XmlFilePagingSettings ReadFromAppConfig()
            {
                var settings = new XmlFilePagingSettings();

                if (
                    !Uri.TryCreate(ConfigurationManager.AppSettings["SourceDocumentUri"], UriKind.RelativeOrAbsolute,
                        out var sourceDocumentUri))
                {
                    throw new ConfigurationErrorsException(
                        $"App setting 'SourceDocumentUri' is not a valid URI. Value: {ConfigurationManager.AppSettings["SourceDocumentUri"]}");
                }

                settings.SourceDocumentUri = sourceDocumentUri;

                settings.XmlElementToPage = ConfigurationManager.AppSettings["XmlElementToPage"];
                if (string.IsNullOrWhiteSpace(settings.XmlElementToPage))
                {
                    throw new ConfigurationErrorsException(
                        "App setting \'XmlElementToPage\' must be specified.");
                }

                settings.XmlElementIdentifier = ConfigurationManager.AppSettings["XmlElementIdentifier"];
                if (string.IsNullOrWhiteSpace(settings.XmlElementIdentifier))
                {
                    throw new ConfigurationErrorsException(
                        "App setting \'XmlElementIdentifier\' must be specified.");
                }

                settings.OutputFileNameBase = ConfigurationManager.AppSettings["OutputFileNameBase"];
                if (string.IsNullOrWhiteSpace(settings.OutputFileNameBase))
                {
                    settings.OutputFileNameBase = "page";
                }

                settings.OutputFolder = ConfigurationManager.AppSettings["OutputFolder"];
                if (!Directory.Exists(settings.OutputFolder))
                {
                    throw new ConfigurationErrorsException(
                        $"App setting 'OutputFolder' is not an existing directory. Value: {settings.OutputFolder}");
                }

                if (
                    !Uri.TryCreate(ConfigurationManager.AppSettings["OutputFileBaseUrl"], UriKind.Absolute,
                        out var outputFileBaseUri))
                {
                    throw new ConfigurationErrorsException(
                        $"App setting 'OutputFileBaseUrl' is not a valid absolute URI. Value: {ConfigurationManager.AppSettings["OutputFileBaseUrl"]}");
                }

                settings.OutputFileBaseUri = outputFileBaseUri;

                if (int.TryParse(ConfigurationManager.AppSettings["NumberOfPages"], out var numberOfPages))
                {
                    settings.NumberOfPages = numberOfPages;
                }
                else
                {
                    settings.NumberOfPages = 7; // Revert to default value.
                }

                if (int.TryParse(ConfigurationManager.AppSettings["HttpRequestTimeout"], out var httpRequestTimeout))
                {
                    settings.HttpRequestTimeout = httpRequestTimeout;
                }
                else
                {
                    settings.HttpRequestTimeout = 30; // Revert to default value.
                }

                return settings;
            }
        }
    }

    public class PagedFileInfo
    {
        public string Url { get; set; }
        public int ElementCount { get; set; }
    }

    public static class Extensions
    {
        public static IEnumerable<IEnumerable<T>> Page<T>(this IEnumerable<T> elements, int pageSize)
        {
            return elements
                .Select((x, i) => new {Index = i, Value = x})
                .GroupBy(x => x.Index / pageSize)
                .Select(x => x.Select(v => v.Value));
        }

        public static Uri Combine(this Uri uri, string relativePath)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if ((relativePath == null) || (relativePath.Trim().Length == 0))
            {
                return uri;
            }

            var uriUrl = uri.ToString().TrimEnd('/', '\\');
            var pathUrl = relativePath.TrimStart('/', '\\');

            return new Uri($"{uriUrl}/{pathUrl}", UriKind.RelativeOrAbsolute);
        }
    }
}
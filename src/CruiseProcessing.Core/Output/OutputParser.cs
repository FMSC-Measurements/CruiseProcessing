using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CruiseProcessing.Output
{
    public class OutputParser
    {
        protected static Regex REPORT_PAGE_REGEX = new Regex(@"(^(?<reportID>[\w\d]{2,4}): (?<reportTitle>([\w\p{P}]+ )+)? *PAGE (?<pageNumber>\d+)(?:\r\n?|\n))(?<reportSubtitle>(^( ?[\w\(\)\d]+ ?)+(?:\r\n?|\n)){0,2})(^CRUISE#: (?<cruiseNumber>\d+) +SALE#: (?<saleNumber>\d+)(?:\r\n?|\n))(^SALENAME: (?<saleName>[\w\d _-]+) +VERSION: (?<cpVersion>(\d+\.\d+\.\d+)|(DRAFT\.\d+))(?:\r\n?|\n))(RUN DATE & TIME: (?<runDateTime>\d+\/\d+\/\d+\s\d+:\d+:\d+\s\w+) +VOLUME LIBRARY VERSION: (?<volLibVersion>[\w\d\. ]+(?:\r\n?|\n)))(?:\r\n?|\n)+(?<reportContent>([ \d\w\t\p{P}\p{S}]*(?:\r\n?|\n))+)"
                , RegexOptions.Multiline | RegexOptions.Compiled, TimeSpan.FromSeconds(1));


        public static IEnumerable<ReportPage> ExtractReportPages(string text)
        {

            var pages = SplitPages(text);
            foreach (var page in pages)
            {
                var content = page.Content;
                Match match = null;
                try
                {
                    match = REPORT_PAGE_REGEX.Match(content);
                }
                catch (RegexMatchTimeoutException e)
                {
                    //Console.WriteLine(e.Message);
                    Console.WriteLine(content);
                }
                if (match != null && match.Success)
                    {
                        yield return new ReportPage
                        {
                            ReportID = match.Groups["reportID"].Value,
                            ReportTitle = match.Groups["reportTitle"].Value,
                            PageNumber = match.Groups["pageNumber"].Value,
                            ReportSubtitle = match.Groups["reportSubtitle"].Value,
                            ReportContent = match.Groups["reportContent"].Value,
                            PageContent = content,
                        };
                    }
               
            }

            //var matchs = REPORT_PAGE_REGEX.Matches(text);

            //for (int i = 0; i < matchs.Count; i++)
            //{
            //    yield return new ReportPage
            //    {
            //        PageNumber = matchs[i].Groups["pageNumber"].Value,
            //        ReportID = matchs[i].Groups["reportID"].Value,
            //        ReportTitle = matchs[i].Groups["reportTitle"].Value,
            //        ReportSubtitle = matchs[i].Groups["reportSubtitle"].Value,
            //        ReportContent = matchs[i].Groups["reportContent"].Value,
            //        PageContent = matchs[i].Value,
            //    };
            //}
        }

        public static IEnumerable<Page> SplitPages(string text)
        {
            var pages = text.Split('\f');

            for (int i = 0; i < pages.Length; i++)
            {
                yield return new Page
                {
                    Content = pages[i],
                    Number = i + 1,
                };
            }
        }

        public static IEnumerable<string> ExtractReportLines(string text, string reportID)
        {
            var stringReader = new System.IO.StringReader(text);

            return ExtractReportLines(stringReader, reportID);
        }

        /// <summary>
        /// Uses CruiseProcessing's original method to extract report lines from a text reader
        /// This method was used to generate CSV reports from the text output of CruiseProcessing
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="reportID"></param>
        /// <returns></returns>
        public static IEnumerable<string> ExtractReportLines(TextReader reader, string reportID)
        {
            while (true)
            {
                var line = reader.ReadLine();
                if (line == null) { break; }

                //detect new page
                if (line == "\f")
                {
                    line = reader.ReadLine();
                    if (line == null) { break; }
                    //detect start of report
                    if (line.Length >= 3 && line.Substring(0, 3) == reportID)
                    {
                        while (true)
                        {
                            yield return line;

                            //read next line
                            line = reader.ReadLine();
                            if (line == null) { break; }
                            // detect new page
                            if (line == "\f")
                            {
                                line = reader.ReadLine();
                                // detect end of document or start of new report
                                if (line == null
                                    || (line.Length >= 3 && line.Substring(0, 3) != reportID))
                                { yield break; }
                            }
                        }
                    }
                }
            }


        }
    }

    public class Page
    {
        public string Content { get; set; }
        public int Number { get; set; }

        public PageType PageType { get; set; }

        public ReportPage? ReportInfo { get; set; }

    }

    public enum PageType { Header, VolEq, BiomassEq, Report, Warning, Error }

    public class ReportPage
    {
        public string? ReportID { get; set; }
        public string? ReportTitle { get; set; }
        public string? PageNumber { get; set; }
        public string? ReportSubtitle { get; set; }
        public string? ReportContent { get; set; }
        public string? PageContent { get; set; }
    }
}

using HtmlAgilityPack;

namespace DepthCharts;

public abstract class AbstractDepthChartScraper
{

    
    internal static HtmlDocument RemoveNbsp(string html)
    {
        // Load the HTML document
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Iterate over all the text nodes in the document
        foreach (var node in doc.DocumentNode.SelectNodes("//text()") ?? Enumerable.Empty<HtmlNode>())
        {
            // Replace &nbsp; (non-breaking space) with a regular space
            node.InnerHtml = node.InnerHtml.Replace("&nbsp;", " ");
        }

        return doc;
    }
}
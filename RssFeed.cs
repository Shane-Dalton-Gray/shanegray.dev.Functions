using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using shanegray.dev.Functions.Models;
using Supabase;
using System.Net;
using System.Text;
using static Postgrest.Constants;

namespace shanegray.dev.Functions;

public class RssFeed
{
    private readonly ILogger<RssFeed> _logger;

    public RssFeed(ILogger<RssFeed> logger)
    {
        _logger = logger;
    }

    [Function("rss")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "rss")]
        HttpRequestData req)
    {
        var supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL")!;
        var supabaseKey = Environment.GetEnvironmentVariable("SUPABASE_KEY")!;
        var baseUrl = Environment.GetEnvironmentVariable("BASE_URL")
            ?? "https://shanegray.dev";

        var supabase = new Client(supabaseUrl, supabaseKey);
        await supabase.InitializeAsync();

        var result = await supabase
            .From<Post>()
            .Where(p => p.Published == true)
            .Order(p => p.Date, Ordering.Descending)
            .Get();

        var xml = GenerateFeed(result.Models, baseUrl);

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/rss+xml; charset=utf-8");
        await response.WriteStringAsync(xml);
        return response;
    }

    private string GenerateFeed(List<Post> posts, string baseUrl)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<rss version=\"2.0\">");
        sb.AppendLine("<channel>");
        sb.AppendLine("<title>b0dhi</title>");
        sb.AppendLine($"<link>{baseUrl}</link>");
        sb.AppendLine("<description>Thoughts from b0dhi— developer, gamer, perpetual learner.</description>");
        sb.AppendLine("<language>en-us</language>");

        foreach (var post in posts)
        {
            sb.AppendLine("<item>");
            sb.AppendLine($"<title>{post.Title ?? ""}</title>");
            sb.AppendLine($"<link>{baseUrl}/thoughts/{post.Slug ?? ""}</link>");
            sb.AppendLine($"<description>{post.Blurb ?? ""}</description>");
            sb.AppendLine($"<pubDate>{post.Date:R}</pubDate>");
            sb.AppendLine($"<guid>{baseUrl}/thoughts/{post.Slug ?? ""}</guid>");
            sb.AppendLine("</item>");
        }

        sb.AppendLine("</channel>");
        sb.AppendLine("</rss>");

        return sb.ToString();
    }
}
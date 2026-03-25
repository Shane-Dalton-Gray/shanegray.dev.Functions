using Postgrest.Attributes;
using Postgrest.Models;

namespace shanegray.dev.Functions.Models;

[Table("posts")]
public class Post : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("slug")]
    public string Slug { get; set; } = string.Empty;

    [Column("blurb")]
    public string Blurb { get; set; } = string.Empty;

    [Column("body")]
    public string Body { get; set; } = string.Empty;

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("published")]
    public bool Published { get; set; }

    public int ReadingTimeMinutes()
    {
        if (string.IsNullOrWhiteSpace(Body)) return 1;
        var wordCount = Body.Trim().Split(' ',
            StringSplitOptions.RemoveEmptyEntries).Length;
        return Math.Max(1, (int)Math.Ceiling(wordCount / 200.0));
    }
}
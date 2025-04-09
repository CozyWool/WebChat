using System.Globalization;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace WebChatApplication.TagHelpers;

public class DateHumanizerTagHelper : TagHelper
{
    public DateTime? UtcDateTime { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";

        if (UtcDateTime.HasValue)
        {
            var formattedDate = UtcDateTime.Value
                                           .ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);

            output.Attributes.SetAttribute("data-utc", formattedDate);
            output.Attributes.SetAttribute("class", "date-humanizer");
        }
        else
        {
            output.Attributes.SetAttribute("data-utc", "");
        }

        output.Content.SetHtmlContent("");
    }
}
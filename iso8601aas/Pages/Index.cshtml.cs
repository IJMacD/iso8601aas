using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace iso8601aas.Pages;
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {

    }

    public void OnPostParse (string inputString) {
        ViewData["InputString"] = inputString;
        try {
            ViewData["Result"] = ISO8601.Parse(inputString);
        }
        catch {
            ViewData["Error"] = "Unable to parse";
        }
    }
}

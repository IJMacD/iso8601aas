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

    public void OnGet(string input)
    {
        if (input is string) {
            ViewData["InputString"] = input;
            try {
                ViewData["Result"] = ISO8601.Parse(input);
            }
            catch {
                ViewData["Error"] = "Unable to parse";
            }
        }
    }
}

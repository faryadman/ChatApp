using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChatApp.Pages
{
    [Authorize]
    public class SupportAgentModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}

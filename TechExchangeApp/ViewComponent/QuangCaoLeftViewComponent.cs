using Microsoft.AspNetCore.Mvc;
using TechExchangeApp.Data;
using TechExchangeApp.ViewModel;
using TechExchangeApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace TechExchangeApp.ViewComponents
{
    public class QuangCaoLeftViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public QuangCaoLeftViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // tương đương Session("LanguageId")
            var lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;

            var items = await _context.ImageAdvers
                .Where(x =>
                    x.StatusID == 3 &&
                    x.Subject == 9 &&
                    x.LanguageID == lang)
                .OrderBy(x => x.Sort)
                .ToListAsync();

            var vm = new QuangCaoLeftVm
            {
                Items = items
            };

            return View(vm);
        }
    }
}

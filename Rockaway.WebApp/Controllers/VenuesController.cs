using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rockaway.WebApp.Data;

namespace Rockaway.WebApp.Controllers {
	public class VenuesController : Controller {
		private readonly RockawayDbContext _context;

		public VenuesController(RockawayDbContext context) {
			_context = context;
		}

		// GET: Venues
		public async Task<IActionResult> Index() {
			return View(await _context.Venues.ToListAsync());
		}

		// GET: Venues/Details/5
		public async Task<IActionResult> Details(Guid? id) {
			if (id == null) {
				return NotFound();
			}

			var venue = await _context.Venues
				.FirstOrDefaultAsync(m => m.Id == id);
			if (venue == null) {
				return NotFound();
			}

			return View(venue);
		}

	}
}
﻿namespace PeezyMovies.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Caching.Memory;
    using PeezyMovies.Core.Contracts;
    using PeezyMovies.Core.Models;
    using PeezyMovies.Core.Services;
    using PeezyMovies.Infrastructure.Data.Common;
    using PeezyMovies.Infrastructure.Data.Models;
    using PeezyMovies.Models;
    using System.Diagnostics;

    public class HomeController : Controller
    {
        private readonly IRepository repo;
        private readonly IHomeService homeService;
        private readonly IMemoryCache cache;


        public HomeController(IRepository repo, IHomeService homeService, IMemoryCache cache)
        {
            this.repo = repo;
            this.homeService = homeService;
            this.cache = cache;
        }

        [Authorize(Roles = WebAppDataConstants.Admin)]
        public IActionResult AjaxDemo()
        {
            return this.View();
        }

        [Authorize(Roles = WebAppDataConstants.Admin)]
        public IActionResult AjaxData()
        {
            var result = this.repo.All<Actor>().ToList();
            return this.Json(result);
        }
        public async Task<IActionResult> Index()
        {
            const string latestMoviesCache = "LatestMoviesCacheKey";

            var latest = this.cache.Get<IEnumerable<MovieViewModel>>(latestMoviesCache);

            if(latest == null)
            {
                latest = await this.homeService.GetLastThreeAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

                this.cache.Set(latestMoviesCache, latest, cacheOptions);
            }

            return this.View(latest);

        }

       public IActionResult NotFound(int statusCode)
       {
            var viewModel = new HttpErrorViewModel
            {
                StatusCode = statusCode,
            };

            if (statusCode == 404)
            {
                return this.View(viewModel);
            }

            return this.View(
                "Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
namespace PeezyMovies.Core.Services
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using PeezyMovies.Core.Contracts;
    using PeezyMovies.Core.Exceptions;
    using PeezyMovies.Core.Models;
    using PeezyMovies.Infrastructure.Data.Common;
    using PeezyMovies.Infrastructure.Data.Models;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;


    public class MovieService : IMovieService
    {
        private readonly IRepository repo;
        private readonly ILogger logger;
        private readonly IGuard guard;

        public MovieService(IRepository _repo, ILogger<MovieService> logger, IGuard guard)
        {
            this.repo = _repo;
            this.logger = logger;
            this.guard = guard;
        }

        public async Task AddMovieAsync(AddMovieViewModel model)
        {
            var movie = new Movie()
            {
                Title = model.Title,
                Description = model.Description,
                Rating = model.Rating,
                Price = model.Price,
                Director = model.Director,
                ImageUrl = model.ImageUrl,
                CinemaId = model.CinemaId,
                GenreId = model.GenreId,
                ProducerId = model.ProducerId,
                Trailer = model.MovieTrailer,
                IsDeleted = false,

            };

            try
            {
                await repo.AddAsync(movie);
                await repo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(nameof(AddMovieAsync), ex);
                throw new ApplicationException("Database failed to save", ex);
            }

            foreach (var actorId in model.ActorIds)
            {
                var newActorMovie = new ActorMovie()
                {
                    MovieId = movie.Id,
                    ActorId = actorId,
                };

                await repo.AddAsync(newActorMovie);
            }

            await repo.SaveChangesAsync();
        }

        public async Task<IEnumerable<MovieViewModel>> GetAllAsync()
        {
            var entities = await repo.All<Movie>()
                .Where(x => x.IsDeleted == false)
                .Include(x => x.Genre)
                .Include(x => x.Producer)
                .Include(x => x.Cinema)
                .ToListAsync();

            return entities.Select(x => new MovieViewModel
            {
                Cinema = x.Cinema.Name,
                Genre = x.Genre.Name,
                Producer = x.Producer.FullName,
                Director = x.Director,
                Id = x.Id,
                Description = x.Description,
                Price = x.Price,
                Rating = x.Rating,
                ImageUrl = x.ImageUrl,
                Title = x.Title,
                MovieTrailer = x.Trailer,
            });
        }


        public async Task<IEnumerable<Cinema>> GetCinemasAsync() => await repo.All<Cinema>().ToListAsync();

        public async Task<IEnumerable<Genre>> GetGenresAsync() => await repo.All<Genre>().ToListAsync();

        public async Task<IEnumerable<Producer>> GetProducersAsync() => await repo.All<Producer>().ToListAsync();

        public async Task AddMovieToCollectionAsync(string userId, int movieId)
        {
            guard.AgainstNull(userId, "User cannot be found");


            var user = await repo.All<User>()
                .Where(x => x.Id == userId)
                .Include(x => x.UsersMovies)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return;
            }

            var movie = await repo.All<Movie>()
                .Where(x => x.Id == movieId && x.IsDeleted == false)
                .FirstOrDefaultAsync();
            if (!user.UsersMovies.Any(x => x.MovieId == movie?.Id))
            {
                var userMovie = new UserMovie()
                {
                    MovieId = movie.Id,
                    UserId = user.Id,
                    User = user,
                    Movie = movie,
                };
                user.UsersMovies.Add(userMovie);
                await repo.SaveChangesAsync();
            }

        }

        public async Task RemoveFromCollectionAsync(string userId, int movieId)
        {
            var user = await repo.All<User>().Where(x => x.Id == userId)
               .Include(x => x.UsersMovies)
               .FirstOrDefaultAsync();

            var movie = user?.UsersMovies.FirstOrDefault(x => x.MovieId == movieId);

            if (movie != null)
            {
                user.UsersMovies.Remove(movie);

                await repo.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<MovieViewModel>> GetWatchedAsync(string userId)
        {
            var user = await repo.All<User>().Where(x => x.Id == userId)
                .Include(x => x.UsersMovies)
                .ThenInclude(x => x.Movie)
               .ThenInclude(c => c.Cinema)
               .Include(x => x.UsersMovies)
               .ThenInclude(x => x.Movie)
               .ThenInclude(x => x.Genre)
               .Include(x => x.UsersMovies)
               .ThenInclude(x => x.Movie)
               .ThenInclude(x => x.Producer)
               .FirstOrDefaultAsync();


            return user.UsersMovies
               .Select(m => new MovieViewModel()
               {
                   Id = m.MovieId,
                   Director = m.Movie.Director,
                   Description = m.Movie.Description,
                   Price = m.Movie.Price,
                   ImageUrl = m.Movie.ImageUrl,
                   Title = m.Movie.Title,
                   Rating = m.Movie.Rating,
                   Producer = m.Movie.Producer.FullName,
                   Cinema = m.Movie.Cinema?.Name,
                   Genre = m.Movie.Genre?.Name,
                   MovieTrailer = m.Movie.Trailer,
               });
        }

        public async Task EditMovie(int movieId, EditMovieViewModel model)
        {
            var testMovie = await this.repo.All<Movie>().FirstOrDefaultAsync(x => x.Id == movieId);

            guard.AgainstNull(testMovie, "Movie cannot be found");

            testMovie.ImageUrl = model.ImageUrl;
            testMovie.Director = model.Director;
            testMovie.Rating = model.Rating;
            testMovie.CinemaId = model.CinemaId;
            testMovie.GenreId = model.GenreId;
            testMovie.ProducerId = model.ProducerId;
            testMovie.Title = model.Title;
            testMovie.Trailer = model.MovieTrailer;

            await repo.SaveChangesAsync();
        }

        public async Task<Movie> GetMovieByIdAsync(int movieId)
        {
            var movieDetails = await repo.All<Movie>()
                .Where(x => x.IsDeleted == false)
                .Include(c => c.Genre)
                .Include(c => c.Cinema)
               .Include(p => p.Producer)
               .Include(am => am.ActorsMovies).ThenInclude(a => a.Actor)
               .FirstOrDefaultAsync(n => n.Id == movieId);


            guard.AgainstNull(movieId, "Movie cannot be found");

            return movieDetails;
        }

        public async Task<ActorsViewModel> GetActorsDropDown() =>
            new ActorsViewModel() { Actors = await repo.All<Actor>().OrderBy(x => x.Id).ToListAsync() };


        public AllMoviesViewModel All(AllMoviesViewModel model)
        {
            var query = repo.All<Movie>().Where(x => x.IsDeleted == false).AsQueryable();

            if (!string.IsNullOrWhiteSpace(model.Genre))
            {
                query = query.Where(x => x.Genre.Name == model.Genre);
            }

            query = model.Sorting switch
            {
                MovieSorting.Price => query.OrderByDescending(c => c.Price),
                MovieSorting.Genre => query.OrderBy(c => c.Genre.Name),
                MovieSorting.Rating or _ => query.OrderByDescending(c => c.Rating)
            };

            var movies = query
                .Skip((model.CurrentPage - 1) * model.MoviesPerPage)
                .Take(model.MoviesPerPage)
                .Include(x => x.Genre)
                .Include(x => x.Producer)
                .Include(x => x.Cinema)
                .Select(x => new MovieViewModel
                {
                    Cinema = x.Cinema.Name,
                    Genre = x.Genre.Name,
                    Producer = x.Producer.FullName,
                    Director = x.Director,
                    Id = x.Id,
                    Description = x.Description,
                    Price = x.Price,
                    Rating = x.Rating,
                    ImageUrl = x.ImageUrl,
                    Title = x.Title,
                    MovieTrailer = x.Trailer,

                })
                .ToList();


            return new AllMoviesViewModel()
            {
                Movies = movies,
                CurrentPage = model.CurrentPage,
                MoviesPerPage = model.MoviesPerPage,
                Sorting = model.Sorting,
                Genre = model.Genre,
                TotalCount = query.Count()
            };
        }

        public IEnumerable<string> GenresNamesAsStrings() => repo.All<Genre>()
               .Select(x => x.Name)
               .Distinct()
               .ToList();


        public async Task<bool> Exists(int id) =>
            await repo.AllReadonly<Movie>().Where(x => x.IsDeleted == false).AnyAsync(x => x.Id == id);

        public MovieViewModel MovieForView(int id) => this.repo.All<Movie>()
                .Where(x => x.Id == id && x.IsDeleted == false)
                .Select(x => new MovieViewModel
                {
                    Id = x.Id,
                    ImageUrl = x.ImageUrl,
                    Director = x.Director,
                    Rating = x.Rating,
                    Title = x.Title,
                    MovieTrailer = x.Trailer,
                })
                .FirstOrDefault();

        public async Task<bool> DeleteMovie(int id)
        {
            var movie = await this.repo.All<Movie>().Where(x => x.IsDeleted == false)
                .FirstOrDefaultAsync(x => x.Id == id);


            guard.AgainstNull(movie, "Movie cannot be found");

            movie.IsDeleted = true;

            await repo.SaveChangesAsync();

            return true;
        }
    }
}

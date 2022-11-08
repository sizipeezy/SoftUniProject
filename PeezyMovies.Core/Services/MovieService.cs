﻿namespace PeezyMovies.Core.Services
{
    using Microsoft.EntityFrameworkCore;
    using PeezyMovies.Core.Contracts;
    using PeezyMovies.Core.Models;
    using PeezyMovies.Infrastructure.Data.Common;
    using PeezyMovies.Infrastructure.Data.Models;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;


    public class MovieService : IMovieService
    {
        private readonly IRepository repo;

        public MovieService(IRepository _repo)
        {
            this.repo = _repo;
        }

        public async Task AddMovieAsync(AddMovieViewModel model)
        {
            var movie = new Movie
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

            };

            await repo.AddAsync(movie);
            await repo.SaveChangesAsync();

            foreach (var actor in repo.All<Actor>())
            {
                var newActorMovie = new ActorMovie()
                {
                    MovieId = movie.Id,
                    ActorId = actor.Id,
                };

                await repo.AddAsync(newActorMovie);
            }

            await repo.SaveChangesAsync();
        }

        public async Task<IEnumerable<MovieViewModel>> GetAllAsync()
        {
            var entities = await repo.All<Movie>().Include(x => x.Genre)
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
            });
        }

        public EditMovieViewModel GetById(int movieId)
        {
            return repo.All<Movie>().Where(x => x.Id == movieId)
              .Select(x => new EditMovieViewModel
              {
                  Id = x.Id,
                  ImageUrl = x.ImageUrl,
                  Director = x.Director,
                  Rating = x.Rating,
                  CinemaId = x.CinemaId,
                  GenreId = x.GenreId,
                  ProducerId = x.ProducerId,
                  Title = x.Title,
                  Description = x.Description,
                  Price = x.Price,
                  

              }).FirstOrDefault();
        }

        public async Task<Movie> GetMovieAsync(int movieId)
        {
           return await repo.All<Movie>().Where(x => x.Id == movieId).FirstOrDefaultAsync();
            
        }

        public async Task<IEnumerable<Cinema>> GetCinemasAsync()
        {
            return await repo.All<Cinema>().ToListAsync();
        }

        public async Task<IEnumerable<Genre>> GetGenresAsync()
        {
            return await repo.All<Genre>().ToListAsync();
        }
        public async Task<IEnumerable<Producer>> GetProducersAsync()
        {
            return await repo.All<Producer>().ToListAsync();
        }

        public async Task<IEnumerable<Actor>> GetActors()
        {
            return await repo.All<Actor>().ToListAsync();
        }

        public async Task<IEnumerable<MovieViewModel>> GetLastThreeAsync()
        {
            return await repo.All<Movie>().OrderByDescending(x => x.Id)
            .Take(3)
            .Select(x => new MovieViewModel
            {
                Id = x.Id,
                Director = x.Director,
                ImageUrl = x.ImageUrl,
                Genre = x.Genre.Name,
                Title = x.Title,
                Rating = x.Rating,
                Price = x.Price,
                Cinema = x.Cinema.Name,
                Producer = x.Producer.FullName,
                Description = x.Description,

            }).ToListAsync();
        }

        public async Task AddMovieToCollectionAsync(string userId, int movieId)
        {
            var user = await repo.All<User>()
                .Where(x => x.Id == userId)
                .Include(x => x.UsersMovies)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return;
            }

            var movie = await repo.All<Movie>()
                .Where(x => x.Id == movieId)
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
               });
        }

        public Task EditMovieAsync(AddMovieViewModel model, int movieId)
        {
            throw new NotImplementedException();
        }
    }
}
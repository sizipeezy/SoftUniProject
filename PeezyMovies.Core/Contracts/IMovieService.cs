﻿namespace PeezyMovies.Core.Contracts
{
    using PeezyMovies.Core.Models;
    using PeezyMovies.Infrastructure.Data.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IMovieService
    {
        Task<IEnumerable<MovieViewModel>> GetAllAsync();

         Task AddMovieAsync(AddMovieViewModel model);

        EditMovieViewModel GetById(int movieId);

        Task<Movie> GetMovieAsync(int movieId);

        Task EditMovieAsync(AddMovieViewModel model, int movieId);
        Task<IEnumerable<MovieViewModel>> GetLastThreeAsync();

        Task AddMovieToCollectionAsync(string userId, int movieId);

        Task RemoveFromCollectionAsync(string userId, int movieId);

        Task<IEnumerable<MovieViewModel>> GetWatchedAsync(string userId);

        Task<IEnumerable<Genre>> GetGenresAsync();

        Task<IEnumerable<Producer>> GetProducersAsync();

        Task<IEnumerable<Cinema>> GetCinemasAsync();

        Task<IEnumerable<Actor>> GetActors();
    }
}
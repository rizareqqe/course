using API_KURS.Contracts.Movies;
using MovieLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace API_KURS.Services
{
    public static class MovieSortingService
    {
        public static IEnumerable<Movie> ApplySort(IEnumerable<Movie> source, MovieFilterQuery filter)
        {
            IOrderedEnumerable<Movie>? ordered = ApplyOrderedSort(source, filter.PrimarySort, filter.PrimaryDescending, true);
            if (ordered == null)
            {
                ordered = source.OrderBy(movie => movie.title);
            }

            var secondary = ApplyOrderedSort(ordered, filter.SecondarySort, filter.SecondaryDescending, false);
            return secondary ?? ordered;
        }

        private static IOrderedEnumerable<Movie>? ApplyOrderedSort(IEnumerable<Movie> source, string? sortField, bool descending, bool firstSort)
        {
            if (string.IsNullOrWhiteSpace(sortField))
            {
                return null;
            }

            return sortField.ToLower() switch
            {
                "title" => Order(source, movie => movie.title, descending, firstSort),
                "year" => Order(source, movie => movie.year, descending, firstSort),
                "director" => Order(source, movie => movie.Director?.Name ?? string.Empty, descending, firstSort),
                "curator" => Order(source, movie => movie.CuratorUser?.FullName ?? string.Empty, descending, firstSort),
                _ => null
            };
        }

        private static IOrderedEnumerable<Movie> Order<TKey>(IEnumerable<Movie> source, Func<Movie, TKey> keySelector, bool descending, bool firstSort)
        {
            if (firstSort)
            {
                return descending ? source.OrderByDescending(keySelector) : source.OrderBy(keySelector);
            }

            var ordered = (IOrderedEnumerable<Movie>)source;
            return descending ? ordered.ThenByDescending(keySelector) : ordered.ThenBy(keySelector);
        }
    }
}

using API_KURS.Contracts.Movies;
using API_KURS.Services;
using MovieLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace API_KURS.UnitTests
{
    public static class MovieSortingServiceTests
    {
        public static int Main()
        {
            var tests = new Action[]
            {
                ApplySort_UsesTitleAscending_WhenPrimarySortIsInvalid,
                ApplySort_AppliesPrimaryAndSecondarySort,
                ApplySort_CanSortByDirectorAndCuratorNames
            };

            foreach (var test in tests)
            {
                test();
                Console.WriteLine($"Passed: {test.Method.Name}");
            }

            Console.WriteLine($"All tests passed: {tests.Length}");
            return 0;
        }

        private static void ApplySort_UsesTitleAscending_WhenPrimarySortIsInvalid()
        {
            var movies = new List<Movie>
            {
                new Movie { title = "Брат", year = 1997 },
                new Movie { title = "Аватар", year = 2009 },
                new Movie { title = "Матрица", year = 1999 }
            };

            var query = new MovieFilterQuery { PrimarySort = "unknown" };

            var result = MovieSortingService.ApplySort(movies, query).Select(movie => movie.title).ToList();

            AssertSequenceEqual(new[] { "Аватар", "Брат", "Матрица" }, result);
        }

        private static void ApplySort_AppliesPrimaryAndSecondarySort()
        {
            var movies = new List<Movie>
            {
                new Movie { title = "Фильм A", year = 2000 },
                new Movie { title = "Фильм C", year = 2000 },
                new Movie { title = "Фильм B", year = 1999 }
            };

            var query = new MovieFilterQuery
            {
                PrimarySort = "year",
                PrimaryDescending = false,
                SecondarySort = "title",
                SecondaryDescending = true
            };

            var result = MovieSortingService.ApplySort(movies, query).Select(movie => movie.title).ToList();

            AssertSequenceEqual(new[] { "Фильм B", "Фильм C", "Фильм A" }, result);
        }

        private static void ApplySort_CanSortByDirectorAndCuratorNames()
        {
            var movies = new List<Movie>
            {
                new Movie
                {
                    title = "Фильм 1",
                    Director = new Director { Name = "Нолан" },
                    CuratorUser = new UserAccount { FullName = "Иванов Иван" }
                },
                new Movie
                {
                    title = "Фильм 2",
                    Director = new Director { Name = "Кэмерон" },
                    CuratorUser = new UserAccount { FullName = "Петров Петр" }
                },
                new Movie
                {
                    title = "Фильм 3",
                    Director = new Director { Name = "Кэмерон" },
                    CuratorUser = new UserAccount { FullName = "Алексеева Анна" }
                }
            };

            var query = new MovieFilterQuery
            {
                PrimarySort = "director",
                SecondarySort = "curator"
            };

            var result = MovieSortingService.ApplySort(movies, query).Select(movie => movie.title).ToList();

            AssertSequenceEqual(new[] { "Фильм 3", "Фильм 2", "Фильм 1" }, result);
        }

        private static void AssertSequenceEqual(IReadOnlyList<string> expected, IReadOnlyList<string> actual)
        {
            if (expected.Count != actual.Count)
            {
                throw new InvalidOperationException($"Expected {expected.Count} items, but got {actual.Count} items.");
            }

            for (var index = 0; index < expected.Count; index++)
            {
                if (expected[index] != actual[index])
                {
                    throw new InvalidOperationException(
                        $"Item {index} is incorrect. Expected '{expected[index]}', but got '{actual[index]}'.");
                }
            }
        }
    }
}

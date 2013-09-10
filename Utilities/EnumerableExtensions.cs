﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace WallpaperGenerator.Utilities
{
    public static class EnumerableExtensions
    {        
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (T i in items)
                action(i);
        }

        public static IEnumerable<T> Repeat<T>(Func<T> func, int? count = null)
        {
            return Repeat(i => func(), count);
        }

        public static IEnumerable<T> Repeat<T>(Func<int, T> func, int? count = null)
        {
            return Repeat(Enumerable.Repeat(func, 1), count);
        }

        public static IEnumerable<T> Repeat<T>(this IEnumerable<T> items, int? count = null)
        {
            return Repeat(items.Select(item => (Func<int, T>)(i => item)), count);
        }

        public static IEnumerable<T> Repeat<T>(this IEnumerable<Func<int, T>> items, int? count = null)
        {
            if (!items.Any())
                throw new ArgumentException("Sequence is empty.", "items");

            int i = 0;
            while (true)
            {
                if (count != null && i >= count)
                    break;

                foreach (Func<int, T> item in items)
                    yield return item(i);

                i++;
            }
        }

        public static IEnumerable<R> SelectWithFolding<T, R>(this IEnumerable<T> source, Func<R, T, R> func, R initValue = default(R))
        {
            R previousValue = initValue; 
            foreach (T e in source)
            {
                R value = func(previousValue, e);
                yield return value;
                previousValue = value;
            }
        }

        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> source, Random random)
        {
            return source.OrderBy(item => random.Next());
        }

        public static T TakeRandom<T>(this IEnumerable<T> source, Random random)
        {
            int index = random.Next(source.Count()); 
            return source.Skip(index).First();
        }

        public static T TakeRandom<T>(this IEnumerable<T> source, Random random, IEnumerable<double> elementProbabilities)
        {
            if (source.Count() != elementProbabilities.Count())
            { 
                throw new ArgumentException("Count of elements isn't equal with count of probabilties.");
            }

            double probabiltiesSum = elementProbabilities.Sum(p => p);
            const double doubleError = 1E-9;
            if (Math.Abs(probabiltiesSum - 1) > doubleError)
            {
                throw new ArgumentException("Sum of probabilties isn't equal to 1.");
            }

            double[] probabilisticRange = elementProbabilities.SelectWithFolding((p, c) => p + c, 0.0).ToArray();
            probabilisticRange[probabilisticRange.Length - 1] = 1;

            double r = random.NextDouble();
            int i = probabilisticRange.TakeWhile(p => r >= p).Count();
            return source.Skip(i).First();
        }
    }
}

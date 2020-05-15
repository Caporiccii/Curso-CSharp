﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks.Dataflow;

namespace Curso.TestPipeline
{
    class MembersPipe
    {
        // Demonstrates how to create a basic dataflow pipeline.
        // This program downloads the book "The Iliad of Homer" by Homer from the Web
        // and finds all reversed words that appear in that book.
        //
        // Create the members of the pipeline.
        //

        // Downloads the requested resource as a string.
        public void DownloadString() { 
        var downloadString = new TransformBlock<string, string>(async uri =>
        {
            Console.WriteLine("Downloading '{0}'...", uri);

            return await new HttpClient(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip }).GetStringAsync(uri);
        });    
        }

        // Separates the specified text into an array of words.
        public TransformBlock<string, string[]> CreatWordList()
        {
            var createWordList = new TransformBlock<string, string[]>(text =>
            {
                Console.WriteLine("Creating word list...");

            // Remove common punctuation by replacing all non-letter characters
            // with a space character.
            char[] tokens = text.Select(c => char.IsLetter(c) ? c : ' ').ToArray();
                text = new string(tokens);

            // Separate the text into an array of words.
            var textSeparated = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return textSeparated;
            });

            return createWordList; 
        }

        public void FilterWordList()
        {
            // Removes short words and duplicates.
            var filterWordList = new TransformBlock<string[], string[]>(words =>
            {
                Console.WriteLine("Filtering word list...");

                return words
                   .Where(word => word.Length > 3)
                   .Distinct()
                   .ToArray();
            });
        }

        // Finds all words in the specified collection whose reverse also
        // exists in the collection.
        public void FindReverseWord()
        {
            var findReversedWords = new TransformManyBlock<string[], string>(words =>
            {
                Console.WriteLine("Finding reversed words...");

                var wordsSet = new HashSet<string>(words);

                return from word in words.AsParallel()
                       let reverse = new string(word.Reverse().ToArray())
                       where word != reverse && wordsSet.Contains(reverse)
                       select word;
            });
        }

        // Prints the provided reversed words to the console.
        public void Print()
        {
            var printReversedWords = new ActionBlock<string>(reversedWord =>
            {
                Console.WriteLine("Found reversed words {0}/{1}",
                   reversedWord, new string(reversedWord.Reverse().ToArray()));
            });
        }
    }
}

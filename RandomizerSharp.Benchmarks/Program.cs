using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using RandomizerSharp.NDS;

namespace RandomizerSharp.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<SearchMark>();
            Console.ReadLine();
        }
    }

    public class SearchMark
    {
        private const int NumberOfTests = 200000;

        private readonly int[][] _buffers = new int[NumberOfTests][];
        private readonly int[] _starts = new int[NumberOfTests];
        private readonly int[] _ends = new int[NumberOfTests];
        private readonly int[] _bestPos = new int[NumberOfTests];

        public SearchMark()
        {
            var random = new Random();

            for (var j = 0; j < NumberOfTests; j++)
            {
                var buffer = new int[random.Next(255, 500)];
                var start = random.Next(buffer.Length);
                var end = random.Next(start + 1, buffer.Length);
                var bestPos = random.Next(buffer.Length);

                for (var i = 0; i < buffer.Length; i++) buffer[i] = random.Next(byte.MaxValue);

                _buffers[j] = buffer;
                _starts[j] = start;
                _ends[j] = end;
                _bestPos[j] = bestPos;
            }
        }
    }
}

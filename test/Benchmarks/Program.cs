using System;
using System.Diagnostics;
using BenchmarkDotNet.Running;

namespace WyHash.Benchmarks;

internal static class Program
{
    private static void Main(string[] args)
    {
        // Main benchmarks
        BenchmarkRunner.Run<Test>();

        // Micro-optimisation benchmarks
        //BenchmarkRunner.Run<Multiply64Tests>();
        //BenchmarkRunner.Run<Read64SwappedTests>();
        //BenchmarkRunner.Run<Read64Tests>();
        //BenchmarkRunner.Run<Read32Tests>();
        //BenchmarkRunner.Run<Read8Tests>();


        /* Used for quick-and dirty tests
        var test = new Test { DataSize = Test.KB };
        test.Setup();
        test.TestWyHash();

        var sw = Stopwatch.StartNew();

        for (var i = 0; i < 10000000; i++)
        {
            test.TestWyHash();
        }

        sw.Stop();
        Console.WriteLine(sw.Elapsed);
        Console.ReadLine();*/
    }
}

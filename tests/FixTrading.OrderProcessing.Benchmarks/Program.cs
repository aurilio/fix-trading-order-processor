using BenchmarkDotNet.Running;
using FixTrading.OrderProcessing.Benchmarks;

// Rodar todos os benchmarks
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

// Rodar um específico:
// BenchmarkRunner.Run<OrderCreationBenchmarks>();

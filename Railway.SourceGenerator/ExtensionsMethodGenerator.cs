using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Just.Railway.SourceGen;

[Generator]
public class ExtensionsMethodGenerator : IIncrementalGenerator
{
    private readonly IEnumerable<IGeneratorExecutor> _executors = new IGeneratorExecutor[]
    {
        new ResultCombineExecutor(),
        new ResultMatchExecutor(),
        new ResultMapExecutor(),
        new ResultBindExecutor(),
        new ResultTapExecutor(),
        new ResultTryRecoverExecutor(),
        new ResultAppendExecutor(),
        new TryExtensionsExecutor(),
        new EnsureExtensionsExecutor(),
    };

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        foreach (var executor in _executors)
        {
            context.RegisterSourceOutput(context.CompilationProvider, executor.Execute);
        }
    }
}

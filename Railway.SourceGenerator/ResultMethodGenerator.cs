using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Just.Railway.SourceGen;

[Generator]
public class ResultMethodGenerator : IIncrementalGenerator
{
    private readonly IEnumerable<IGeneratorExecutor> _executors = new IGeneratorExecutor[]
    {
        new ResultCombineExecutor(),
        new ResultMatchExecutor(),
        new ResultMapExecutor(),
        new ResultBindExecutor(),
        new ResultTapExecutor(),
        new ResultAppendExecutor(),
    };

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        foreach (var executor in _executors)
        {
            context.RegisterSourceOutput(context.CompilationProvider, executor.Execute);
        }
    }
}

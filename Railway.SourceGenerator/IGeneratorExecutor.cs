using Microsoft.CodeAnalysis;

namespace Just.Railway.SourceGen;

internal interface IGeneratorExecutor
{
    public abstract void Execute(SourceProductionContext context, Compilation source);
}

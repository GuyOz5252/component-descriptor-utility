using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ComponentDescriptorUtility.Cli;

public class ImplementationOptionsResolver
{
    private readonly IList<Compilation> _compilations;
    
    public ImplementationOptionsResolver(List<Compilation> compilations)
    {
        _compilations = compilations;
    }

    public async Task<List<ImplementationOptionDescriptor>> ResolveImplementationOptions(ITypeSymbol dependency)
    {
        if (dependency is { IsAbstract: false })
        {
            throw new ArgumentException($"Cannot resolve implementation options for non abstract type: {dependency.Name}");
        }
        
        var options = new List<ImplementationOptionDescriptor>();

        foreach (var compilation in _compilations)
        {
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var root = await syntaxTree.GetRootAsync();
                var model = compilation.GetSemanticModel(syntaxTree);
                var syntaxNodes = root.DescendantNodes()
                    .Where(syntaxNode => syntaxNode is ClassDeclarationSyntax or RecordDeclarationSyntax)
                    .ToList();

                foreach (var syntaxNode in syntaxNodes)
                {
                    var symbol = model.GetDeclaredSymbol(syntaxNode);
                    if (symbol is null || symbol.IsAbstract)
                    {
                        continue;
                    }

                    if (symbol is INamedTypeSymbol namedSymbol)
                    {
                        // TODO: Check if symbol is assignable from dependency type
                    }
                }
            }
        }

        return options;
    }
}

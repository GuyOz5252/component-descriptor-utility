using Microsoft.CodeAnalysis;

namespace ComponentDescriptorUtility.Cli;

public class DependencyResolver
{
    private readonly ImplementationOptionsResolver _implementationOptionsResolver;
    
    public DependencyResolver(ImplementationOptionsResolver implementationOptionsResolver)
    {
        _implementationOptionsResolver = implementationOptionsResolver;
    }

    public async Task<List<DependencyDescriptor>> ResolveDependenciesAsync(ITypeSymbol symbol)
    {
        var dependencies = symbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(property => property is
            {
                DeclaredAccessibility: Accessibility.Public,
                IsStatic: false,
                SetMethod: not null
            })
            .ToList();

        if (dependencies.Count is 0)
        {
            return [];
        }

        return dependencies.Select(async dependency =>
            {
                if (dependency.Type is { IsAbstract: true })
                {
                    var implementationOptions = await _implementationOptionsResolver
                        .ResolveImplementationOptions(dependency.Type);
                    return new DependencyDescriptor
                    {
                        DisplayName = dependency.Name,
                        TypeName = dependency.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        Dependencies = [],
                        ImplementationOptions = implementationOptions
                    };
                }
                var resolvedInnerDependencies = await ResolveDependenciesAsync(dependency.Type);
                
                return new DependencyDescriptor
                {
                    DisplayName = dependency.Name,
                    TypeName = dependency.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    Dependencies = resolvedInnerDependencies
                };
            })
            .Select(t => t.Result)
            .ToList();
    }
}
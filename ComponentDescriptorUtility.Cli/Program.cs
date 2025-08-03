using ComponentDescriptorUtility.Cli;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

const string IComponentFactory = "IComponentFactory";

MSBuildLocator.RegisterDefaults();

var solutionPath = "C://Code/ConfigurationProvider.ConsoleUi/ConfigurationProvider.ConsoleUi.sln";

using var workspace = MSBuildWorkspace.Create();
var solution = await workspace.OpenSolutionAsync(solutionPath);

var compilations = new List<Compilation>();

foreach (var project in solution.Projects)
{
    var componentFactories = new List<INamedTypeSymbol>();
    
    var compilation = await project.GetCompilationAsync();
    if (compilation is not null)
    {
        compilations.Add(compilation);
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var model = compilation.GetSemanticModel(syntaxTree);
            var root = await syntaxTree.GetRootAsync();
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var @class in classes)
            {
                var symbol = model.GetDeclaredSymbol(@class);
                if (symbol is null || symbol.IsAbstract)
                {
                    continue;
                }

                if (symbol.AllInterfaces.Any(@interface =>
                        @interface.Name is IComponentFactory
                        || @interface.ToDisplayString() is IComponentFactory))
                {
                    componentFactories.Add(symbol);
                }
            }
        }
    }

    var implementationOptionsResolver = new ImplementationOptionsResolver(compilations);
    var dependencyResolver = new DependencyResolver(implementationOptionsResolver);

    foreach (var componentFactory in componentFactories)
    {
        Console.WriteLine($"IComponentFactory: {componentFactory.Name}");
        var dependencies = await dependencyResolver.ResolveDependenciesAsync(componentFactory);
        dependencies?.ForEach(Console.WriteLine);
    }
}
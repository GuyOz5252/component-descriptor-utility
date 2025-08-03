namespace ComponentDescriptorUtility.Cli;

public class DependencyDescriptor
{
    public required string DisplayName { get; init; }
    public required string TypeName { get; init; }
    public required List<DependencyDescriptor> Dependencies { get; init; }
    public List<ImplementationOptionDescriptor>? ImplementationOptions { get; init; }

    public override string ToString() =>
        $"""
          
          {nameof(DisplayName)}: {DisplayName},
          {nameof(TypeName)}: {TypeName},
          {nameof(Dependencies)}: {Dependencies.Count},
          {nameof(ImplementationOptions)}: {ImplementationOptions?.Count}
         """;
}
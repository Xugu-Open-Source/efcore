namespace Microsoft.AspNetCore.Hosting.WebHostBuilderFactory
{
    internal enum FactoryResolutionResultKind
    {
        Success,
        NoEntryPoint,
        NoCreateWebHostBuilder,
        NoBuildWebHost
    }
}

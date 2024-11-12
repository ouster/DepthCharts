using Xunit;

namespace DepthCharts.tests;

public class WorkingDirectoryFixture : IDisposable
{
    public WorkingDirectoryFixture()
    {
        var projectDirectory = Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.FullName;
        Directory.SetCurrentDirectory($"{projectDirectory}/tests");
    }

    public void Dispose() { }
}

[CollectionDefinition("Directory collection")]
public class DirectoryCollection : ICollectionFixture<WorkingDirectoryFixture> { }

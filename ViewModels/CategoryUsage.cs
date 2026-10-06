namespace Website_Idea.ViewModels;

public record CategoryUsage(int Id, string Name, int ProductCount, int PhotoCount)
{
    public bool InUseByProducts => ProductCount > 0;
}

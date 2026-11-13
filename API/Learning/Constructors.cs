public class Constructors
{
    static string _filePath;
    static Constructors()
    {
        _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "medicines.json");
    }
    private Constructors()
    {
        _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "medicines.json");
    }

    public static void AddConstructors()
    {
        
    }
}
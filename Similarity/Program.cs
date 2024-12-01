using Similarity.utils;

namespace Similarity;

class Program
{
    private const string DEFAULT_OUT_DIR = "refactor";
    static void Main(string[] args)
    {
        
        CheckArgs(args);
        
        string dirPath = args[0];
        string outputPath = args.Length > 1 ? args[1] : DEFAULT_OUT_DIR;

        CheckPath(dirPath);
        
        FilesManager.InitOutputDirectory(outputPath);
        
        var files = FilesManager.GetAllCsFilesFromDirectory(dirPath);

        var analyzer = new Analyzer(files);
       
        analyzer.Analyze();
        var opportunities = analyzer.GetRefactorOpportunities();
        
        var refactor = new Refactor(opportunities, outputPath);
        refactor.GenerateRefactor();
       
        var result = refactor.GetRefactorsCount();
        
        Report.GenerateReport(result, outputPath);
    }
    
    private static void CheckArgs(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Caminho do diretório não informado");
            Environment.Exit(1);
        }
        
    }
    
    private static void CheckPath(string path)
    {
        if (!FilesManager.IsValidPath(path))
        {
            Console.WriteLine($"Caminho inválido");
            Environment.Exit(1);
        }
    }
}

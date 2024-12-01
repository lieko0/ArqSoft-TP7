using Microsoft.CodeAnalysis.CSharp.Syntax;
using Similarity.dto;

namespace Similarity;

public static class Report
{
    public static void GenerateReport(int count, string outputPath)
    {
        if (count > 0)
        {
            Console.WriteLine($"Foram encontradas {count} oportunidades de refatoração");
            Console.WriteLine($"Os novos arquivos foram gerados na pasta {outputPath}");
        }
        else
        {
            Console.WriteLine("Não foram encontradas opotunidades de refatoração");
        }
    }
}
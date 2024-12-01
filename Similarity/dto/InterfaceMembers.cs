using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Similarity.dto;

public class InterfaceMembers(List<FileClassDeclarations> fileClassDeclarationsList, List<MethodDeclarationSyntax> methodDeclarationSyntaxList)
{
    public List<FileClassDeclarations> fileClassDeclarationsList { get; set; } = fileClassDeclarationsList;
    public List<MethodDeclarationSyntax> methodDeclarationSyntaxList { get; set; } = methodDeclarationSyntaxList;
    
}
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Similarity.dto;
public class Relationship(FileClassDeclarations fileClassDeclarations, MethodDeclarationSyntax method)
{
    public FileClassDeclarations classDeclaration { get; } = fileClassDeclarations;
    public MethodDeclarationSyntax methodDeclaration { get; } = method;
}
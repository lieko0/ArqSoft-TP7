using Similarity.dto;
using Similarity.utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Similarity;

public class Refactor(List<InterfaceMembers> interfaceMembersList, string outputPath)
{
    private const string INTERFACE_NAMESPACE = "Similarity.refactor.interfaces";
    private const string CLASSES_NAMESPACE = "Similarity.refactor.classes";
    private const string INTERFACE_NAME = "GeneratedInterface";

    private readonly string _classPath = $"{outputPath}/classes";
    private readonly string _interfacePath = $"{outputPath}/interfaces";

    private int _generatedInterfaceNum = 0;

    public void GenerateRefactor()
    {
        FilesManager.InitOutputDirectory(_classPath);
        FilesManager.InitOutputDirectory(_interfacePath);

        foreach (var interfaceMembers in interfaceMembersList)
        {
            GenerateInterface(interfaceMembers);
            GenerateClasses(interfaceMembers);

            _generatedInterfaceNum++;
        }
    }

    public int GetRefactorsCount()
    {
        return interfaceMembersList.Count;
    }
    

    private void GenerateInterface(InterfaceMembers interfaceMembers)
    {
        var interfaceDeclaration = SyntaxFactory.InterfaceDeclaration(GetInterfaceName())
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        var methodDeclarations = interfaceMembers.methodDeclarationSyntaxList;

        foreach (var methodDeclaration in methodDeclarations)
        {
            var methodIdentifier = methodDeclaration.Identifier;
            var methodParams = methodDeclaration.ParameterList.Parameters;
            var returnType = methodDeclaration.ReturnType;

            var methodSignature = SyntaxFactory.MethodDeclaration(returnType, methodIdentifier)
                .AddParameterListParameters(methodParams.ToArray())
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

            interfaceDeclaration = interfaceDeclaration.AddMembers(methodSignature);
        }


        var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(INTERFACE_NAMESPACE))
            .AddMembers(interfaceDeclaration);

        var compilationUnit = SyntaxFactory.CompilationUnit()
            .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        var code = compilationUnit.ToFullString();
        FilesManager.SaveFile(_interfacePath, $"{GetInterfaceName()}.cs", code);
    }

    void GenerateClasses(InterfaceMembers interfaceMembers)
    {
        foreach (var fileClassDeclaration in interfaceMembers.fileClassDeclarationsList)
        {
            var classDeclaration = fileClassDeclaration.classDeclaration;
            var root = classDeclaration.SyntaxTree.GetRoot();

            var modifiedClass =
                classDeclaration.AddBaseListTypes(
                    SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(GetInterfaceName())));
            var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(INTERFACE_NAMESPACE));
            var modifiedRoot = root.ReplaceNode(classDeclaration, modifiedClass);

            var namespaceDeclaration =
                modifiedRoot.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();

            var newNamespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(CLASSES_NAMESPACE))
                .WithMembers(namespaceDeclaration.Members)
                .NormalizeWhitespace();

            modifiedRoot = modifiedRoot.ReplaceNode(namespaceDeclaration, newNamespaceDeclaration);

            modifiedRoot = ((CompilationUnitSyntax)modifiedRoot).AddUsings(usingDirective);

            var modifiedCode = modifiedRoot.NormalizeWhitespace().ToFullString();
            FilesManager.SaveFile(_classPath, $"{classDeclaration.Identifier.Text}.cs", modifiedCode);
        }
    }

    private string GetInterfaceName()
    {
        return $"{INTERFACE_NAME}{_generatedInterfaceNum}";
    }
}
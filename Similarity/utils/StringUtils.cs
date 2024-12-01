using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Similarity.utils;

public static class StringUtils
{
    public static string GetFormattedMethodName(MethodDeclarationSyntax method)
    {
        return $"{method.ReturnType.ToString()}{method.Identifier.Text}({method.ParameterList})";
    }
}
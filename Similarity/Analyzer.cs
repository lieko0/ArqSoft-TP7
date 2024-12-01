using Similarity.dto;
using Similarity.utils;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Similarity;

public class Analyzer
{
    private FileClassDeclarations[] ClassDeclarations { get; }

    private readonly List<RefactorOportunity> _refactorOpportunities  = new();
    
    private readonly Dictionary<string, string> _env;

    public Analyzer(IEnumerable<string> files)
    {
        _env = DotEnv.GetEnv(".env");
        CsFile[] syntaxTrees = files.Select(f => new CsFile(f)).ToArray();
        ClassDeclarations = syntaxTrees.SelectMany(f => f.classDeclarations).ToArray();
    }

    public void Analyze()
    {
        CheckSimilarNodes();
    }
    
    public List<InterfaceMembers> GetRefactorOpportunities()
    {
        var relationships = new Dictionary<string, List<Relationship>>();
        foreach (var opportunity in _refactorOpportunities)
        {
            var dictKey = StringUtils.GetFormattedMethodName(opportunity.methodA);
            
            if (relationships.ContainsKey(dictKey))
            {
                var relationship = new Relationship(opportunity.fileB, opportunity.methodB);
                relationships[dictKey].Add(relationship);
            }
            else
            {
                var methodA = new Relationship(opportunity.fileA, opportunity.methodA);
                var methodB = new Relationship(opportunity.fileB, opportunity.methodB);
                var list = new List<Relationship> {methodA, methodB};
                
                relationships[dictKey] = list;
            }
        }
        
        var merged = GetMergedMethodsAndClasses(relationships);

        return RemoveDuplicatedInterfaceMembers(merged);
    }
    
    private List<InterfaceMembers> GetMergedMethodsAndClasses(Dictionary<string, List<Relationship>> relationships)
    {
        var interfaceMembers = new List<InterfaceMembers>();

        foreach (var key in relationships.Keys)
        {
            var methodDeclarations = relationships[key].Select(m => m.methodDeclaration).ToList();
            var classDeclarations = relationships[key].Select(r => r.classDeclaration).ToList();
            bool canAdd = true;
            
            foreach (var members in interfaceMembers)
            {
                bool hasSameClasses = classDeclarations.All(c => members.fileClassDeclarationsList.Contains(c));
                
                if (hasSameClasses)
                {
                    members.methodDeclarationSyntaxList.AddRange(methodDeclarations);
                    canAdd = false;
                    break;
                }
            }
            
            foreach (var members in interfaceMembers)
            {
                bool hasSameClasses = classDeclarations.All(c => members.fileClassDeclarationsList.Contains(c));
                bool hasAllMethods = methodDeclarations.All(m => members.methodDeclarationSyntaxList.Contains(m));
                if (hasSameClasses && hasAllMethods)
                {
                    canAdd = false;
                    break;
                }
            }

            if (canAdd)
            {
                var interfaceMember = new InterfaceMembers(classDeclarations, methodDeclarations);
                interfaceMembers.Add(interfaceMember);
            }
        }

        return interfaceMembers;
    }
    
    private List<InterfaceMembers> RemoveDuplicatedInterfaceMembers(List<InterfaceMembers> interfaceMembersList)
    {
        foreach (var interfaceMembers in interfaceMembersList)
        {
            interfaceMembers.methodDeclarationSyntaxList = interfaceMembers.methodDeclarationSyntaxList.Distinct(new MethodDeclarationSyntaxComparer()).ToList();
            interfaceMembers.fileClassDeclarationsList = interfaceMembers.fileClassDeclarationsList.Distinct().ToList();
        }

        return interfaceMembersList;
    }
    
    
    private void CheckSimilarNodes()
    {
        for (int i = 0; i < ClassDeclarations.Length; i++)
        {
            for (int j = i + 1; j < ClassDeclarations.Length; j++)
            {
                var iClassMethods = ClassDeclarations[i].classDeclaration.Members.OfType<MethodDeclarationSyntax>().ToList();
                var jClassMethods = ClassDeclarations[j].classDeclaration.Members.OfType<MethodDeclarationSyntax>().ToList();

                
                if (!_env.TryGetValue("JACCARD_THRESHOLD",out string? threshold))
                {
                    Console.WriteLine("JACCARD_THRESHOLD not found in .env file, using default value 0.5");
                    threshold = "0.5";
                }
                
                CheckJaccardSimilarity(iClassMethods, jClassMethods, ClassDeclarations[i], ClassDeclarations[j], Convert.ToDouble(threshold));
            }
        }
    }

    private void CheckJaccardSimilarity(List<MethodDeclarationSyntax> iClassMethods,
        List<MethodDeclarationSyntax> jClassMethods, FileClassDeclarations i, FileClassDeclarations j, Double threshold)
    {
        // a = the number of dependencies on both entities,
        // b = the number of dependencies on entity i only,
        // c = the number of dependencies on entity j only
        // Jaccard a/(a + b + c)
        
        int a = 0;
        int b = 0;
        int c = 0;
        var localOpportunities= new List<RefactorOportunity>(); 
        foreach (var iMethod in iClassMethods)
        {
            foreach (var jMethod in jClassMethods)
            {
                var sameReturnType = HasSameReturnType(iMethod, jMethod);
                var sameParameters = HasSameParameters(iMethod, jMethod);
                var hasSameName = HasSameName(iMethod, jMethod);

                if (sameReturnType && sameParameters && hasSameName)
                {
                    localOpportunities.Add(new RefactorOportunity(i, j, iMethod, jMethod));
                    a++;
                }
            }
        }
        b = iClassMethods.Count - a;
        c = jClassMethods.Count - a;

        if ((double) a / (a + b + c) > threshold)
        {
            _refactorOpportunities.AddRange(localOpportunities);
        }
    }
    

    private bool HasSameReturnType(MethodDeclarationSyntax a, MethodDeclarationSyntax b)
    {
        return a.ReturnType.ToString() == b.ReturnType.ToString();
    }

    private bool HasSameParameters(MethodDeclarationSyntax a, MethodDeclarationSyntax b)
    {
        var aParams = a.ParameterList.Parameters.Select(p => p.ToString()).ToList();
        var bParams = b.ParameterList.Parameters.Select(p => p.ToString()).ToList();
        return aParams.SequenceEqual(bParams);
    }
    

    private bool HasSameName(MethodDeclarationSyntax a, MethodDeclarationSyntax b)
    {
        return a.Identifier.Text == b.Identifier.Text;
    }
}
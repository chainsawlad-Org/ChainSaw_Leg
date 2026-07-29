using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class AssemblyDependencyArchitectureTests
{
    private const string ProjectAssemblyPrefix = "ChainSawLeg.";
    private const string ProjectAssetsRoot = "Assets/_Project";

    [Test]
    public void ProjectAssembliesBelongToKnownArchitectureLayers()
    {
        IReadOnlyList<AssemblyDefinition> definitions = LoadProjectAssemblyDefinitions();

        string[] violations = definitions
            .Where(definition => GetLayer(definition.Name) == AssemblyLayer.Unknown)
            .Select(definition =>
                $"{definition.Name} ({definition.AssetPath}) does not belong to a known architecture layer.")
            .ToArray();

        AssertNoViolations(
            violations,
            "Every project assembly must have a name that identifies its architecture layer.");
    }

    [Test]
    public void ProjectAssemblyReferencesFollowAllowedDependencyDirections()
    {
        IReadOnlyList<AssemblyDefinition> definitions = LoadProjectAssemblyDefinitions();
        Dictionary<string, AssemblyDefinition> definitionsByName =
            definitions.ToDictionary(definition => definition.Name, StringComparer.Ordinal);
        var violations = new List<string>();

        foreach (AssemblyDefinition source in definitions.OrderBy(definition => definition.Name))
        {
            AssemblyLayer sourceLayer = GetLayer(source.Name);

            foreach (string reference in ResolveProjectReferences(source, definitions, violations))
            {
                if (!definitionsByName.TryGetValue(reference, out AssemblyDefinition target))
                {
                    violations.Add(
                        $"{source.Name} references {reference}, but that project assembly was not found under " +
                        $"{ProjectAssetsRoot}.");
                    continue;
                }

                AssemblyLayer targetLayer = GetLayer(target.Name);
                if (!IsDependencyAllowed(sourceLayer, targetLayer))
                {
                    violations.Add(
                        $"{source.Name} ({sourceLayer}) must not reference {target.Name} ({targetLayer}).");
                }
            }
        }

        AssertNoViolations(
            violations,
            "Project assembly references must point only in the allowed architecture direction.");
    }

    [Test]
    public void ProjectAssemblyDependencyGraphHasNoCycles()
    {
        IReadOnlyList<AssemblyDefinition> definitions = LoadProjectAssemblyDefinitions();
        var loadViolations = new List<string>();
        Dictionary<string, string[]> graph = definitions.ToDictionary(
            definition => definition.Name,
            definition => ResolveProjectReferences(definition, definitions, loadViolations)
                .OrderBy(reference => reference)
                .ToArray(),
            StringComparer.Ordinal);

        var cycles = new HashSet<string>(StringComparer.Ordinal);
        var states = new Dictionary<string, VisitState>(StringComparer.Ordinal);
        var path = new List<string>();

        foreach (string assemblyName in graph.Keys.OrderBy(name => name))
        {
            FindCycles(assemblyName, graph, states, path, cycles);
        }

        var violations = new List<string>(loadViolations);
        violations.AddRange(cycles.Select(cycle => $"Circular dependency: {cycle}."));

        AssertNoViolations(
            violations,
            "Project assembly dependency graph must be acyclic.");
    }

    [Test]
    public void GameAssembliesDoNotReferenceDependencyInjectionFramework()
    {
        IReadOnlyList<AssemblyDefinition> definitions = LoadProjectAssemblyDefinitions();

        string[] violations = definitions
            .Where(definition =>
            {
                AssemblyLayer layer = GetLayer(definition.Name);
                return layer == AssemblyLayer.GameShared || layer == AssemblyLayer.Feature;
            })
            .SelectMany(definition => definition.References
                .Where(IsDependencyInjectionReference)
                .Select(reference =>
                    $"{definition.Name} must not reference dependency injection assembly {reference}."))
            .ToArray();

        AssertNoViolations(
            violations,
            "Game code must remain independent from the dependency injection framework.");
    }

    private static IReadOnlyList<AssemblyDefinition> LoadProjectAssemblyDefinitions()
    {
        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName
            ?? throw new InvalidOperationException("Unable to determine the Unity project root.");

        string[] assetPaths = AssetDatabase
            .FindAssets("t:AssemblyDefinitionAsset", new[] { ProjectAssetsRoot })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .OrderBy(path => path)
            .ToArray();

        var definitions = new List<AssemblyDefinition>(assetPaths.Length);
        foreach (string assetPath in assetPaths)
        {
            string absolutePath = Path.Combine(projectRoot, assetPath);
            AssemblyDefinitionJson json =
                JsonUtility.FromJson<AssemblyDefinitionJson>(File.ReadAllText(absolutePath));

            if (json == null || string.IsNullOrWhiteSpace(json.name))
            {
                Assert.Fail($"Unable to read assembly definition at {assetPath}.");
            }

            definitions.Add(new AssemblyDefinition(
                json.name,
                assetPath,
                AssetDatabase.AssetPathToGUID(assetPath),
                json.references ?? Array.Empty<string>()));
        }

        return definitions;
    }

    private static IEnumerable<string> ResolveProjectReferences(
        AssemblyDefinition source,
        IReadOnlyList<AssemblyDefinition> definitions,
        ICollection<string> violations)
    {
        Dictionary<string, string> namesByGuid = definitions.ToDictionary(
            definition => definition.Guid,
            definition => definition.Name,
            StringComparer.OrdinalIgnoreCase);

        foreach (string reference in source.References)
        {
            string resolvedReference = reference;
            if (reference.StartsWith("GUID:", StringComparison.OrdinalIgnoreCase))
            {
                string guid = reference.Substring("GUID:".Length);
                if (!namesByGuid.TryGetValue(guid, out resolvedReference))
                {
                    continue;
                }
            }

            if (resolvedReference.StartsWith(ProjectAssemblyPrefix, StringComparison.Ordinal))
            {
                yield return resolvedReference;
            }
        }
    }

    private static AssemblyLayer GetLayer(string assemblyName)
    {
        if (assemblyName.StartsWith("ChainSawLeg.Project.Tests", StringComparison.Ordinal))
        {
            return AssemblyLayer.Tests;
        }

        if (assemblyName.StartsWith("ChainSawLeg.Composition.Runtime", StringComparison.Ordinal))
        {
            return AssemblyLayer.Composition;
        }

        if (assemblyName.StartsWith("ChainSawLeg.Application.Runtime", StringComparison.Ordinal)
            || assemblyName.StartsWith("ChainSawLeg.Coordination.Runtime", StringComparison.Ordinal))
        {
            return AssemblyLayer.Application;
        }

        if (assemblyName.StartsWith("ChainSawLeg.Infrastructure.Runtime", StringComparison.Ordinal))
        {
            return AssemblyLayer.Infrastructure;
        }

        if (assemblyName.StartsWith("ChainSawLeg.Game.Shared.Runtime", StringComparison.Ordinal))
        {
            return AssemblyLayer.GameShared;
        }

        if (assemblyName.StartsWith("ChainSawLeg.Features.", StringComparison.Ordinal))
        {
            return AssemblyLayer.Feature;
        }

        if (assemblyName.StartsWith("ChainSawLeg.UI.Runtime", StringComparison.Ordinal))
        {
            return AssemblyLayer.UI;
        }

        return AssemblyLayer.Unknown;
    }

    private static bool IsDependencyAllowed(AssemblyLayer source, AssemblyLayer target)
    {
        switch (source)
        {
            case AssemblyLayer.Application:
                return target == AssemblyLayer.Application
                    || target == AssemblyLayer.Infrastructure
                    || target == AssemblyLayer.GameShared
                    || target == AssemblyLayer.Feature
                    || target == AssemblyLayer.UI;

            case AssemblyLayer.Composition:
                return target != AssemblyLayer.Tests
                    && target != AssemblyLayer.Unknown;

            case AssemblyLayer.Infrastructure:
            case AssemblyLayer.Feature:
            case AssemblyLayer.UI:
                return target == AssemblyLayer.GameShared;

            case AssemblyLayer.GameShared:
                return false;

            case AssemblyLayer.Tests:
                return target != AssemblyLayer.Tests
                    && target != AssemblyLayer.Unknown;

            default:
                return false;
        }
    }

    private static bool IsDependencyInjectionReference(string reference)
    {
        return reference.IndexOf("Zenject", StringComparison.OrdinalIgnoreCase) >= 0
            || reference.IndexOf("Extenject", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static void FindCycles(
        string assemblyName,
        IReadOnlyDictionary<string, string[]> graph,
        IDictionary<string, VisitState> states,
        IList<string> path,
        ISet<string> cycles)
    {
        if (states.TryGetValue(assemblyName, out VisitState state))
        {
            if (state == VisitState.Visiting)
            {
                int cycleStart = path.IndexOf(assemblyName);
                if (cycleStart >= 0)
                {
                    cycles.Add(string.Join(" -> ", path.Skip(cycleStart).Concat(new[] { assemblyName })));
                }
            }

            return;
        }

        states[assemblyName] = VisitState.Visiting;
        path.Add(assemblyName);

        if (graph.TryGetValue(assemblyName, out string[] references))
        {
            foreach (string reference in references.Where(graph.ContainsKey))
            {
                FindCycles(reference, graph, states, path, cycles);
            }
        }

        path.RemoveAt(path.Count - 1);
        states[assemblyName] = VisitState.Visited;
    }

    private static void AssertNoViolations(
        IReadOnlyCollection<string> violations,
        string ruleDescription)
    {
        if (violations.Count == 0)
        {
            return;
        }

        Assert.Fail($"{ruleDescription}{Environment.NewLine}- {string.Join(Environment.NewLine + "- ", violations)}");
    }

    [Serializable]
    private sealed class AssemblyDefinitionJson
    {
        public string name;
        public string[] references;
    }

    private sealed class AssemblyDefinition
    {
        public AssemblyDefinition(string name, string assetPath, string guid, string[] references)
        {
            Name = name;
            AssetPath = assetPath;
            Guid = guid;
            References = references;
        }

        public string Name { get; }
        public string AssetPath { get; }
        public string Guid { get; }
        public string[] References { get; }
    }

    private enum AssemblyLayer
    {
        Unknown,
        Application,
        Composition,
        Infrastructure,
        GameShared,
        Feature,
        UI,
        Tests
    }

    private enum VisitState
    {
        Visiting,
        Visited
    }
}

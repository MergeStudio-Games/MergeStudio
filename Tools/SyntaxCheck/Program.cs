using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
var root = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
var files = Directory.GetFiles(Path.Combine(root, "Assets"), "*.cs", SearchOption.AllDirectories);
int errors = 0;
var trees = new List<SyntaxTree>();
foreach (var file in files)
{
    var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file), new CSharpParseOptions(LanguageVersion.CSharp9), file);
    trees.Add(tree);
    foreach (var diagnostic in tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error)) { Console.WriteLine(diagnostic); errors++; }
}
Console.WriteLine($"C# syntax: {files.Length} files, {errors} errors. Unity API/type validation requires Unity Editor.");
if (args.Length > 1 && errors == 0)
{
    // Optional additional type check against locally imported assemblies.
    // It does not emulate Unity's asmdef graph, import pipeline or runtime.
    string managed = args[1];
    var candidates = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator).ToList();
    candidates.AddRange(Directory.GetFiles(managed, "Unity*.dll", SearchOption.AllDirectories));
    string android = Path.Combine(Path.GetDirectoryName(managed), "PlaybackEngines", "AndroidPlayer");
    if (Directory.Exists(android)) candidates.AddRange(Directory.GetFiles(android, "Unity*.dll"));
    candidates.AddRange(Directory.GetFiles(Path.Combine(root, "Library", "ScriptAssemblies"), "*.dll")
        .Where(path => !Path.GetFileName(path).StartsWith("MergeStudio.")));
    candidates.AddRange(Directory.GetFiles(Path.Combine(root, "Library", "PackageCache"), "nunit.framework.dll", SearchOption.AllDirectories));
    var references = candidates.Where(path => Path.GetFileName(path) != "UnityEngine.dll" && Path.GetFileName(path) != "UnityEditor.dll")
        .GroupBy(Path.GetFileName).Select(group => MetadataReference.CreateFromFile(group.First()));
    var compilation = CSharpCompilation.Create("MergeStudio.TypeCheck", trees, references,
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    foreach (var diagnostic in compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error))
    { Console.WriteLine(diagnostic); errors++; }
    Console.WriteLine($"Imported-assembly type check: {errors} errors. Not a Unity import or test run.");
}
return errors == 0 ? 0 : 1;

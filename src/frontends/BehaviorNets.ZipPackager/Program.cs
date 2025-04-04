﻿using System.Reflection;
using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using ManifestResourceAttributes = AsmResolver.PE.DotNet.Metadata.Tables.ManifestResourceAttributes;

if (args.Length < 2)
{
    Console.WriteLine("Usage: BehaviorNets.ZipPackager <zip-package> <main-exe-file>");
    return 1;
}

string zipPackage = Path.GetFullPath(args[0]);
string mainExecutableFile = args[1];

// Open module.
var module = ModuleDefinition.FromFile(Path.Combine(
    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
    "BehaviorNets.ZipRunner.exe"));

// Embed the zip into the runner.
module.Resources.Add(new ManifestResource(
    "application.zip", 
    ManifestResourceAttributes.Public, 
    new DataSegment(File.ReadAllBytes(zipPackage))));

// Update instructions in main method that specify the main file to run.
var instructions = module.ManagedEntryPointMethod!.CilMethodBody!.Instructions
    .Where(i => i.OpCode.Code == CilCode.Ldstr && i.Operand?.ToString() == "%mainFileToRun%")
    .ToArray();

foreach (var instruction in instructions) 
    instruction.Operand = mainExecutableFile;

// Save.
string outputPath = Path.Combine(
    Path.GetDirectoryName(zipPackage)!,
    Path.ChangeExtension(Path.GetFileName(mainExecutableFile), ".exe"));
module.Write(outputPath);
return 0;
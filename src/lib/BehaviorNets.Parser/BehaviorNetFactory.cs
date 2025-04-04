using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using BehaviorNets.Model;
using BehaviorNets.Parser.Internal;

namespace BehaviorNets.Parser;

/// <summary>
/// Provides a mechanism for compiling behavior nets using the behavior net Domain Specific Language (DSL).
/// </summary>
public static class BehaviorNetFactory
{
    /// <summary>
    /// Loads a behavior net from the provided file.
    /// </summary>
    /// <param name="path">The path to the file containing the behavior net in the DSL format.</param>
    /// <returns>The behavior net.</returns>
    public static BehaviorNet FromFile(string path) => FromCharStream(new AntlrFileStream(path));

    /// <summary>
    /// Loads a behavior net from the provided string.
    /// </summary>
    /// <param name="text">The string containing the behavior net in the DSL format.</param>
    /// <returns>The behavior net.</returns>
    public static BehaviorNet FromText(string text) => FromText(new StringReader(text));

    /// <summary>
    /// Loads a behavior net from the provided reader.
    /// </summary>
    /// <param name="reader">The input stream containing the behavior net in the DSL format.</param>
    /// <returns>The behavior net.</returns>
    public static BehaviorNet FromText(TextReader reader) => FromCharStream(new AntlrInputStream(reader));

    private static BehaviorNet FromCharStream(ICharStream inputStream)
    {
        var lexer = new BehaviorNetLexer(inputStream);
        var parser = new BehaviorNetParser(new BufferedTokenStream(lexer));
        var tree = parser.behavior();

        var walker = new ParseTreeWalker();
        var builder = new BehaviorNetBuilder();
        walker.Walk(builder, tree);

        return builder.Result!;
    }
}
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AnalyzerNullReference
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AnalyzerNullReferenceCodeFixProvider)), Shared]
    public class AnalyzerNullReferenceCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(AnalyzerNullReferenceAnalyzer.DiagnosticId); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {

            var diagnostic = context.Diagnostics.First();

            var document = context.Document;

            var root = await document.GetSyntaxRootAsync();

            var declaration = root.FindNode(diagnostic.Location.SourceSpan);

            context.RegisterCodeFix(CodeAction.Create("Delete null reference check", async c =>
            {
                var statement = root.FindNode(diagnostic.Location.SourceSpan);
                while ((statement.Parent.Kind() != SyntaxKind.IfStatement) && //delete if node
                       (statement.Parent.Kind() != SyntaxKind.LocalDeclarationStatement) && //delete variable node
                       (statement.Parent.Kind() != SyntaxKind.ReturnStatement)) //delete return node
                {
                    statement = statement.Parent;
                }

                var newRoot = root.RemoveNode(statement.Parent, SyntaxRemoveOptions.KeepNoTrivia);

                return document.WithSyntaxRoot(newRoot);

            }), diagnostic);

        }

    }
}

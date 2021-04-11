using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace AnalyzerNullReference
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AnalyzerNullReferenceAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AnalyzerNullReference";


        private static readonly string Title = "Remove variable checks for null refeence";
        private static readonly string MessageFormat = "Waiting deletion if with check null reference";
        private static readonly string Description = "Remove null reference checking";
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }
        public override void Initialize(AnalysisContext context)
        {

            context.RegisterSyntaxTreeAction(syntaxTreeContext =>
            {
                var root = syntaxTreeContext.Tree.GetRoot(syntaxTreeContext.CancellationToken);

                foreach (var statement in root.DescendantNodes().OfType<LiteralExpressionSyntax>())
                {
                    if (statement.Kind() != SyntaxKind.NullLiteralExpression)
                    {
                        continue;
                    }

                    if (!(statement.Parent.Kind() is SyntaxKind.EqualsExpression))
                    {
                        continue;
                    }

                    if ((statement.Parent.Parent.Kind() is SyntaxKind.IfStatement) ||  //for check in simple if
                        (statement.Parent.Parent.Kind() is SyntaxKind.EqualsValueClause) || //for check in Equals bool variable
                        (statement.Parent.Parent.Kind() is SyntaxKind.ConditionalExpression) || //for check in ternary if
                        (statement.Parent.Parent.Kind() is SyntaxKind.ReturnStatement)) //for check in return
                    {
                        var diagnostic = Diagnostic.Create(Rule, statement.Parent.GetLocation());
                        syntaxTreeContext.ReportDiagnostic(diagnostic);
                    }
                }
            });

        }
    }
}


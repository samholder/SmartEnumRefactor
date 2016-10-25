using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace RefactorEnumToSmartEnum
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(RefactorEnumToSmartEnumCodeRefactoringProvider)),
     Shared]
    internal class RefactorEnumToSmartEnumCodeRefactoringProvider : CodeRefactoringProvider
    {
        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            // TODO: Replace the following code with your own analysis, generating a CodeAction for each refactoring to offer

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // Find the node at the selection.
            var node = root.FindNode(context.Span);

            // Only offer a refactoring if the selected node is an enum declaration node.
            var typeDecl = node as EnumDeclarationSyntax;
            if (typeDecl == null)
            {
                return;
            }

            // For any type declaration node, create a code action to reverse the identifier text.
            var action = CodeAction.Create("Convert to smart enum", c => CreateSmartEnum(context.Document, typeDecl, c));

            // Register this code action.
            context.RegisterRefactoring(action);
        }

        private static async Task<Solution> CreateSmartEnum(Document document, EnumDeclarationSyntax enumDeclaration,
            CancellationToken cancellationToken)
        {
            string enumName = enumDeclaration.Identifier.Text;
            var originalProject = document.Project;
            var className = enumName;
            var newDocument = originalProject.AddDocument(className,
                SmartEnumGenerator.CreateSmartEnumClass(enumDeclaration, className));
            var documentRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var updatedDocument= documentRoot.ReplaceNode(enumDeclaration, newDocument.GetSyntaxRootAsync(cancellationToken).Result);
            return document.WithSyntaxRoot(updatedDocument).Project.Solution;
        }
    }
}
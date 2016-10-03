using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorEnumToSmartEnum
{
    internal class SmartEnumGenerator
    {
        public static SyntaxNode CreateSmartEnumClass(EnumDeclarationSyntax enumDeclaration, string className)
        {
            var enumClassMembers = new SyntaxList<MemberDeclarationSyntax>();
            enumClassMembers = enumClassMembers.Add(CreateValueField());
            enumClassMembers = enumClassMembers.Add(CreateConstructor(className));
            enumClassMembers = enumClassMembers.Add(CreateImplicitConversionToString(className));
            enumClassMembers = enumClassMembers.Add(CreateParseMethod(className, enumDeclaration));
            enumClassMembers = enumClassMembers.Add(CreateInnerClass(className, enumDeclaration));
            return CreateClass(className, enumClassMembers).NormalizeWhitespace();
        }

        private static MemberDeclarationSyntax CreateParseMethod(string className, EnumDeclarationSyntax enumDeclaration)
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.IdentifierName(className),
                SyntaxFactory.Identifier("Parse"))
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        new[]
                        {
                            SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                            SyntaxFactory.Token(SyntaxKind.StaticKeyword)
                        }))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(
                            SyntaxFactory.Parameter(
                                SyntaxFactory.Identifier("value"))
                                .WithType(
                                    SyntaxFactory.PredefinedType(
                                        SyntaxFactory.Token(SyntaxKind.StringKeyword))))))
                .WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.SingletonList<StatementSyntax>(
                            SyntaxFactory.SwitchStatement(
                                SyntaxFactory.IdentifierName("value"))
                                .WithSections(
                                    GenerateSwitchStatementSections(enumDeclaration,className)))));
        }

        private static SyntaxList<SwitchSectionSyntax> GenerateSwitchStatementSections(EnumDeclarationSyntax enumDeclaration, string className)
        {
            var swicthSections = SyntaxFactory.List<SwitchSectionSyntax>();
            foreach (var member in enumDeclaration.Members)
            {
                swicthSections = swicthSections.Add(GenerateEnumMemeberSwitchSection(member,className));
            }
            swicthSections = swicthSections.Add(GenerateDefaultSwitchSection(className));
            return swicthSections;
            
        }

        private static SwitchSectionSyntax GenerateDefaultSwitchSection(string className)
        {
            return SyntaxFactory.SwitchSection()
                .WithLabels(
                    SyntaxFactory.SingletonList<SwitchLabelSyntax>(
                        SyntaxFactory.DefaultSwitchLabel()))
                .WithStatements(
                    SyntaxFactory.SingletonList<StatementSyntax>(
                        SyntaxFactory.ThrowStatement(
                            SyntaxFactory.ObjectCreationExpression(
                                SyntaxFactory.IdentifierName("ArgumentException"))
                                .WithArgumentList(
                                    SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.InterpolatedStringExpression(
                                                    SyntaxFactory.Token(SyntaxKind.InterpolatedStringStartToken))
                                                    .WithContents(
                                                        SyntaxFactory.List<InterpolatedStringContentSyntax>(
                                                            new InterpolatedStringContentSyntax[]{
                                                                SyntaxFactory.InterpolatedStringText()
                                                                    .WithTextToken(
                                                                        SyntaxFactory.Token(
                                                                            SyntaxFactory.TriviaList(),
                                                                            SyntaxKind.InterpolatedStringTextToken,
                                                                            "the given value ",
                                                                            "the given value ",
                                                                            SyntaxFactory.TriviaList())),
                                                                SyntaxFactory.Interpolation(
                                                                    SyntaxFactory.IdentifierName("value")),
                                                                SyntaxFactory.InterpolatedStringText()
                                                                    .WithTextToken(
                                                                        SyntaxFactory.Token(
                                                                            SyntaxFactory.TriviaList(),
                                                                            SyntaxKind.InterpolatedStringTextToken,
                                                                            " could not be parsed as a " + className,
                                                                            " could not be parsed as a " + className,
                                                                            SyntaxFactory.TriviaList()))})))))))));
        }

        private static SwitchSectionSyntax GenerateEnumMemeberSwitchSection(EnumMemberDeclarationSyntax member,string className)
        {
            return SyntaxFactory.SwitchSection()
                .WithLabels(
                    SyntaxFactory.SingletonList<SwitchLabelSyntax>(
                        SyntaxFactory.CaseSwitchLabel(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                SyntaxFactory.Literal(member.Identifier.Text)))))
                .WithStatements(
                    SyntaxFactory.SingletonList<StatementSyntax>(
                        SyntaxFactory.ReturnStatement(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(className),
                                SyntaxFactory.IdentifierName(member.Identifier.Text)))));
        }

        private static MemberDeclarationSyntax CreateImplicitConversionToString(string className)
        {
            return SyntaxFactory.ConversionOperatorDeclaration(
                SyntaxFactory.Token(SyntaxKind.ImplicitKeyword),
                SyntaxFactory.PredefinedType(
                    SyntaxFactory.Token(SyntaxKind.StringKeyword)))
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        new[]
                        {
                            SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                            SyntaxFactory.Token(SyntaxKind.StaticKeyword)
                        }))
                .WithOperatorKeyword(
                    SyntaxFactory.Token(SyntaxKind.OperatorKeyword))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(
                            SyntaxFactory.Parameter(
                                SyntaxFactory.Identifier(className.ToLowerFirstChar()))
                                .WithType(
                                    SyntaxFactory.IdentifierName(className))))
                        .WithOpenParenToken(
                            SyntaxFactory.Token(SyntaxKind.OpenParenToken))
                        .WithCloseParenToken(
                            SyntaxFactory.Token(SyntaxKind.CloseParenToken)))
                .WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.SingletonList<StatementSyntax>(
                            SyntaxFactory.ReturnStatement(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName(className.ToLowerFirstChar()),
                                    SyntaxFactory.IdentifierName("value"))
                                    .WithOperatorToken(
                                        SyntaxFactory.Token(SyntaxKind.DotToken)))
                                .WithReturnKeyword(
                                    SyntaxFactory.Token(SyntaxKind.ReturnKeyword))
                                .WithSemicolonToken(
                                    SyntaxFactory.Token(SyntaxKind.SemicolonToken))))
                        .WithOpenBraceToken(
                            SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
                        .WithCloseBraceToken(
                            SyntaxFactory.Token(SyntaxKind.CloseBraceToken)));
        }

        private static MemberDeclarationSyntax CreateInnerClass(string className, EnumDeclarationSyntax enumDeclaration)
        {
            var enumClassMembers = new SyntaxList<MemberDeclarationSyntax>();
            foreach (var enumMember in enumDeclaration.Members)
            {
                enumClassMembers = enumClassMembers.Add(CreateEnumFieldMethod(enumMember,className));
                enumClassMembers = enumClassMembers.Add(CreateEnumFactoryMethod(enumMember,className));
            }
            return CreateClass(PluralizeClassName(className), enumClassMembers);
        }

        private static MemberDeclarationSyntax CreateEnumFieldMethod(EnumMemberDeclarationSyntax enumMember, string className)
        {
            return SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.IdentifierName(className))
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                            SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier(enumMember.Identifier.Text + "Instance"))
                                .WithInitializer(
                                    SyntaxFactory.EqualsValueClause(
                                        SyntaxFactory.ObjectCreationExpression(
                                            SyntaxFactory.IdentifierName(className))
                                            .WithNewKeyword(
                                                SyntaxFactory.Token(SyntaxKind.NewKeyword))
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.LiteralExpression(
                                                                SyntaxKind.StringLiteralExpression,
                                                                SyntaxFactory.Literal(enumMember.Identifier.Text)))))
                                                    .WithOpenParenToken(
                                                        SyntaxFactory.Token(SyntaxKind.OpenParenToken))
                                                    .WithCloseParenToken(
                                                        SyntaxFactory.Token(SyntaxKind.CloseParenToken))))
                                        .WithEqualsToken(
                                            SyntaxFactory.Token(SyntaxKind.EqualsToken))))))
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        new[]
                        {
                            SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                            SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                            SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)
                        }))
                .WithSemicolonToken(
                    SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        private static MemberDeclarationSyntax CreateEnumFactoryMethod(EnumMemberDeclarationSyntax enumMember, string className)
        {
            return SyntaxFactory.PropertyDeclaration(
                       SyntaxFactory.IdentifierName(className),
                      SyntaxFactory.Identifier(enumMember.Identifier.Text))
                    .WithModifiers(
                       SyntaxFactory.TokenList(
                            new[]{
                        SyntaxFactory.        Token(SyntaxKind.PublicKeyword),
                        SyntaxFactory.        Token(SyntaxKind.StaticKeyword)}))
                    .WithExpressionBody(
                      SyntaxFactory.ArrowExpressionClause(
                       SyntaxFactory.IdentifierName(enumMember.Identifier.Text +"Instance"))
                        .WithArrowToken(
                       SyntaxFactory.Token(SyntaxKind.EqualsGreaterThanToken)))
                    .WithSemicolonToken(
                      SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        private static string PluralizeClassName(string className)
        {
            return className + "s";
        }

                private static MemberDeclarationSyntax CreateConstructor(string className)
                {
                    return SyntaxFactory.ConstructorDeclaration(
                        SyntaxFactory.Identifier(className))
                        .WithModifiers(
                            SyntaxFactory.TokenList(
                                SyntaxFactory.Token(SyntaxKind.PrivateKeyword)))
                        .WithParameterList(
                            SyntaxFactory.ParameterList(
                                SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(
                                    SyntaxFactory.Parameter(
                                        SyntaxFactory.Identifier("value"))
                                        .WithType(
                                            SyntaxFactory.PredefinedType(
                                                SyntaxFactory.Token(SyntaxKind.StringKeyword)))))
                                .WithOpenParenToken(
                                    SyntaxFactory.Token(SyntaxKind.OpenParenToken))
                                .WithCloseParenToken(
                                    SyntaxFactory.Token(SyntaxKind.CloseParenToken)))
                        .WithBody(
                            SyntaxFactory.Block(
                                SyntaxFactory.SingletonList<StatementSyntax>(
                                    SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression()
                                                    .WithToken(
                                                        SyntaxFactory.Token(SyntaxKind.ThisKeyword)),
                                                SyntaxFactory.IdentifierName("value"))
                                                .WithOperatorToken(
                                                    SyntaxFactory.Token(SyntaxKind.DotToken)),
                                            SyntaxFactory.IdentifierName("value"))
                                            .WithOperatorToken(
                                                SyntaxFactory.Token(SyntaxKind.EqualsToken)))
                                        .WithSemicolonToken(
                                            SyntaxFactory.Token(SyntaxKind.SemicolonToken))))
                                .WithOpenBraceToken(
                                    SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
                                .WithCloseBraceToken(
                                    SyntaxFactory.Token(SyntaxKind.CloseBraceToken)));
                }
        
        private static MemberDeclarationSyntax CreateValueField()
        {
            return SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.StringKeyword)))
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                            SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier("value")))))
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        new[]
                        {
                            SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                            SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)
                        }))
                .WithSemicolonToken(
                    SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        private static ClassDeclarationSyntax CreateClass(string className,
            SyntaxList<MemberDeclarationSyntax> classMembers)
        {
            return SyntaxFactory.ClassDeclaration(className)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword))).WithKeyword(
                    SyntaxFactory.Token(SyntaxKind.ClassKeyword))
                .WithOpenBraceToken(
                    SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
                .WithMembers(classMembers)
                .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken));
        }
    }

    public static class StringExtensions
    {
        public static string ToLowerFirstChar(this string input)
        {
            string newString = input;
            if (!String.IsNullOrEmpty(newString) && Char.IsUpper(newString[0]))
                newString = Char.ToLower(newString[0]) + newString.Substring(1);
            return newString;
        }
    }
    
}
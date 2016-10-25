using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
            foreach (var enumMember in enumDeclaration.Members)
            {
                enumClassMembers = enumClassMembers.Add(CreateEnumFieldMethod(enumMember, className));
                enumClassMembers = enumClassMembers.Add(CreateEnumFactoryMethod(enumMember, className));
            }
            enumClassMembers = enumClassMembers.Add(CreateTypedEqualsMethod(className));
            enumClassMembers = enumClassMembers.Add(CreateUntypedEqualsMethod(className));
            enumClassMembers = enumClassMembers.Add(CreateGetHashCodeMethod());
            return CreateClass(className, enumClassMembers).NormalizeWhitespace();
        }

        private static MemberDeclarationSyntax CreateGetHashCodeMethod()
        {
            return MethodDeclaration(
                PredefinedType(
                    Token(SyntaxKind.IntKeyword)),
                Identifier("GetHashCode"))
                .WithModifiers(
                    TokenList(
                        new[]
                        {
                            Token(SyntaxKind.PublicKeyword),
                            Token(SyntaxKind.OverrideKeyword)
                        }))
                .WithBody(
                    Block(
                        SingletonList<StatementSyntax>(
                            ReturnStatement(
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("value"),
                                        IdentifierName("GetHashCode")))))));
        }

        private static MemberDeclarationSyntax CreateUntypedEqualsMethod(string className)
        {
            return MethodDeclaration(
                PredefinedType(
                    Token(SyntaxKind.BoolKeyword)),
                Identifier("Equals"))
                .WithModifiers(
                    TokenList(
                        new[]
                        {
                            Token(SyntaxKind.PublicKeyword),
                            Token(SyntaxKind.OverrideKeyword)
                        }))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList<ParameterSyntax>(
                            Parameter(
                                Identifier("obj"))
                                .WithType(
                                    PredefinedType(
                                        Token(SyntaxKind.ObjectKeyword))))))
                .WithBody(
                    Block(
                        IfStatement(
                            InvocationExpression(
                                IdentifierName("ReferenceEquals"))
                                .WithArgumentList(
                                    ArgumentList(
                                        SeparatedList<ArgumentSyntax>(
                                            new SyntaxNodeOrToken[]
                                            {
                                                Argument(
                                                    LiteralExpression(
                                                        SyntaxKind.NullLiteralExpression)),
                                                Token(SyntaxKind.CommaToken),
                                                Argument(
                                                    IdentifierName("obj"))
                                            }))),
                            ReturnStatement(
                                LiteralExpression(
                                    SyntaxKind.FalseLiteralExpression))),
                        IfStatement(
                            InvocationExpression(
                                IdentifierName("ReferenceEquals"))
                                .WithArgumentList(
                                    ArgumentList(
                                        SeparatedList<ArgumentSyntax>(
                                            new SyntaxNodeOrToken[]
                                            {
                                                Argument(
                                                    ThisExpression()),
                                                Token(SyntaxKind.CommaToken),
                                                Argument(
                                                    IdentifierName("obj"))
                                            }))),
                            ReturnStatement(
                                LiteralExpression(
                                    SyntaxKind.TrueLiteralExpression))),
                        IfStatement(
                            BinaryExpression(
                                SyntaxKind.NotEqualsExpression,
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("obj"),
                                        IdentifierName("GetType"))),
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName("GetType")))),
                            ReturnStatement(
                                LiteralExpression(
                                    SyntaxKind.FalseLiteralExpression))),
                        ReturnStatement(
                            InvocationExpression(
                                IdentifierName("Equals"))
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList<ArgumentSyntax>(
                                            Argument(
                                                CastExpression(
                                                    IdentifierName(className),
                                                    IdentifierName("obj")))))))));
        }

        private static MemberDeclarationSyntax CreateTypedEqualsMethod(string className)
        {
            return MethodDeclaration(
                PredefinedType(
                    Token(SyntaxKind.BoolKeyword)),
                Identifier("Equals"))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.ProtectedKeyword)))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList<ParameterSyntax>(
                            Parameter(
                                Identifier("other"))
                                .WithType(
                                    IdentifierName(className)))))
                .WithBody(
                    Block(
                        SingletonList<StatementSyntax>(
                            ReturnStatement(
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        PredefinedType(
                                            Token(SyntaxKind.StringKeyword)),
                                        IdentifierName("Equals")))
                                    .WithArgumentList(
                                        ArgumentList(
                                            SeparatedList<ArgumentSyntax>(
                                                new SyntaxNodeOrToken[]
                                                {
                                                    Argument(
                                                        IdentifierName("value")),
                                                    Token(SyntaxKind.CommaToken),
                                                    Argument(
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName("other"),
                                                            IdentifierName("value")))
                                                })))))));
        }

        private static MemberDeclarationSyntax CreateParseMethod(string className, EnumDeclarationSyntax enumDeclaration)
        {
            return MethodDeclaration(
                IdentifierName(className),
                Identifier("Parse"))
                .WithModifiers(
                    TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter(
                                Identifier("value"))
                                .WithType(
                                    PredefinedType(
                                        Token(SyntaxKind.StringKeyword))))))
                .WithBody(
                    Block(
                        SingletonList<StatementSyntax>(
                            SwitchStatement(
                                IdentifierName("value"))
                                .WithSections(
                                    GenerateSwitchStatementSections(enumDeclaration, className)))));
        }

        private static SyntaxList<SwitchSectionSyntax> GenerateSwitchStatementSections(
            EnumDeclarationSyntax enumDeclaration, string className)
        {
            var swicthSections = List<SwitchSectionSyntax>();
            foreach (var member in enumDeclaration.Members)
            {
                swicthSections = swicthSections.Add(GenerateEnumMemeberSwitchSection(member, className));
            }
            swicthSections = swicthSections.Add(GenerateDefaultSwitchSection(className));
            return swicthSections;
        }

        private static SwitchSectionSyntax GenerateDefaultSwitchSection(string className)
        {
            return SwitchSection()
                .WithLabels(
                    SingletonList<SwitchLabelSyntax>(
                        DefaultSwitchLabel()))
                .WithStatements(
                    SingletonList<StatementSyntax>(
                        ThrowStatement(
                            ObjectCreationExpression(
                                QualifiedName(
                                    IdentifierName("System"),
                                    IdentifierName("ArgumentException")))
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList(
                                            Argument(
                                                InterpolatedStringExpression(
                                                    Token(SyntaxKind.InterpolatedStringStartToken))
                                                    .WithContents(
                                                        List(
                                                            new InterpolatedStringContentSyntax[]
                                                            {
                                                                InterpolatedStringText()
                                                                    .WithTextToken(
                                                                        Token(
                                                                            TriviaList(),
                                                                            SyntaxKind.InterpolatedStringTextToken,
                                                                            "the given value ",
                                                                            "the given value ",
                                                                            TriviaList())),
                                                                Interpolation(
                                                                    IdentifierName("value")),
                                                                InterpolatedStringText()
                                                                    .WithTextToken(
                                                                        Token(
                                                                            TriviaList(),
                                                                            SyntaxKind.InterpolatedStringTextToken,
                                                                            " could not be parsed as a " + className,
                                                                            " could not be parsed as a " + className,
                                                                            TriviaList()))
                                                            })))))))));
        }

        private static SwitchSectionSyntax GenerateEnumMemeberSwitchSection(EnumMemberDeclarationSyntax member,
            string className)
        {
            return SwitchSection()
                .WithLabels(
                    SingletonList<SwitchLabelSyntax>(
                        CaseSwitchLabel(
                            LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal(member.Identifier.Text)))))
                .WithStatements(
                    SingletonList<StatementSyntax>(
                        ReturnStatement(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(className),
                                IdentifierName(member.Identifier.Text)))));
        }

        private static MemberDeclarationSyntax CreateImplicitConversionToString(string className)
        {
            return ConversionOperatorDeclaration(
                Token(SyntaxKind.ImplicitKeyword),
                PredefinedType(
                    Token(SyntaxKind.StringKeyword)))
                .WithModifiers(
                    TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                .WithOperatorKeyword(
                    Token(SyntaxKind.OperatorKeyword))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter(
                                Identifier(className.ToLowerFirstChar()))
                                .WithType(
                                    IdentifierName(className))))
                        .WithOpenParenToken(
                            Token(SyntaxKind.OpenParenToken))
                        .WithCloseParenToken(
                            Token(SyntaxKind.CloseParenToken)))
                .WithBody(
                    Block(
                        SingletonList<StatementSyntax>(
                            ReturnStatement(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName(className.ToLowerFirstChar()),
                                    IdentifierName("value"))
                                    .WithOperatorToken(
                                        Token(SyntaxKind.DotToken)))
                                .WithReturnKeyword(
                                    Token(SyntaxKind.ReturnKeyword))
                                .WithSemicolonToken(
                                    Token(SyntaxKind.SemicolonToken))))
                        .WithOpenBraceToken(
                            Token(SyntaxKind.OpenBraceToken))
                        .WithCloseBraceToken(
                            Token(SyntaxKind.CloseBraceToken)));
        }


        private static MemberDeclarationSyntax CreateEnumFieldMethod(EnumMemberDeclarationSyntax enumMember,
            string className)
        {
            return FieldDeclaration(
                VariableDeclaration(
                    IdentifierName(className))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(
                                Identifier(enumMember.Identifier.Text + "Instance"))
                                .WithInitializer(
                                    EqualsValueClause(
                                        ObjectCreationExpression(
                                            IdentifierName(className))
                                            .WithNewKeyword(
                                                Token(SyntaxKind.NewKeyword))
                                            .WithArgumentList(
                                                ArgumentList(
                                                    SingletonSeparatedList(
                                                        Argument(
                                                            LiteralExpression(
                                                                SyntaxKind.StringLiteralExpression,
                                                                Literal(enumMember.Identifier.Text)))))
                                                    .WithOpenParenToken(
                                                        Token(SyntaxKind.OpenParenToken))
                                                    .WithCloseParenToken(
                                                        Token(SyntaxKind.CloseParenToken))))
                                        .WithEqualsToken(
                                            Token(SyntaxKind.EqualsToken))))))
                .WithModifiers(
                    TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.StaticKeyword),
                        Token(SyntaxKind.ReadOnlyKeyword)))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));
        }

        private static MemberDeclarationSyntax CreateEnumFactoryMethod(EnumMemberDeclarationSyntax enumMember,
            string className)
        {
            return PropertyDeclaration(
                IdentifierName(className),
                Identifier(enumMember.Identifier.Text))
                .WithModifiers(
                    TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                .WithExpressionBody(
                    ArrowExpressionClause(
                        IdentifierName(enumMember.Identifier.Text + "Instance"))
                        .WithArrowToken(
                            Token(SyntaxKind.EqualsGreaterThanToken)))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));
        }


        private static MemberDeclarationSyntax CreateConstructor(string className)
        {
            return ConstructorDeclaration(
                Identifier(className))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PrivateKeyword)))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter(
                                Identifier("value"))
                                .WithType(
                                    PredefinedType(
                                        Token(SyntaxKind.StringKeyword)))))
                        .WithOpenParenToken(
                            Token(SyntaxKind.OpenParenToken))
                        .WithCloseParenToken(
                            Token(SyntaxKind.CloseParenToken)))
                .WithBody(
                    Block(
                        SingletonList<StatementSyntax>(
                            ExpressionStatement(
                                AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression()
                                            .WithToken(
                                                Token(SyntaxKind.ThisKeyword)),
                                        IdentifierName("value"))
                                        .WithOperatorToken(
                                            Token(SyntaxKind.DotToken)),
                                    IdentifierName("value"))
                                    .WithOperatorToken(
                                        Token(SyntaxKind.EqualsToken)))
                                .WithSemicolonToken(
                                    Token(SyntaxKind.SemicolonToken))))
                        .WithOpenBraceToken(
                            Token(SyntaxKind.OpenBraceToken))
                        .WithCloseBraceToken(
                            Token(SyntaxKind.CloseBraceToken)));
        }

        private static MemberDeclarationSyntax CreateValueField()
        {
            return FieldDeclaration(
                VariableDeclaration(
                    PredefinedType(
                        Token(SyntaxKind.StringKeyword)))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(
                                Identifier("value")))))
                .WithModifiers(
                    TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword)))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));
        }

        private static ClassDeclarationSyntax CreateClass(string className,
            SyntaxList<MemberDeclarationSyntax> classMembers)
        {
            return ClassDeclaration(className)
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword))).WithKeyword(
                    Token(SyntaxKind.ClassKeyword))
                .WithOpenBraceToken(
                    Token(SyntaxKind.OpenBraceToken))
                .WithMembers(classMembers)
                .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken));
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
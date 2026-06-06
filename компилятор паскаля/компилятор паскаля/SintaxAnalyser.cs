using System;
using System.Collections.Generic;

class SyntaxAnalyzer
{
    private LexicalAnalyzer lex;
    private byte sym;

    private const byte
        ERR_UNEXPECTED_TOKEN = 200,
        ERR_VAR_REDECLARED = 201,
        ERR_PROC_REDECLARED = 202,
        ERR_VAR_NOT_DECLARED = 207,
        ERR_PROC_NOT_DECLARED = 208,
        ERR_EXPECTED_STATEMENT = 209,
        ERR_EXTRA_TOKENS_AFTER_DOT = 210;

    private Dictionary<string, SymbolInfo> symbols = new Dictionary<string, SymbolInfo>();

    private enum SymKind { Var, Proc }

    private class SymbolInfo
    {
        public SymKind Kind;
        public string TypeName;
    }

    public SyntaxAnalyzer(LexicalAnalyzer lexAnalyzer)
    {
        lex = lexAnalyzer;
        sym = lex.NextSym();
    }

    public void Parse()
    {
        if (sym == LexicalAnalyzer.ProgramSy)
        {
            NextSym();
            ExpectIdent();
            Expect(LexicalAnalyzer.Semicolon);
        }
        Block();
        Expect(LexicalAnalyzer.Point);
        if (sym != 0)
        {
            ErrorAndSkip(ERR_EXTRA_TOKENS_AFTER_DOT);
        }
    }

    private void Block()
    {
        while (sym == LexicalAnalyzer.VarSy || sym == LexicalAnalyzer.ProcedurenSy)
        {
            if (sym == LexicalAnalyzer.VarSy)
                VarDeclPart();
            else
                ProcDeclPart();
        }
        CompoundStatement();
    }

    private void VarDeclPart()
    {
        NextSym();

        if (sym != LexicalAnalyzer.Ident)
        {
            ErrorAndSkip(ERR_UNEXPECTED_TOKEN,
                LexicalAnalyzer.Semicolon, LexicalAnalyzer.ProcedurenSy,
                LexicalAnalyzer.BeginSy, LexicalAnalyzer.Point);
            return;
        }

        VarDeclaration();

        while (sym == LexicalAnalyzer.Semicolon)
        {
            NextSym();
            if (sym == LexicalAnalyzer.Ident)
                VarDeclaration();
            else
                break;
        }
    }

    private void VarDeclaration()
    {
        List<string> names = IdentList();

        if (sym != LexicalAnalyzer.Colon)
        {
            ErrorAndSkip(ERR_UNEXPECTED_TOKEN,
                LexicalAnalyzer.Colon, LexicalAnalyzer.Semicolon,
                LexicalAnalyzer.ProcedurenSy, LexicalAnalyzer.BeginSy);
            if (sym != LexicalAnalyzer.Colon)
                return;
        }
        NextSym();

        string typeName = ExpectIdent();

        foreach (string name in names)
        {
            if (symbols.ContainsKey(name))
            {
                InputOutput.Error(ERR_VAR_REDECLARED, lex.TokenPosition);
            }
            else
            {
                symbols[name] = new SymbolInfo { Kind = SymKind.Var, TypeName = typeName };
            }
        }
    }

    private List<string> IdentList()
    {
        List<string> names = new List<string>();
        names.Add(ExpectIdent());
        while (sym == LexicalAnalyzer.Comma)
        {
            NextSym();
            names.Add(ExpectIdent());
        }
        return names;
    }

    private void ProcDeclPart()
    {
        while (sym == LexicalAnalyzer.ProcedurenSy)
        {
            ProcedureDeclaration();

            if (sym == LexicalAnalyzer.Semicolon)
            {
                NextSym();
            }
            else
            {
                ErrorAndSkip(ERR_UNEXPECTED_TOKEN,
                    LexicalAnalyzer.ProcedurenSy,
                    LexicalAnalyzer.BeginSy,
                    LexicalAnalyzer.VarSy,
                    LexicalAnalyzer.Point);
                if (sym == LexicalAnalyzer.Semicolon)
                    NextSym();
            }
        }
    }

    private void ProcedureDeclaration()
    {
        NextSym();

        TextPosition procPos = lex.TokenPosition;
        string procName = ExpectIdent();

        if (symbols.ContainsKey(procName))
            InputOutput.Error(ERR_PROC_REDECLARED, procPos);
        else
            symbols[procName] = new SymbolInfo { Kind = SymKind.Proc, TypeName = null };

        if (sym == LexicalAnalyzer.Semicolon)
        {
            NextSym();
        }
        else
        {
            ErrorAndSkip(ERR_UNEXPECTED_TOKEN,
                LexicalAnalyzer.Semicolon,
                LexicalAnalyzer.BeginSy,
                LexicalAnalyzer.VarSy,
                LexicalAnalyzer.Point);
            if (sym == LexicalAnalyzer.Semicolon)
                NextSym();
        }

        Block();
    }

    private void CompoundStatement()
    {
        if (sym == LexicalAnalyzer.BeginSy)
        {
            NextSym();
        }
        else
        {
            ErrorAndSkip(ERR_UNEXPECTED_TOKEN,
                LexicalAnalyzer.BeginSy,
                LexicalAnalyzer.EndSy,
                LexicalAnalyzer.Point);
            if (sym == LexicalAnalyzer.BeginSy)
                NextSym();
            else
                return;
        }

        StatementSequence();
        Expect(LexicalAnalyzer.EndSy);
    }

    private void StatementSequence()
    {
        if (IsStatementStart())
            Statement();

        while (sym == LexicalAnalyzer.Semicolon || IsStatementStart())
        {
            if (sym == LexicalAnalyzer.Semicolon)
            {
                NextSym();
            }
            else
            {
                InputOutput.Error(ERR_UNEXPECTED_TOKEN, lex.TokenPosition);
            }

            if (IsStatementStart())
                Statement();
        }
    }

    private bool IsStatementStart()
    {
        return sym == LexicalAnalyzer.Ident || sym == LexicalAnalyzer.BeginSy;
    }

    private void Statement()
    {
        if (sym == LexicalAnalyzer.Ident)
        {
            string name = lex.IdentName;
            TextPosition pos = lex.TokenPosition;
            NextSym();

            if (sym == LexicalAnalyzer.Assign)
            {
                NextSym();
                if (!symbols.TryGetValue(name, out var info) || info.Kind != SymKind.Var)
                    InputOutput.Error(ERR_VAR_NOT_DECLARED, pos);
                Expression();
            }
            else
            {
                if (!symbols.TryGetValue(name, out var info) || info.Kind != SymKind.Proc)
                {
                    InputOutput.Error(ERR_PROC_NOT_DECLARED, pos);
                    if (sym == LexicalAnalyzer.LeftPar)
                    {
                        NextSym();
                        while (sym != 0 && sym != LexicalAnalyzer.RightPar
                                        && sym != LexicalAnalyzer.Semicolon
                                        && sym != LexicalAnalyzer.EndSy)
                            NextSym();
                        if (sym == LexicalAnalyzer.RightPar)
                            NextSym();
                    }
                    return;
                }

                if (sym == LexicalAnalyzer.LeftPar)
                {
                    NextSym();
                    if (sym != LexicalAnalyzer.RightPar)
                        ExpressionList();
                    Expect(LexicalAnalyzer.RightPar);
                }
            }
        }
        else if (sym == LexicalAnalyzer.BeginSy)
        {
            CompoundStatement();
        }
        else
        {
            ErrorAndSkip(ERR_EXPECTED_STATEMENT,
                LexicalAnalyzer.Semicolon, LexicalAnalyzer.EndSy);
        }
    }

    private void Expression() => Term();

    private void Term()
    {
        if (sym == LexicalAnalyzer.Ident)
        {
            if (!symbols.TryGetValue(lex.IdentName, out var info) || info.Kind != SymKind.Var)
                InputOutput.Error(ERR_VAR_NOT_DECLARED, lex.TokenPosition);
            NextSym();
        }
        else if (sym == LexicalAnalyzer.IntC ||
                 sym == LexicalAnalyzer.FloatC ||
                 sym == LexicalAnalyzer.StringC)
        {
            NextSym();
        }
        else if (sym == LexicalAnalyzer.LeftPar)
        {
            NextSym();
            Expression();
            Expect(LexicalAnalyzer.RightPar);
        }
        else
        {
            ErrorAndSkip(ERR_UNEXPECTED_TOKEN,
                LexicalAnalyzer.Semicolon, LexicalAnalyzer.EndSy,
                LexicalAnalyzer.RightPar, LexicalAnalyzer.Comma);
        }
    }

    private void ExpressionList()
    {
        Expression();
        while (sym == LexicalAnalyzer.Comma)
        {
            NextSym();
            Expression();
        }
    }

    private void NextSym() => sym = lex.NextSym();

    private void Expect(byte expected)
    {
        if (sym == expected)
        {
            NextSym();
        }
        else
        {
            ErrorAndSkip(ERR_UNEXPECTED_TOKEN, expected,
                LexicalAnalyzer.Semicolon, LexicalAnalyzer.EndSy,
                LexicalAnalyzer.BeginSy, LexicalAnalyzer.Point);
            if (sym == expected)
                NextSym();
        }
    }

    private string ExpectIdent()
    {
        if (sym == LexicalAnalyzer.Ident)
        {
            string name = lex.IdentName;
            NextSym();
            return name;
        }
        else
        {
            ErrorAndSkip(ERR_UNEXPECTED_TOKEN, LexicalAnalyzer.Ident,
                LexicalAnalyzer.Colon, LexicalAnalyzer.Semicolon,
                LexicalAnalyzer.EndSy, LexicalAnalyzer.BeginSy,
                LexicalAnalyzer.VarSy, LexicalAnalyzer.ProcedurenSy,
                LexicalAnalyzer.Point);
            if (sym == LexicalAnalyzer.Ident)
            {
                string name = lex.IdentName;
                NextSym();
                return name;
            }
            return "";
        }
    }

    private void ErrorAndSkip(byte errorCode, params byte[] syncTokens)
    {
        InputOutput.Error(errorCode, lex.TokenPosition);
        while (sym != 0 && !((IList<byte>)syncTokens).Contains(sym))
            NextSym();
    }
}
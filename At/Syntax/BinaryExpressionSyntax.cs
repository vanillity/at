﻿using System.Collections.Generic;

namespace At.Syntax
{
/// <summary>Represents syntax for a binary operation.</summary>
public class BinaryExpressionSyntax : ExpressionSyntax
{
    internal BinaryExpressionSyntax
    (
        ExpressionSyntax          leftOperand,
        AtToken                   @operator,
        ExpressionSyntax          rightOperand,
        IExpressionSource         exprSrc = null,
        IEnumerable<AtDiagnostic> diagnostics = null)
        
        : base(new AtSyntaxNode[]{leftOperand,@operator,rightOperand},exprSrc,diagnostics){

        this.Left     = leftOperand;
        this.Operator = @operator;
        this.Right    = rightOperand;
    }

    public ExpressionSyntax Left
    {
        get;
        private set;
    }

    public AtToken Operator
    {
        get;
        private set;
    }

    public ExpressionSyntax Right
    {
        get;
        private set;
    }

    public override IEnumerable<string> PatternStrings()
    {
        var name = PatternName();
        var o = Operator.PatternName();

        foreach(var l in Left.PatternStrings())
        foreach(var r in Right.PatternStrings())
        {
            yield return $"{name}[{o}]({l},{r})";
            yield return $"{name}({l},{r})";
        }
        
        yield return $"{name}[{o}]";
        yield return name;

        foreach(var b in base.PatternStrings())
            yield return b;
    }
}
}
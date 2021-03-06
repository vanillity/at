﻿using System.Collections.Generic;
using At.Syntax;

namespace At
{
internal class PostBlockSyntax : ExpressionSyntax
{
    public PostBlockSyntax(ExpressionSyntax operand,BlockSyntax block,IExpressionSource expSrc,IEnumerable<AtDiagnostic> diagnostics)
    : base(new AtSyntaxNode[] {operand,block}, expSrc, diagnostics)
    {
        this.Operand = operand;
        this.Block = block;
    }

    public BlockSyntax Block {get;}
    public ExpressionSyntax Operand {get;}

    public override bool MatchesPattern(SyntaxPattern p, IDictionary<string,AtSyntaxNode> d = null)
    {
        var t =    p.Text == PatternName
                && p.Token1==null
                && p.Token2==null
                && (p.Content==null || MatchesPatterns(p.Content,new[] {Operand,Block},d))
                
                || base.MatchesPattern(p,d);

        if (t && d != null && p.Key != null)
            d[p.Key] = this;

        return t;
    }

    public override IEnumerable<string> PatternStrings()
    {
        foreach(var o in Operand.PatternStrings())
        foreach(var b in Block.PatternStrings())
        {
            yield return $"{PatternName}({o},{b})";
        }

        yield return PatternName;

        foreach(var x in base.PatternStrings())
            yield return x;
    }
}
}
﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace At
{
public class AtLexer : IDisposable
{
    readonly AtSourceText text;

    public AtLexer(AtSourceText text)
    {
        this.text = text;
    }

    void IDisposable.Dispose()
    {
       
    }
    object @lock = new object();
    bool tokenizing;
    Buffer<char> chars;

    Dictionary<char,TokenKind> singleCharTokens = new Dictionary<char,TokenKind> 
                                                    { {'@',TokenKind.AtSymbol}
                                                     ,{';',TokenKind.SemiColon}
                                                     ,{'<',TokenKind.LessThan}
                                                     ,{'>',TokenKind.GreaterThan}
                                                     ,{':',TokenKind.Colon}
                                                     ,{'{',TokenKind.LeftBrace}
                                                     ,{'}',TokenKind.RightBrace}
                                                    };    
    
    public IEnumerable<AtToken> Lex()
    {   
        lock(@lock)          
        {
           if (this.tokenizing) throw new Exception("ALREADY TOKENIZING");
           this.tokenizing = true;
        }

        this.chars = new Buffer<char>(text.Source);

        yield return new AtToken(TokenKind.StartOfFile,0);

        while (!chars.End)
        {
           var c = lookAhead(1);
           if (c=='\0') break;

           yield return   isSpaceChar(c)                  ? token(TokenKind.Space,chars,isSpaceChar)

                        : isNewLineChar(c)                ? lineBreak(chars, c)

                        : singleCharTokens.ContainsKey(c) ? token(singleCharTokens[c],chars)

                        : c == '\"'                       ? stringLiteral(c)

                        : c == '.'                        ? dot()

                        : token( TokenKind.TokenCluster,chars
                                ,b=>!isWhiteSpace(b) && !singleCharTokens.ContainsKey(b));
        }  

        yield return new AtToken(TokenKind.EndOfFile, position());

        lock(@lock)          
        {
          this.tokenizing = false;
          this.chars = null;
        }
    }

    // . | .. | ...
    AtToken dot()
    {
        moveNext();
        assertCurrent('.');
        var p = position();
       
        if (isNext('.'))
        {
           moveNext();
           if (isNext('.')) 
           {
              moveNext();
              return new AtToken(TokenKind.Ellipsis,p,"...");
           }

           return new AtToken(TokenKind.DotDot,p,"..");
        }
        else
        {
           return new AtToken(TokenKind.Dot,p,".");
        }
    }

    void assertCurrent(char c)
    {
       Debug.Assert(current()==c);
    }

    AtToken stringLiteral(char delimiter)
    {
        moveNext();       
        var p  = position();
        var sb = new StringBuilder().Append(delimiter);
                
        while (!END() && lookAhead(1) != delimiter) 
        {
           if (lookAhead(1)=='\\' && lookAhead(2)==delimiter)
           {
                moveNext();
                moveNext();
                sb.Append('\\').Append(delimiter);
           }
           else
           {
                chars.MoveNext();
                sb.Append(current());           
           }
        }

        moveNext();
        var text = sb.Append(current()).ToString();
        return new AtToken(TokenKind.StringLiteral,p,text);    
    }

    static AtToken lineBreak(Buffer<char> buffer, char c)
    {
        buffer.MoveNext();
        var p = buffer.Position;

        if (c == '\r' && buffer.LookAhead(1) == '\n')
        {
            buffer.MoveNext();
            return new AtToken(TokenKind.NewLine, p, "\r\n");
        }
        else
        {
            return new AtToken(TokenKind.NewLine, p, c.ToString());
        }
    }

    bool isWhiteSpace(char b)
    {
        return isSpaceChar(b) || isNewLineChar(b);
    }

    private bool isNewLineChar(char c)
    {
        return c=='\r'||c=='\n';
    }


    bool isSpaceChar(char c)
    {
        return c==' ' || c=='\t';
    }

    AtToken token(TokenKind kind, Buffer<char> buffer, Func<char,bool> predicate=null)
    {
        buffer.MoveNext();
        if (predicate==null) predicate = c => false;     
       
        var sb  = new StringBuilder().Append(buffer.Current);
        var pos = buffer.Position;
        
        while (!buffer.End && buffer.LookAhead(1)!='\0' && predicate(buffer.LookAhead(1))) 
        {
           buffer.MoveNext();
           sb.Append(buffer.Current);           
        }

        var text = sb.ToString();
        return new AtToken(kind,pos,text);
    }

    bool isNext(char c, int i = 1) => chars.LookAhead(i)==c; 
    bool moveNext()                => chars.MoveNext();
    char lookAhead(int k)          => chars.LookAhead(k);
    char current()                 => chars.Current;
    bool END()                     => chars.End;
    int  position()                => chars.Position;

}
}
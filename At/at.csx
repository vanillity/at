﻿//CSharpScript (csi.exe)

//Environment.CurrentDirectory = @"E:\Src\2016\At2016\At\";

#r "..\packages\Microsoft.CodeAnalysis.CSharp.1.1.0\lib\net45\Microsoft.CodeAnalysis.CSharp.dll"
#r "bin\Debug\At.exe"

using System.IO;
using System.Linq;
using At;
using cs       = Microsoft.CodeAnalysis.CSharp;
using csSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;

using static cs.SyntaxFactory;
var x = At.AtSyntaxTree.ParseText("#import System; @ns1 : namespace {@f(); @variable : y; @y<>; @class<>  : y {@P<>;@G()}}");
var root = x.GetRoot();
var c = AtCompilation.Create(new[] {x});
var s = new MemoryStream();
var r = c.Emit(s);
var srcs = r.ConvertedSources().ToArray();
var src = srcs[0];

s.Seek(0,SeekOrigin.Begin);
var a  = System.Reflection.Assembly.Load(s.ToArray());
var ts = a.GetTypes();

var f = new FileStream("x.dll",FileMode.Create);
s.CopyTo(f);
f.Close();

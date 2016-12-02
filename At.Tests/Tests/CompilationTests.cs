﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace At.Tests
{
[TestClass]
public  class CompilationTests : AtTest
{
    //Compile-String-To-Assembly Test
    [TestMethod] 
    public void CompileStringToAssemblyTest()
    {
        var className1 = TestData.Identifier(0);
        var baseClass1 = TestData.Identifier(1);
        var className2 = TestData.Identifier(2);
        var variableName1 = TestData.Identifier(3);
        var variableName2 = TestData.Identifier(4);
        var className3 = TestData.Identifier(5);
        var functionName1 = TestData.Identifier(6);
        var ns = TestData.Identifier(7);

        var input = "#import System;\r\n"+
                    $"@{className1}< T , U > : {baseClass1}<{className2}, T>{{ \r\n @P<>; @G() }}\r\n"+ 
                    $"@{baseClass1}<T, U>;\r\n"+
                    $"@{className2}<>;"+
                    $"@{variableName1} : {className1}<{className2},{className2}>;"+
                    $"@{functionName1}()"+
                    $"@{ns} : namespace {{@{functionName1}(); @{@className1}<> }}"+
                    "@ns1 : namespace  {@f(); @variable : y; @y<>; @class<>  : y {@P<>;@G()}}"
                    ;
        Assembly output = null;
        
        try
        {
            output = AtProgram.compileStringToAssembly(input);
        }
        catch(CompilationException ex)
        {
            Assert.Fail($"\r\n{input}\r\n\r\n{ex}");
        }

        verifyOutput(output, className1+"`2", className2, baseClass1+"`2", "P");

        var class1 = output.GetType(className1+"`2");

        assert_not_null(()=>class1.GetNestedType("P"),ifFail:()=>class1.GetNestedTypes());
        assert_not_null(()=>class1.GetMethod("G"),ifFail:()=>class1.GetMethods());    

        var _ =  output.GetType(SyntaxTreeConverter.defaultClassName);
        var variable = _.GetField(variableName1);

        assert_not_null(()=>variable,ifFail:()=>_.GetFields());
        assert_not_null(()=>_.GetMethod(functionName1),ifFail:()=>_.GetMethods());
        assert_equals(()=>$"{className1}`2",()=>variable.FieldType.Name);
    }

    [TestMethod] 
    public void CompileStringTest1()
    {
        var input = "@A<B,C> : D<int,B> {}; @D<E,F>";

        Assembly output = null;
        
        try
        {
            output = AtProgram.compileStringToAssembly(input);
        }
        catch(CompilationException ex)
        {
            Assert.Fail($"\r\n{input}\r\n\r\n{ex}");
        }

        assert_not_null(()=>output);
    }
}
}

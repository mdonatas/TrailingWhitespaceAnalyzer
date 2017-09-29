using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace TrailingWhiteSpaceAnalyzer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        //No diagnostics expected to show up
        [TestMethod]
        public void NoDiagnosticsAppearOnEmptySource()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void TestWhitespaceDetectedAfterACurlyBrace()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "TrailingWhiteSpaceAnalyzer",
                Message = string.Format("Line '{0}' contains trailing whitespace", 7),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 7, 10)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        [TestMethod]
        public void TestWhitespaceDetectedAfterAStatement()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            Console.WriteLine("""");   
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "TrailingWhiteSpaceAnalyzer",
                Message = string.Format("Line '{0}' contains trailing whitespace", 8),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 8, 30)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            Console.WriteLine("""");
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        [TestMethod]
        public void TestWhitespaceDetectedAtAnEmptyLine()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {
   
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "TrailingWhiteSpaceAnalyzer",
                Message = string.Format("Line '{0}' contains trailing whitespace", 8),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 8, 30)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {

        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new TrailingWhiteSpaceAnalyzerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TrailingWhiteSpaceAnalyzer();
        }
    }
}
using System;
using System.IO;

using Kiota.Builder.CodeDOM;
using Kiota.Builder.Writers;
using Kiota.Builder.Writers.CSharp;

using Xunit;

namespace Kiota.Builder.Tests.Writers.CSharp {
    public class CodeClassDeclarationWriterTests : IDisposable
    {
        private const string DefaultPath = "./";
        private const string DefaultName = "name";
        private readonly StringWriter tw;
        private readonly LanguageWriter writer;
        private readonly CodeClassDeclarationWriter codeElementWriter;
        private readonly CodeClass parentClass;

        public CodeClassDeclarationWriterTests() {
            codeElementWriter = new CodeClassDeclarationWriter(new CSharpConventionService());
            writer = LanguageWriter.GetLanguageWriter(GenerationLanguage.CSharp, DefaultPath, DefaultName);
            tw = new StringWriter();
            writer.SetTextWriter(tw);
            var root = CodeNamespace.InitRootNamespace();
            parentClass = new () {
                Name = "parentClass"
            };
            root.AddClass(parentClass);
        }
        public void Dispose() {
            tw?.Dispose();
            GC.SuppressFinalize(this);
        }
        [Fact]
        public void WritesSimpleDeclaration() {
            codeElementWriter.WriteCodeElement(parentClass.StartBlock, writer);
            var result = tw.ToString();
            Assert.Contains("public class", result);
        }
        [Fact]
        public void WritesImplementation() {
            var declaration = parentClass.StartBlock;
            declaration.AddImplements(new CodeType {
                Name = "someInterface"
            });
            codeElementWriter.WriteCodeElement(declaration, writer);
            var result = tw.ToString();
            Assert.Contains(":", result);
        }
        [Fact]
        public void WritesInheritance() {
            var declaration = parentClass.StartBlock;
            declaration.Inherits = new (){
                Name = "someInterface"
            };
            codeElementWriter.WriteCodeElement(declaration, writer);
            var result = tw.ToString();
            Assert.Contains(":", result);
        }
        [Fact]
        public void WritesImports() {
            var declaration = parentClass.StartBlock;
            declaration.AddUsings(new () {
                Name = "Objects",
                Declaration = new() {
                    Name = "system.util",
                    IsExternal = true,
                }
            },
            new () {
                Name = "project.graph",
                Declaration = new() {
                    Name = "Message",
                }
            });
            codeElementWriter.WriteCodeElement(declaration, writer);
            var result = tw.ToString();
            Assert.Contains("using", result);
            Assert.Contains("Project.Graph", result);
            Assert.Contains("System.Util", result);
        }
    }
}

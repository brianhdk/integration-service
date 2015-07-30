using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Razor;
using Microsoft.CSharp;
using Vertica.Integration.Infrastructure.Templating.AttributeParsing;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration.Infrastructure.Templating
{
    public sealed class InMemoryRazorEngine
    {
        private static readonly Regex ModelTypeFinder = new Regex(@"(?:@model\s\w.*?\n)");

        public static string Execute(string razorTemplate)
        {
            return Execute<object>(razorTemplate, null);
        }

        public static string Execute<TModel>(string razorTemplate, TModel model, dynamic viewBag = null, params Assembly[] referenceAssemblies)
        {
            // removes the @model part of the razorTemplate
            razorTemplate = ModelTypeFinder.Replace(razorTemplate, String.Empty, 1);

            var host = new RazorEngineHost(new CSharpRazorCodeLanguage())
            {
                DefaultNamespace = "RazorOutput",
                DefaultClassName = "Template"
            };

            host.NamespaceImports.Add("System");
            host.DefaultBaseClass = typeof(RazorTemplateBase<TModel>).FullName;

            var engine = new RazorTemplateEngine(host);

            using (var template = new StringReader(razorTemplate))
            {
                GeneratorResults code = engine.GenerateCode(template);

                var parameters = new CompilerParameters { GenerateInMemory = true };

				parameters.ReferencedAssemblies.Add(typeof(InMemoryRazorEngine).Assembly.Location);
				parameters.ReferencedAssemblies.Add(typeof(IHtmlString).Assembly.Location);
                
                parameters.ReferencedAssemblies.AddRange(
                    referenceAssemblies.EmptyIfNull().Select(x => x.Location).ToArray());

                var codeProvider = new CSharpCodeProvider();
                CompilerResults compilerResult = codeProvider.CompileAssemblyFromDom(parameters, code.GeneratedCode);

                if (compilerResult.Errors.Count > 0)
                {
                    var compileErrors = new StringBuilder();

                    foreach (CompilerError compileError in compilerResult.Errors)
                    {
                        compileErrors.Append(
                            String.Format("Line: {0}\t Col: {1}\t Error: {2}\r\n",
                                compileError.Line,
                                compileError.Column,
                                compileError.ErrorText));
                    }

                    throw new InvalidOperationException(compileErrors.ToString());
                }

                Type templateType = compilerResult.CompiledAssembly.GetExportedTypes().Single();
                object compiledTemplate = Activator.CreateInstance(templateType);

                PropertyInfo modelProperty = templateType.GetProperty("Model");
                modelProperty.SetValue(compiledTemplate, model, null);

                PropertyInfo viewBagProperty = templateType.GetProperty("ViewBag");
                viewBagProperty.SetValue(compiledTemplate, viewBag, null);

                MethodInfo executeMethod = templateType.GetMethod("Execute");
                executeMethod.Invoke(compiledTemplate, null);

                PropertyInfo builderProperty = templateType.GetProperty("OutputBuilder");
                StringBuilder output = (StringBuilder)builderProperty.GetValue(compiledTemplate, null);

                return output.ToString();
            }
        }

        public abstract class RazorTemplateBase<TModel>
        {
            protected RazorTemplateBase()
            {
                OutputBuilder = new StringBuilder();
				Html = new HtmlHelper();
            }

            public TModel Model { get; set; }

            public dynamic ViewBag { get; set; }

            public StringBuilder OutputBuilder { get; private set; }

			public HtmlHelper Html { get; private set; }

            public abstract void Execute();

            public virtual void Write(object value)
            {
                OutputBuilder.Append(value);
            }

            public virtual void WriteLiteral(object value)
            {
                OutputBuilder.Append(value);
            }

			public virtual void WriteAttribute(string name, PositionTagged<string> prefix, PositionTagged<string> suffix, params AttributeValue[] values)
			{
				bool first = true;
				bool wroteSomething = false;
				if (values.Length == 0)
				{
					// Explicitly empty attribute, so write the prefix and suffix
					WritePositionTaggedLiteral(prefix);
					WritePositionTaggedLiteral(suffix);
				}
				else
				{
					foreach (AttributeValue attrVal in values)
					{
						PositionTagged<object> val = attrVal.Value;
						
						bool? boolVal = null;
						if (val.Value is bool)
						{
							boolVal = (bool)val.Value;
						}

						if (val.Value == null || (boolVal != null && !boolVal.Value)) 
							continue;

						var valStr = val.Value as string ?? val.Value.ToString();
							
						if (boolVal != null)
						{
							Debug.Assert(boolVal.Value);
							valStr = name;
						}
						if (first)
						{
							WritePositionTaggedLiteral(prefix);
							first = false;
						}
						else
						{
							WritePositionTaggedLiteral(attrVal.Prefix);
						}

						if (attrVal.Literal)
						{
							WriteLiteral(valStr);
						}
						else
						{
							Write(valStr);
						}

						wroteSomething = true;
					}

					if (wroteSomething)
						WritePositionTaggedLiteral(suffix);
				}
			}

			private void WritePositionTaggedLiteral(string value)
			{
				if (value == null)
					return;

				WriteLiteral(value);
			}

			private void WritePositionTaggedLiteral(PositionTagged<string> value)
			{
				WritePositionTaggedLiteral(value.Value);
			}
        }
    }
}
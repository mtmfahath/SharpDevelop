using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.NRefactoryResolver;

namespace ICSharpCode.SharpDevelop.Tests
{
	[TestFixture]
	public class GenericResolverTests
	{
		#region Test helper methods
		NRefactoryResolverTests nrrt = new NRefactoryResolverTests();
		
		ResolveResult Resolve(string program, string expression, int line)
		{
			return nrrt.Resolve(program, expression, line);
		}
		
		ResolveResult ResolveVB(string program, string expression, int line)
		{
			return nrrt.ResolveVB(program, expression, line);
		}
		#endregion
		
		const string listProgram = @"using System.Collections.Generic;
class TestClass {
	void Method() {
		List<TestClass> list = new List<TestClass>();
		
	}
	
	T CloneIt<T>(T source) where T : ICloneable {
		if (source == null) return new TestClass();
		return source.Clone();
	}
	
	public int PublicField;
}
";
		
		[Test]
		public void ListAddTest()
		{
			ResolveResult result = Resolve(listProgram, "list.Add(new A())", 5);
			Assert.IsNotNull(result);
			Assert.IsTrue(result is MemberResolveResult);
			IMethod m = (IMethod)((MemberResolveResult)result).ResolvedMember;
			Assert.AreEqual(1, m.Parameters.Count);
			Assert.AreEqual("TestClass", m.Parameters[0].ReturnType.FullyQualifiedName);
		}
		
		[Test]
		public void ListAddRangeTest()
		{
			ResolveResult result = Resolve(listProgram, "list.AddRange(new A[0])", 5);
			Assert.IsNotNull(result);
			Assert.IsTrue(result is MemberResolveResult);
			IMethod m = (IMethod)((MemberResolveResult)result).ResolvedMember;
			Assert.AreEqual(1, m.Parameters.Count);
			Assert.IsTrue(m.Parameters[0].ReturnType is SpecificReturnType);
			Assert.AreEqual("System.Collections.Generic.IEnumerable", m.Parameters[0].ReturnType.FullyQualifiedName);
			Assert.AreEqual("TestClass", ((SpecificReturnType)m.Parameters[0].ReturnType).TypeParameters[0].FullyQualifiedName);
		}
		
		[Test]
		public void ListToArrayTest()
		{
			ResolveResult result = Resolve(listProgram, "list.ToArray()", 5);
			Assert.IsNotNull(result);
			Assert.IsTrue(result is MemberResolveResult);
			IMethod m = (IMethod)((MemberResolveResult)result).ResolvedMember;
			Assert.AreEqual("TestClass", m.ReturnType.FullyQualifiedName);
			Assert.AreEqual(1, m.ReturnType.ArrayDimensions);
		}
		
		[Test]
		public void ClassReferenceTest()
		{
			ResolveResult result = Resolve(listProgram, "List<string>", 5);
			Assert.IsNotNull(result);
			Assert.IsTrue(result is TypeResolveResult);
			Assert.AreEqual("System.Collections.Generic.List", ((TypeResolveResult)result).ResolvedClass.FullyQualifiedName);
			Assert.IsTrue(result.ResolvedType is SpecificReturnType);
			Assert.AreEqual("System.String", ((SpecificReturnType)result.ResolvedType).TypeParameters[0].FullyQualifiedName);
		}
		
		[Test]
		public void GenericMethodCallTest()
		{
			ResolveResult result = Resolve(listProgram, "CloneIt<TestClass>(null)", 5);
			Assert.IsNotNull(result);
			Assert.IsTrue(result is MemberResolveResult);
			Assert.AreEqual("TestClass", result.ResolvedType.FullyQualifiedName);
			MemberResolveResult mrr = (MemberResolveResult) result;
			Assert.AreEqual("TestClass.CloneIt", mrr.ResolvedMember.FullyQualifiedName);
		}
		
		[Test]
		public void FieldReferenceOnGenericMethodTest()
		{
			ResolveResult result = Resolve(listProgram, "CloneIt<TestClass>(null).PublicField", 5);
			Assert.IsNotNull(result);
			Assert.IsTrue(result is MemberResolveResult);
			Assert.AreEqual("System.Int32", result.ResolvedType.FullyQualifiedName);
			MemberResolveResult mrr = (MemberResolveResult) result;
			Assert.AreEqual("TestClass.PublicField", mrr.ResolvedMember.FullyQualifiedName);
		}
	}
}

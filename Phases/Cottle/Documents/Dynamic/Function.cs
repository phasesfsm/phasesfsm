﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Cottle.Documents.Dynamic
{
	class Function : IFunction
	{
		#region Attributes

		private readonly Renderer renderer;

		private readonly Storage storage;

		#endregion

		#region Constructors

		public Function (IEnumerable<string> arguments, Command command, Trimmer trimmer)
		{
			Compiler compiler;
			DynamicMethod method;

			method = new DynamicMethod (string.Empty, typeof (Value), new [] {typeof (Storage), typeof (IList<Value>), typeof (IStore), typeof (TextWriter)}, this.GetType ());
			compiler = new Compiler (method.GetILGenerator (), trimmer);

			this.storage = compiler.Compile (arguments, command);
			this.renderer = (Renderer)method.CreateDelegate (typeof (Renderer));
		}

		#endregion

		#region Methods / Public

		public int CompareTo (IFunction other)
		{
			return object.ReferenceEquals (this, other) ? 0 : 1;
		}

		public bool Equals (IFunction other)
		{
			return this.CompareTo (other) == 0;
		}

		public override bool Equals (object obj)
		{
			IFunction other = obj as IFunction;

			return other != null && this.Equals (other);
		}

		public Value Execute (IList<Value> arguments, IStore store, TextWriter output)
		{
			return this.renderer (this.storage, arguments, store, output);
		}

		public override int GetHashCode ()
		{
			return this.renderer.GetHashCode ();
		}

		public override string ToString ()
		{
			return "<dynamic>";
		}

		#endregion

		#region Methods / Static

		public static void Save (Command command, Trimmer trimmer, string assemblyName, string fileName)
		{
			AssemblyBuilder assembly;
			Compiler compiler;
			MethodBuilder method;
			ModuleBuilder module;
			TypeBuilder program;

#if NETSTANDARD2_0
			assembly = AssemblyBuilder.DefineDynamicAssembly (new AssemblyName (assemblyName), AssemblyBuilderAccess.Run);
			module = assembly.DefineDynamicModule (fileName);
#else
			assembly = AppDomain.CurrentDomain.DefineDynamicAssembly (new AssemblyName (assemblyName), AssemblyBuilderAccess.RunAndSave);
			module = assembly.DefineDynamicModule (assemblyName, fileName);
#endif

			program = module.DefineType ("Program", TypeAttributes.Public);
			method = program.DefineMethod ("Main", MethodAttributes.Public | MethodAttributes.Static, typeof (Value), new [] {typeof (Storage), typeof (IList<Value>), typeof (IStore), typeof (TextWriter)});

			compiler = new Compiler (method.GetILGenerator (), trimmer);
			compiler.Compile (Enumerable.Empty<string> (), command);

#if NETSTANDARD2_0
			program.CreateTypeInfo ();
#else
			program.CreateType ();
			assembly.Save (fileName);
#endif
        }

		#endregion
	}
}

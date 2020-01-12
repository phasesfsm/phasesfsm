﻿using System;
using System.Collections.Generic;
using System.IO;
using Cottle.Values;

namespace Cottle.Functions
{
	public sealed class NativeFunction : IFunction
	{
		#region Properties

		public bool Pure
		{
			get;
			private set;
		}

		#endregion

		#region Attributes

		private readonly Func<IList<Value>, IStore, TextWriter, Value> callback;

		private readonly int max;

		private readonly int min;

		#endregion

		#region Constructors / Public

		public NativeFunction (Func<IList<Value>, IStore, TextWriter, Value> callback, int min, int max) :
			this (callback, min, max, false)
		{
		}

		public NativeFunction (Func<IList<Value>, IStore, TextWriter, Value> callback, int exact) :
			this (callback, exact, exact, false)
		{
		}

		public NativeFunction (Func<IList<Value>, IStore, TextWriter, Value> callback) :
			this (callback, 0, -1, false)
		{
		}

		public NativeFunction (Func<IList<Value>, IStore, Value> callback, int min, int max) :
			this ((v, s, o) => callback (v, s), min, max, false)
		{
		}

		public NativeFunction (Func<IList<Value>, IStore, Value> callback, int exact) :
			this ((v, s, o) => callback (v, s), exact, exact, false)
		{
		}

		public NativeFunction (Func<IList<Value>, IStore, Value> callback) :
			this ((v, s, o) => callback (v, s), 0, -1, false)
		{
		}

		public NativeFunction (Func<IList<Value>, Value> callback, int min, int max) :
			this ((v, s, o) => callback (v), min, max, true)
		{
		}

		public NativeFunction (Func<IList<Value>, Value> callback, int exact) :
			this ((v, s, o) => callback (v), exact, exact, true)
		{
		}

		public NativeFunction (Func<IList<Value>, Value> callback) :
			this ((v, s, o) => callback (v), 0, -1, true)
		{
		}

		#endregion

		#region Constructors / Private

		private NativeFunction (Func<IList<Value>, IStore, TextWriter, Value> callback, int min, int max, bool pure)
		{
			if (callback == null)
				throw new ArgumentNullException ("callback");

			this.callback = callback;
			this.max = max;
			this.min = min;
			this.Pure = pure;
		}

		#endregion

		#region Methods

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
			if (this.min > arguments.Count || (this.max >= 0 && this.max < arguments.Count))
				return VoidValue.Instance;

			return this.callback (arguments, store, output);
		}

		public override int GetHashCode ()
		{
			unchecked
			{
				return
					(this.callback.GetHashCode () &	(int)0xFFFFFF00) |
					(this.max.GetHashCode () &		(int)0x000000F0) |
					(this.min.GetHashCode () & 		(int)0x0000000F);
			}
		}

		public override string ToString ()
		{
			return "native";
		}

		#endregion
	}
}

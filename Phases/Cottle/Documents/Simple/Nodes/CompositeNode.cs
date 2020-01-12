﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cottle.Values;

namespace Cottle.Documents.Simple.Nodes
{
	class CompositeNode : INode
	{
		#region Attributes

		private readonly INode[] nodes;

		#endregion

		#region Constructors

		public CompositeNode (IEnumerable<INode> nodes)
		{
			this.nodes = nodes.ToArray ();
		}

		#endregion

		#region Methods

		public bool Render (IStore store, TextWriter output, out Value result)
		{
			foreach (INode node in this.nodes)
			{
				if (node.Render (store, output, out result))
					return true;
			}

			result = VoidValue.Instance;

			return false;
		}

		public void Source (ISetting setting, TextWriter output)
		{
			foreach (INode node in this.nodes)
				node.Source (setting, output);
		}

		#endregion
	}
}

﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Cottle.Documents.Simple;
using Cottle.Documents.Simple.Evaluators;
using Cottle.Documents.Simple.Nodes;
using Cottle.Documents.Simple.Nodes.AssignNodes;
using Cottle.Settings;

namespace Cottle.Documents
{
	/// <summary>
	/// Simple document renders templates using an interpreter. If offers
	/// better garbage collection and easier debugging but average rendering
	/// performance.
	/// </summary>
	public sealed class SimpleDocument : AbstractDocument
	{
		#region Attributes

		private readonly INode renderer;

		private readonly ISetting setting;

		#endregion

		#region Constructors

		public SimpleDocument (TextReader reader, ISetting setting)
		{
			IParser parser = ParserFactory.BuildParser (setting);
			Command root = parser.Parse (reader);

			this.renderer = this.CompileCommand (root, setting.Trimmer);
			this.setting = setting;
		}

		public SimpleDocument (TextReader reader) :
			this (reader, DefaultSetting.Instance)
		{
		}

		public SimpleDocument (string template, ISetting setting) :
			this (new StringReader (template), setting)
		{
		}

		public SimpleDocument (string template) :
			this (new StringReader (template), DefaultSetting.Instance)
		{
		}

		#endregion

		#region Methods / Public

		public override Value Render (IStore store, TextWriter writer)
		{
			Value result;

			store.Enter ();

			this.renderer.Render (store, writer, out result);

			store.Leave ();

			return result;
		}

		public void Source (TextWriter writer)
		{
			this.renderer.Source (this.setting, writer);
		}

		public string Source ()
		{
			StringWriter writer;

			writer = new StringWriter (CultureInfo.InvariantCulture);

			this.Source (writer);

			return writer.ToString ();
		}

		#endregion

		#region Methods / Private

		private INode CompileCommand (Command command, Trimmer trimmer)
		{
			List<KeyValuePair<IEvaluator, INode>> branches;
			List<INode> nodes;

			switch (command.Type)
			{
				case CommandType.AssignFunction:
					return new FunctionAssignNode (command.Name, command.Arguments, this.CompileCommand (command.Body, trimmer), command.Mode);

				case CommandType.AssignRender:
					return new RenderAssignNode (command.Name, this.CompileCommand (command.Body, trimmer), command.Mode);

				case CommandType.AssignValue:
					return new ValueAssignNode (command.Name, this.CompileExpression (command.Operand), command.Mode);

				case CommandType.Composite:
					nodes = new List<INode> ();

					for (; command.Type == CommandType.Composite; command = command.Next)
						nodes.Add (this.CompileCommand (command.Body, trimmer));

					nodes.Add (this.CompileCommand (command, trimmer));

					return new CompositeNode (nodes);

				case CommandType.Dump:
					return new DumpNode (this.CompileExpression (command.Operand));

				case CommandType.Echo:
					return new EchoNode (this.CompileExpression (command.Operand));

				case CommandType.For:
					return new ForNode (this.CompileExpression (command.Operand), command.Key, command.Name, this.CompileCommand (command.Body, trimmer), command.Next != null ? this.CompileCommand (command.Next, trimmer) : null);

				case CommandType.If:
					branches = new List<KeyValuePair<IEvaluator, INode>> ();

					for (; command != null && command.Type == CommandType.If; command = command.Next)
						branches.Add (new KeyValuePair<IEvaluator, INode> (this.CompileExpression (command.Operand), this.CompileCommand (command.Body, trimmer)));

					return new IfNode (branches, command != null ? this.CompileCommand (command, trimmer) : null);

				case CommandType.Literal:
					return new LiteralNode (trimmer (command.Text));

				case CommandType.Return:
					return new ReturnNode (this.CompileExpression (command.Operand));

				case CommandType.While:
					return new WhileNode (this.CompileExpression (command.Operand), this.CompileCommand (command.Body, trimmer));

				default:
					return new LiteralNode (string.Empty);
			}
		}

		private IEvaluator CompileExpression (Expression expression)
		{
			IEvaluator[] arguments;
			KeyValuePair<IEvaluator, IEvaluator>[] elements;
			IEvaluator key;
			IEvaluator value;

			switch (expression.Type)
			{
				case ExpressionType.Access:
					return new AccessEvaluator (this.CompileExpression (expression.Source), this.CompileExpression (expression.Subscript));

				case ExpressionType.Constant:
					return new ConstantEvaluator (expression.Value);

				case ExpressionType.Invoke:
					arguments = new IEvaluator[expression.Arguments.Length];

					for (int i = 0; i < arguments.Length; ++i)
						arguments[i] = this.CompileExpression (expression.Arguments[i]);

					return new InvokeEvaluator (this.CompileExpression (expression.Source), arguments);					

				case ExpressionType.Map:
					elements = new KeyValuePair<IEvaluator, IEvaluator>[expression.Elements.Length];

					for (int i = 0; i < elements.Length; ++i)
					{
						key = this.CompileExpression (expression.Elements[i].Key);
						value = this.CompileExpression (expression.Elements[i].Value);

						elements[i] = new KeyValuePair<IEvaluator, IEvaluator> (key, value);
					}

					return new MapEvaluator (elements);

				case ExpressionType.Symbol:
					return new SymbolEvaluator (expression.Value);

				default:
					return VoidEvaluator.Instance;
			}
		}

		#endregion
	}
}

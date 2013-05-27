#region License
// Copyright (c) 2009 Sander van Rossen
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Graph
{
	public abstract class NodeConnector : IElement
	{
		public NodeConnector(NodeItem item, bool enabled) { Item = item; Enabled = enabled; }
		public Node				Node		{ get { return Item.Node; } }
		public NodeItem			Item		{ get; private set; }
		public bool				Enabled		{ get; internal set; }
		public bool				Highlight	{ get; internal set; }

		internal PointF			Center		{ get { return new PointF((bounds.Left + bounds.Right) / 2.0f, (bounds.Top + bounds.Bottom) / 2.0f); } }
		internal RectangleF		bounds;
		internal RenderState	state;

		public abstract ElementType ElementType { get; }
	}

	public sealed class NodeInputConnector : NodeConnector
	{
		public NodeInputConnector(NodeItem item, bool enabled) : base(item, enabled) { }
		public override ElementType ElementType { get { return ElementType.InputConnector; } }
	}

	public sealed class NodeOutputConnector : NodeConnector
	{
		public NodeOutputConnector(NodeItem item, bool enabled) : base(item, enabled) { }
		public override ElementType ElementType { get { return ElementType.OutputConnector; } }
	}
}

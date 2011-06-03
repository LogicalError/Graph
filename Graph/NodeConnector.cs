using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Graph
{
	public class NodeConnector 
	{
		public NodeConnector(NodeItem item, bool enabled) { Item = item; Enabled = enabled; }
		public Node				Node		{ get { return Item.Node; } }
		public NodeItem			Item		{ get; private set; }
		public bool				Enabled		{ get; internal set; }

		internal RectangleF		bounds;
		internal RenderState	state;
	}
}

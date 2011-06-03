using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace Graph
{
	public class NodeConnectionEventArgs : EventArgs
	{
		public NodeConnectionEventArgs(NodeConnection connection) { Connection = connection; }
		public NodeConnection Connection { get; private set; }
	}

	public class AcceptNodeConnectionEventArgs : CancelEventArgs
	{
		public AcceptNodeConnectionEventArgs(NodeConnection connection) { Connection = connection; }
		public AcceptNodeConnectionEventArgs(NodeConnection connection, bool cancel) : base(cancel) { Connection = connection; }
		public NodeConnection Connection { get; private set; }
	}

	public class NodeConnection
	{
		public event EventHandler<NodeConnectionEventArgs>	DoubleClick;

		public NodeConnector	From	{ get; set; }
		public NodeConnector	To		{ get; set; }
		public string			Name	{ get; set; }
		public object			Tag		{ get; set; }
		
		internal RenderState	state;
		internal RectangleF		bounds;
		internal RectangleF		textBounds;


		internal void			DoDoubleClick() { if (DoubleClick != null) DoubleClick(this, new NodeConnectionEventArgs(this)); }
	}
}

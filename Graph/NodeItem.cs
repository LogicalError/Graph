using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel.Composition;

namespace Graph
{
	public class NodeItemEventArgs : EventArgs
	{
		public NodeItemEventArgs(NodeItem item) { Item = item; }
		public NodeItem Item { get; private set; }
	}

	public abstract class NodeItem
	{
		public NodeItem()
		{
			this.Input		= new NodeConnector(this, false);
			this.Output		= new NodeConnector(this, false);
		}

		public NodeItem(bool enableInput, bool enableOutput)
		{
			this.Input		= new NodeConnector(this, enableInput);
			this.Output		= new NodeConnector(this, enableOutput);
		}

		public Node					Node			{ get; internal set; }
		public object				Tag				{ get; set; }

		public NodeConnector		Input			{ get; private set; }
		public NodeConnector		Output			{ get; private set; }

		internal RectangleF			bounds;

		public virtual bool			OnClick()		{ return false; }
		public virtual bool			OnEnter()		{ return false; }
		public virtual bool			OnLeave()		{ return false; }
		public virtual bool			OnStartDrag(PointF location) { return false; }
		public virtual bool			OnDrag(PointF location)		 { return false; }		
		public virtual bool			OnEndDrag() 				 { return false; }
	}
}

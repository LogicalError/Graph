using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using Graph.Items;

namespace Graph
{
	public class NodeEventArgs : EventArgs
	{
		public NodeEventArgs(Node node) { Node = node; }
		public Node Node { get; private set; }
	}

	public class AcceptNodeEventArgs : CancelEventArgs
	{
		public AcceptNodeEventArgs(Node node) { Node = node; }
		public AcceptNodeEventArgs(Node node, bool cancel) : base(cancel) { Node = node; }
		public Node Node { get; private set; }
	}

	public class Node
	{
		public string			Title			{ get { return titleItem.Title; } set { titleItem.Title = value; } }

		#region Collapsed
		internal bool			internalCollapsed;
		public bool				Collapsed		
		{ 
			get 
			{
				return (internalCollapsed && 
						((state & RenderState.Dragging) == 0)) ||
						nodeItems.Count == 0;
			} 
			set 
			{
				var oldValue = Collapsed;
				internalCollapsed = value;
				if (Collapsed != oldValue)
					titleItem.ForceResize();
			} 
		}
		#endregion

		public bool				HasNoItems		{ get { return nodeItems.Count == 0; } }

		public PointF			Location		{ get; set; }
		public object			Tag				{ get; set; }

		public IEnumerable<NodeConnection>	Connections { get { return connections; } }
		public IEnumerable<NodeItem>		Items		{ get { return nodeItems; } }
		
		internal RectangleF		bounds;
		internal RectangleF		inputBounds;
		internal RectangleF		outputBounds;
		internal RectangleF		itemsBounds;
		internal RenderState	state			= RenderState.None;
		internal RenderState	inputState		= RenderState.None;
		internal RenderState	outputState		= RenderState.None;

		internal readonly List<NodeConnector>	inputConnectors		= new List<NodeConnector>();
		internal readonly List<NodeConnector>	outputConnectors	= new List<NodeConnector>();
		internal readonly List<NodeConnection>	connections			= new List<NodeConnection>();
		internal readonly NodeTitleItem			titleItem			= new NodeTitleItem();
		readonly List<NodeItem>					nodeItems			= new List<NodeItem>();

		public Node(string title)
		{
			this.Title = title;
			titleItem.Node = this;
		}

		public void AddItem(NodeItem item)
		{
			if (nodeItems.Contains(item))
				return;
			if (item.Node != null)
				item.Node.RemoveItem(item);
			nodeItems.Add(item);
			item.Node = this;
		}

		public void RemoveItem(NodeItem item)
		{
			if (!nodeItems.Contains(item))
				return;
			item.Node = null;
			nodeItems.Remove(item);
		}
	}
}

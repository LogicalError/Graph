﻿#region License
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Graph
{
	public partial class GraphControl : Control
	{
		#region Constructor
		public GraphControl()
		{
			InitializeComponent();
			this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.UserPaint, true);
		}
		#endregion

		public event EventHandler<AcceptNodeEventArgs>				NodeAdded;
		public event EventHandler<AcceptNodeEventArgs>				NodeRemoved;
		public event EventHandler<AcceptNodeConnectionEventArgs>	ConnectionAdded;
		public event EventHandler<AcceptNodeConnectionEventArgs>	ConnectionRemoved;


		#region DragElement
		IElement internalDragElement;
		IElement DragElement
		{
			get { return internalDragElement; }
			set
			{
				if (internalDragElement == value)
					return;
				if (internalDragElement != null)
					SetFlag(internalDragElement, RenderState.Dragging, false, false);
				internalDragElement = value;
				if (internalDragElement != null)
					SetFlag(internalDragElement, RenderState.Dragging, true, false);
			}
		}
		#endregion
		
		#region HoverElement
		IElement internalHoverElement;
		IElement HoverElement
		{
			get { return internalHoverElement; }
			set
			{
				if (internalHoverElement == value)
					return;
				if (internalHoverElement != null)
					SetFlag(internalHoverElement, RenderState.Hover, false, true);
				internalHoverElement = value;
				if (internalHoverElement != null)
					SetFlag(internalHoverElement, RenderState.Hover, true, true);
			}
		}
		#endregion
		
		#region FocusElement
		IElement internalFocusElement;
		public IElement FocusElement
		{
			get { return internalFocusElement; }
			set
			{
				if (internalFocusElement == value)
					return;
				if (internalFocusElement != null)
					SetFlag(internalFocusElement, RenderState.Focus, false, false);
				internalFocusElement = value;
				if (internalFocusElement != null)
					SetFlag(internalFocusElement, RenderState.Focus, true, false);
			}
		}
		#endregion


		#region SetFlag
		RenderState SetFlag(RenderState original, RenderState flag, bool value)
		{
			if (value)
				return original | flag;
			else
				return original & ~flag;
		}
		void SetFlag(IElement element, RenderState flag, bool value)
		{
			if (element == null)
				return;

			switch (element.ElementType)
			{
				case ElementType.Node:
					var node = element as Node;
					node.state = SetFlag(node.state, flag, value);
					SetFlag(node.titleItem, flag, value);
					break;

				case ElementType.InputConnector:
				case ElementType.OutputConnector:
					var connector = element as NodeConnector;
					connector.state = SetFlag(connector.state, flag, value);
					break;

				case ElementType.Connection:
					var connection = element as NodeConnection;
					connection.state = SetFlag(connection.state, flag, value);
					break;

				case ElementType.NodeItem:
					var item = element as NodeItem;
					item.state = SetFlag(item.state, flag, value);
					break;
			}
		}
		void SetFlag(IElement element, RenderState flag, bool value, bool setConnections)
		{
			if (element == null)
				return;

			switch (element.ElementType)
			{
				case ElementType.Node:
					var node = element as Node;
					node.state = SetFlag(node.state, flag, value);
					SetFlag(node.titleItem, flag, value);
					break;

				case ElementType.InputConnector:
				case ElementType.OutputConnector:
					var connector = element as NodeConnector;
					connector.state = SetFlag(connector.state, flag, value);
					SetFlag(connector.Node, flag, value, setConnections);
					break;

				case ElementType.Connection:
					var connection = element as NodeConnection;
					connection.state = SetFlag(connection.state, flag, value);
					if (setConnections)
					{
						if (connection.From != null)
							connection.From.state = SetFlag(connection.From.state, flag, value);
						if (connection.To != null)
							connection.To.state = SetFlag(connection.To.state, flag, value);
						//SetFlag(connection.From, flag, value, setConnections);
						//SetFlag(connection.To, flag, value, setConnections);
					}
					break;

				case ElementType.NodeItem:
					var item = element as NodeItem;
					item.state = SetFlag(item.state, flag, value);
					SetFlag(item.Node, flag, value, setConnections);
					break;
			}
		}
		#endregion

		#region BringElementToFront
		public void BringElementToFront(IElement element)
		{
			if (element == null)
				return;
			switch (element.ElementType)
			{
				case ElementType.Connection:
					var connection = element as NodeConnection;
					BringElementToFront(connection.From);
					BringElementToFront(connection.To);
					
					var connections = connection.From.Node.connections;
					if (connections[0] != connection)
					{
						connections.Remove(connection);
						connections.Insert(0, connection);
					}
					
					connections = connection.To.Node.connections;
					if (connections[0] != connection)
					{
						connections.Remove(connection);
						connections.Insert(0, connection);
					}
					break;
				case ElementType.Node:
					var node = element as Node;
					if (graphNodes[0] != node)
					{
						graphNodes.Remove(node);
						graphNodes.Insert(0, node);
					}
					break;
				case ElementType.InputConnector:
				case ElementType.OutputConnector:
					var connector = element as NodeConnector;
					BringElementToFront(connector.Node);
					break;
				case ElementType.NodeItem:
					var item = element as NodeItem;
					BringElementToFront(item.Node);
					break;
			}
		}
		#endregion
		
		#region HasFocus
		bool HasFocus(IElement element)
		{
			if (element == null)
				return FocusElement == null;

			if (FocusElement == null)
				return false;

			if (element.ElementType ==
				FocusElement.ElementType)
				return (element == FocusElement);
			
			switch (FocusElement.ElementType)
			{
				case ElementType.Connection:
					var focusConnection = FocusElement as NodeConnection;
					return (focusConnection.To == element ||
							focusConnection.From == element ||
							
							((focusConnection.To != null &&
							focusConnection.To.Node == element) ||
							(focusConnection.From != null &&
							focusConnection.From.Node == element)));
				case ElementType.NodeItem:
					var focusItem = FocusElement as NodeItem;
					return (focusItem.Node == element);
				case ElementType.InputConnector:
				case ElementType.OutputConnector:
					var focusConnector = FocusElement as NodeConnector;
					return (focusConnector.Node == element);
				default:
				case ElementType.Node:
					return false;
			}
		}
		#endregion


		#region ShowLabels
		bool internalShowLabels = false;
		public bool ShowLabels 
		{ 
			get 
			{
				return internalShowLabels;
			} 
			set
			{
				if (internalShowLabels == value)
					return;
				internalShowLabels = value;
				this.Invalidate();
			}
		}
		#endregion


		#region Nodes
		readonly List<Node> graphNodes = new List<Node>();
		public IEnumerable<Node> Nodes { get { return graphNodes; } }
		#endregion


		IElement				internalDragOverElement;
		bool					mouseMoved	= false;
		bool					dragging	= false;

		Point					lastLocation;
		PointF					snappedLocation;
		
		PointF					translation = new PointF();
		float					zoom = 1.0f;


		#region UpdateMatrices
		readonly Matrix			transformation = new Matrix();
		readonly Matrix			inverse_transformation = new Matrix();
		void UpdateMatrices()
		{
			if (zoom < 0.25f) zoom = 0.25f;
			if (zoom > 5.00f) zoom = 5.00f;
			var center = new PointF(this.Width / 2.0f, this.Height / 2.0f);
			transformation.Reset();
			transformation.Translate(translation.X, translation.Y);
			transformation.Translate(center.X, center.Y);
			transformation.Scale(zoom, zoom);
			transformation.Translate(-center.X, -center.Y);

			inverse_transformation.Reset();
			inverse_transformation.Translate(center.X, center.Y);
			inverse_transformation.Scale(1.0f / zoom, 1.0f / zoom);
			inverse_transformation.Translate(-center.X, -center.Y);
			inverse_transformation.Translate(-translation.X, -translation.Y);
		}
		#endregion



		#region AddNode
		public bool AddNode(Node node)
		{
			if (node == null ||
				graphNodes.Contains(node))
				return false;

			graphNodes.Insert(0, node);			
			if (NodeAdded != null)
			{
				var eventArgs = new AcceptNodeEventArgs(node);
				NodeAdded(this, eventArgs);
				if (eventArgs.Cancel)
				{
					graphNodes.Remove(node);
					return false;
				}
			}

			BringElementToFront(node);
			FocusElement = node;
			this.Invalidate();
			return true;
		}
		#endregion

		#region AddNodes
		public bool AddNodes(IEnumerable<Node> nodes)
		{
			if (nodes == null)
				return false;

			int		index		= 0;
			bool	modified	= false;
			Node	lastNode	= null;
			foreach (var node in nodes)
			{
				if (node == null)
					continue;
				if (graphNodes.Contains(node))
					continue;

				graphNodes.Insert(index, node); index++;

				if (NodeAdded != null)
				{
					var eventArgs = new AcceptNodeEventArgs(node);
					NodeAdded(this, eventArgs);
					if (eventArgs.Cancel)
					{
						graphNodes.Remove(node);
						modified = true;
					} else
						lastNode = node;
				} else
					lastNode = node;
			}
			if (lastNode != null)
			{
				BringElementToFront(lastNode);
				FocusElement = lastNode;
			}
			return modified;
		}
		#endregion

		#region RemoveNode
		public void RemoveNode(Node node)
		{
			if (node == null)
				return;

			if (NodeRemoved != null)
			{
				var eventArgs = new AcceptNodeEventArgs(node);
				NodeRemoved(this, eventArgs);
				if (eventArgs.Cancel)
					return;
			}
			if (HasFocus(node))
				FocusElement = null;

			DisconnectAll(node);
			graphNodes.Remove(node);
			this.Invalidate();
		}
		#endregion

		#region RemoveNodes
		public bool RemoveNodes(IEnumerable<Node> nodes)
		{
			if (nodes == null)
				return false;

			bool modified = false;
			foreach (var node in nodes)
			{
				if (node == null)
					continue;
				if (NodeRemoved != null)
				{
					var eventArgs = new AcceptNodeEventArgs(node);
					NodeRemoved(this, eventArgs);
					if (eventArgs.Cancel)
						continue;
				}

				if (HasFocus(node))
					FocusElement = null;

				DisconnectAll(node);
				graphNodes.Remove(node);
				modified = true;
			}
			return modified;
		}
		#endregion

		#region Connect
		public NodeConnection Connect(NodeItem from, NodeItem to)
		{
			return Connect(from.Output, to.Input);
		}

		public NodeConnection Connect(NodeConnector from, NodeConnector to)
		{
			if (from      == null || to      == null ||
				from.Node == null || to.Node == null ||
				!from.Enabled || 
				!to.Enabled)
				return null;

			foreach (var other in from.Node.connections)
			{
				if (other.From == from &&
					other.To == to)
					return null;
			}

			foreach (var other in to.Node.connections)
			{
				if (other.From == from &&
					other.To == to)
					return null;
			}

			var connection = new NodeConnection();
			connection.From = from;
			connection.To = to;

			from.Node.connections.Add(connection);
			to.Node.connections.Add(connection);
			
			if (ConnectionAdded != null)
			{
				var eventArgs = new AcceptNodeConnectionEventArgs(connection);
				ConnectionAdded(this, eventArgs);
				if (eventArgs.Cancel)
				{
					Disconnect(connection);
					return null;
				}
			}

			return connection;
		}
		#endregion

		#region Disconnect
		public bool Disconnect(NodeConnection connection)
		{
			if (connection == null)
				return false;

			if (ConnectionRemoved != null)
			{
				var eventArgs = new AcceptNodeConnectionEventArgs(connection);
				ConnectionRemoved(this, eventArgs);
				if (eventArgs.Cancel)
					return false;
			}

			if (HasFocus(connection))
				FocusElement = null;
			
			var from	= connection.From;
			var to		= connection.To;
			if (from != null && from.Node != null)
				from.Node.connections.Remove(connection);
			if (to != null && to.Node != null)
				to.Node.connections.Remove(connection);

			// Just in case somebody stored it somewhere ..
			connection.From = null;
			connection.To = null;
			return true;
		}
		#endregion

		#region DisconnectAll (private)
		bool DisconnectAll(Node node)
		{
			bool modified = false;
			var connections = node.connections.ToList();
			foreach (var connection in connections)
				modified = Disconnect(connection) ||
					modified;
			return modified;
		}
		#endregion


		#region FindNodeItemAt
		static NodeItem FindNodeItemAt(Node node, PointF location)
		{
			if (node.itemsBounds == null ||
				location.X < node.itemsBounds.Left ||
				location.X > node.itemsBounds.Right)
				return null;

			foreach (var item in node.Items)
			{
				if (item.bounds.IsEmpty)
					continue;

				if (location.Y < item.bounds.Top)
					break;

				if (location.Y < item.bounds.Bottom)
					return item;
			}
			return null;
		}
		#endregion

		#region FindInputConnectorAt
		static NodeConnector FindInputConnectorAt(Node node, PointF location)
		{
			if (node.itemsBounds == null || node.Collapsed)
				return null;

			foreach (var inputConnector in node.inputConnectors)
			{
				if (inputConnector.bounds.IsEmpty)
					continue;

				if (inputConnector.bounds.Contains(location))
					return inputConnector;
			}
			return null;
		}
		#endregion

		#region FindOutputConnectorAt
		static NodeConnector FindOutputConnectorAt(Node node, PointF location)
		{
			if (node.itemsBounds == null || node.Collapsed)
				return null;

			foreach (var outputConnector in node.outputConnectors)
			{
				if (outputConnector.bounds.IsEmpty)
					continue;

				if (outputConnector.bounds.Contains(location))
					return outputConnector;
			}
			return null;
		}
		#endregion

		#region FindElementAt
		IElement FindElementAt(PointF location)
		{
			foreach (var node in graphNodes)
			{
				var inputConnector = FindInputConnectorAt(node, location);
				if (inputConnector != null)
					return inputConnector;

				var outputConnector = FindOutputConnectorAt(node, location);
				if (outputConnector != null)
					return outputConnector;

				if (node.bounds.Contains(location))
				{
					var item = FindNodeItemAt(node, location);
					if (item != null)
						return item;
					return node;
				}
			}

			var skipConnections		= new HashSet<NodeConnection>();
			var foundConnections	= new List<NodeConnection>();
			foreach (var node in graphNodes)
			{
				foreach (var connection in node.connections)
				{
					if (skipConnections.Add(connection)) // if we can add it, we haven't checked it yet
					{
						if (connection.bounds.Contains(location))
							foundConnections.Insert(0, connection);
					}
				}
			}
			foreach (var connection in foundConnections)
			{
				if (connection.textBounds.Contains(location))
					return connection;
			}
			foreach (var connection in foundConnections)
			{
				using (var region = GraphRenderer.GetConnectionRegion(connection))
				{
					if (region.IsVisible(location))
						return connection;
				}
			}

			return null;
		}
		#endregion


		#region OnPaint
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (this.graphNodes.Count == 0)
				return;
			if (e.Graphics == null)
				return;

			e.Graphics.Clear(Color.White);

			UpdateMatrices();
			e.Graphics.PageUnit = GraphicsUnit.Pixel;
			e.Graphics.CompositingQuality	= CompositingQuality.GammaCorrected;
			e.Graphics.InterpolationMode	= InterpolationMode.HighQualityBicubic;
			e.Graphics.SmoothingMode		= SmoothingMode.HighQuality;
			e.Graphics.TextRenderingHint	= TextRenderingHint.ClearTypeGridFit;
			e.Graphics.PixelOffsetMode		= PixelOffsetMode.HighQuality;
			
			
			e.Graphics.Transform			= transformation;

			GraphRenderer.Render(e.Graphics, graphNodes, ShowLabels);
			if (dragging)
			{
				var points = new PointF[] { snappedLocation };
				inverse_transformation.TransformPoints(points);
				var transformed_location = points[0];

				if (DragElement != null)
				{
					switch (DragElement.ElementType)
					{
						case ElementType.OutputConnector:
							var outputConnector = DragElement as NodeConnector;
							GraphRenderer.RenderOutputConnection(e.Graphics, outputConnector, 
								transformed_location.X, transformed_location.Y, RenderState.Dragging | RenderState.Hover);
							break;
						case ElementType.InputConnector:
							var inputConnector = DragElement as NodeConnector;
							GraphRenderer.RenderInputConnection(e.Graphics, inputConnector, 
								transformed_location.X, transformed_location.Y, RenderState.Dragging | RenderState.Hover);
							break;
					}
				}
			}
		}
		#endregion



		#region OnMouseWheel
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);

			zoom *= (float)Math.Pow(2, e.Delta / 480.0f);

			this.Refresh();
		}
		#endregion

		#region OnMouseDown
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			dragging	= true;
			mouseMoved	= false;
			snappedLocation = lastLocation = e.Location;
			
			var points = new Point[] { e.Location };
			inverse_transformation.TransformPoints(points);
			var transformed_location = points[0];

			var element = FindElementAt(transformed_location);
			if (element != null)
			{
				var item = element as NodeItem;
				if (item != null &&
					!item.OnStartDrag(transformed_location))
					element = item.Node;
				FocusElement =
				DragElement = element;
				BringElementToFront(element);
				this.Refresh();
			}
		}
		#endregion

		#region OnMouseMove
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			var deltaX = (lastLocation.X - e.Location.X) / zoom;
			var deltaY = (lastLocation.Y - e.Location.Y) / zoom;

			var points = new Point[] { e.Location };
			inverse_transformation.TransformPoints(points);
			var transformed_location = points[0];

			bool needRedraw = false;

			if (dragging)
			{
				if (!mouseMoved)
				{
					if ((Math.Abs(deltaX) > 1) ||
						(Math.Abs(deltaY) > 1))
						mouseMoved = true;
				}

				if (mouseMoved &&
					(Math.Abs(deltaX) > 0) ||
					(Math.Abs(deltaY) > 0))
				{
					mouseMoved = true;
					if (DragElement == null)
					{ 
						// translate view
						translation.X -= deltaX;
						translation.Y -= deltaY;
						snappedLocation = lastLocation = e.Location;
						this.Refresh();
						return;
					} else
					{
						BringElementToFront(DragElement); 
						switch (DragElement.ElementType)
						{
							case ElementType.Node:				// drag nodes
							{
								var node = DragElement as Node;
								node.Location	= new Point((int)Math.Round(node.Location.X - deltaX),
															(int)Math.Round(node.Location.Y - deltaY));
								snappedLocation = lastLocation = e.Location;
								this.Refresh();
								return;
							}
							case ElementType.NodeItem:			// drag in node-item
							{
								var nodeItem = DragElement as NodeItem;
								needRedraw		= nodeItem.OnDrag(transformed_location);
								snappedLocation = lastLocation = e.Location;
								break;
							}
							case ElementType.Connection:		// start dragging end of connection to new input connector
							{
								BringElementToFront(DragElement);
								var connection			= DragElement as NodeConnection;
								var outputConnector		= connection.From;
								FocusElement			= outputConnector.Node;
								if (Disconnect(connection))
								{
									DragElement	= outputConnector;
								} else
									DragElement = null;

								goto case ElementType.OutputConnector;
							}
							case ElementType.InputConnector:	// drag connection from input or output connector
							case ElementType.OutputConnector:
							{	
								snappedLocation = lastLocation = e.Location;
								needRedraw = true;
								break;
							}
						}
					}
				}
			}

			IElement draggingOverElement = null;
			var element = FindElementAt(transformed_location);
			if (element != null)
			{
				switch (element.ElementType)
				{
					default:
						if (DragElement != null)
							element = null;
						break;

					case ElementType.NodeItem:
					{	
						var item = element as NodeItem;
						if (DragElement != null)
						{
							element = item.Node;
							goto case ElementType.Node;
						}
						break;
					}
					case ElementType.Node:
					{
						var node = element as Node;
						if (DragElement != null)
						{
							if (DragElement.ElementType == ElementType.InputConnector)
							{
								var dragConnector = DragElement as NodeConnector;
								if (node.outputConnectors.Count == 1)
								{
									element = node.outputConnectors[0];
									goto case ElementType.OutputConnector;
								}

								if (node != dragConnector.Node)
									draggingOverElement = node;
							} else
							if (DragElement.ElementType == ElementType.OutputConnector)
							{
								var dragConnector = DragElement as NodeConnector;
								if (node.inputConnectors.Count == 1)
								{
									element = node.inputConnectors[0];
									goto case ElementType.InputConnector;
								}

								if (node != dragConnector.Node)
									draggingOverElement = node;
							}

							//element = null;
						}
						break;
					}
					case ElementType.InputConnector:
					case ElementType.OutputConnector:
					{
						var connector = element as NodeConnector;

						if (DragElement != null &&
							(DragElement.ElementType == ElementType.InputConnector ||
							 DragElement.ElementType == ElementType.OutputConnector))
						{
							var dragConnector = DragElement as NodeConnector;
							if (dragConnector.Node == connector.Node ||
								DragElement.ElementType == element.ElementType)
							{
								element = null;
								break;
							}
						}

						var pre_points = new PointF[] { 
							new PointF((connector.bounds.Left + connector.bounds.Right) / 2,
									   (connector.bounds.Top  + connector.bounds.Bottom) / 2) };
						transformation.TransformPoints(pre_points);
						snappedLocation = pre_points[0];
						draggingOverElement = connector.Node;
						break;
					}
				}
			}
		
			if (HoverElement != element)
			{
				HoverElement = element;
				needRedraw = true;
			}

			if (internalDragOverElement != draggingOverElement)
			{
				if (internalDragOverElement != null)
				{
					SetFlag(internalDragOverElement, RenderState.DraggedOver, false);
					needRedraw = true;
				}

				internalDragOverElement = draggingOverElement;

				if (internalDragOverElement != null)
				{
					SetFlag(internalDragOverElement, RenderState.DraggedOver, true);
					needRedraw = true;
				}
			}

			if (needRedraw)
				this.Refresh();
		}
		#endregion

		#region OnMouseUp
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			bool needRedraw = false;
			try
			{
				var points	 = new Point[] { e.Location };
				inverse_transformation.TransformPoints(points);
				var transformed_location = points[0];

				if (DragElement != null)
				{
					switch (DragElement.ElementType)
					{
						case ElementType.InputConnector:
						{
							var inputConnector	= (NodeConnector)DragElement;
							var outputConnector = HoverElement as NodeOutputConnector;
							if (outputConnector != null &&
								outputConnector.Node != inputConnector.Node)
								FocusElement = Connect(outputConnector, inputConnector);
							needRedraw = true;
							return;
						}
						case ElementType.OutputConnector:
						{
							var outputConnector = (NodeConnector)DragElement;
							var inputConnector	= HoverElement as NodeInputConnector;
							if (inputConnector != null &&
								inputConnector.Node != outputConnector.Node)
								FocusElement = Connect(outputConnector, inputConnector);
							needRedraw = true;
							return;
						}
						case ElementType.Connection:
						{
							needRedraw = true;
							return;
						}
						case ElementType.Node:
						{
							needRedraw = true;
							return;
						}
					}
				}
				if (DragElement != null ||
					FocusElement != null)
				{
					FocusElement = null;
					needRedraw = true;
				}
			}
			finally
			{
				if (DragElement != null)
				{
					var nodeItem = DragElement as NodeItem;
					if (nodeItem != null)
						nodeItem.OnEndDrag();
					DragElement = null;
					needRedraw = true;
				}

				dragging = false;
				
				if (needRedraw)
					this.Refresh();
			}
		}
		#endregion

		#region OnDoubleClick
		protected override void OnDoubleClick(EventArgs e)
		{
			base.OnDoubleClick(e);
			if (mouseMoved)
				return;

			var points = new Point[] { lastLocation };
			inverse_transformation.TransformPoints(points);
			var transformed_location = points[0];

			var element = FindElementAt(transformed_location);
			if (element == null)
				return;

			switch (element.ElementType)
			{
				case ElementType.Connection:
					((NodeConnection)element).DoDoubleClick();
					break;
				case ElementType.NodeItem:
					var item = element as NodeItem;
					element = item.Node;
					goto case ElementType.Node;
				case ElementType.Node:
					var node = element as Node;
					node.Collapsed = !node.Collapsed;
					FocusElement = node;
					this.Refresh();
					break;
			}
		}
		#endregion

		#region OnClick
		protected override void  OnClick(EventArgs e)
		{
 			base.OnClick(e);
			if (mouseMoved)
				return;

			var points = new Point[] { lastLocation };
			inverse_transformation.TransformPoints(points);
			var transformed_location = points[0];

			var element = FindElementAt(transformed_location);
			if (element == null)
				return;

			switch (element.ElementType)
			{
				case ElementType.NodeItem:
				{
					var item = element as NodeItem;
					if (item.OnClick())
					{
						mouseMoved = true; // to avoid double-click from firing
						this.Refresh();
						return;
					}
					break;
				}
			}
		}
		#endregion


		#region OnKeyUp
		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);
			if (e.KeyCode == Keys.Delete)
			{
				if (FocusElement == null)
					return;

				switch (FocusElement.ElementType)
				{
					case ElementType.Node:			RemoveNode(FocusElement as Node); break;
					case ElementType.Connection:	Disconnect(FocusElement as NodeConnection); break;
				}
			}
		}
		#endregion


		#region OnDragEnter
		Node dragNode = null;
		protected override void OnDragEnter(DragEventArgs drgevent)
		{
			base.OnDragEnter(drgevent);
			dragNode = null;
			var node = (Node)drgevent.Data.GetData(typeof(Node));
			if (node == null)
				return;

			if (!AddNode(node))
				return;
			dragNode = node;

			drgevent.Effect = DragDropEffects.Copy;
		}
		#endregion

		#region OnDragOver
		protected override void OnDragOver(DragEventArgs drgevent)
		{
			base.OnDragOver(drgevent);
			if (dragNode == null)
				return;

			var location = (PointF)this.PointToClient(new Point(drgevent.X, drgevent.Y));
			location.X -= ((dragNode.bounds.Right - dragNode.bounds.Left) / 2);
			location.Y -= ((dragNode.titleItem.bounds.Bottom - dragNode.titleItem.bounds.Top) / 2);
			
			var points = new PointF[] { location };
			inverse_transformation.TransformPoints(points);
			location = points[0];

			if (dragNode.Location != location)
			{
				dragNode.Location = location;
				this.Invalidate();
			}
			
			drgevent.Effect = DragDropEffects.Copy;
		}
		#endregion

		#region OnDragLeave
		protected override void OnDragLeave(EventArgs e)
		{
			base.OnDragLeave(e);
			if (dragNode == null)
				return;
			RemoveNode(dragNode);
			dragNode = null;
		}
		#endregion

		#region OnDragDrop
		protected override void OnDragDrop(DragEventArgs drgevent)
		{
			base.OnDragDrop(drgevent);
		}
		#endregion
	}
}

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
		public GraphControl()
		{
			InitializeComponent();
			this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.UserPaint, true);
		}

		public event EventHandler<AcceptNodeEventArgs>				NodeAdded;
		public event EventHandler<AcceptNodeEventArgs>				NodeRemoved;
		public event EventHandler<AcceptNodeConnectionEventArgs>	ConnectionAdded;
		public event EventHandler<AcceptNodeConnectionEventArgs>	ConnectionRemoved;

		readonly List<Node> graphNodes = new List<Node>();

		NodeConnector			currentInputConnector;
		NodeConnector			currentOutputConnector;
		NodeItem				currentItem;
		Node					currentNode;
		NodeConnection			currentConnection;
		bool					selecting = false;
		bool					mouseMoved = false;

		NodeConnector			hoverInputConnector;
		NodeConnector			hoverOutputConnector;
		Node					hoverNode;
		NodeItem				hoverItem;
		NodeConnection			hoverConnection;

		bool					NodeHasFocus = false;

		#region FocusNode
		Node					privateFocusNode;
		public Node				FocusNode 
		{
			get { return privateFocusNode; }
			private set
			{
				if (privateFocusNode == value ||
					(value != null && !graphNodes.Contains(value)))
					return;
				if (privateFocusNode != null)
					privateFocusNode.state &= ~RenderState.Focus;
				privateFocusNode = value;
				if (privateFocusNode != null)
				{
					if (!NodeHasFocus)
						FocusConnection = null;
					NodeHasFocus = true;
					privateFocusNode.state |= RenderState.Focus;

					if (graphNodes[0] == privateFocusNode)
					{
						graphNodes.Remove(privateFocusNode);
						graphNodes.Insert(0, privateFocusNode);
					}
				}
			}
		}
		#endregion

		#region FocusConnection
		NodeConnection privateFocusConnection;
		public NodeConnection FocusConnection
		{
			get { return privateFocusConnection; }
			private set
			{
				if (privateFocusConnection == value ||
					(value != null && !graphNodes.Contains(value.From.Node)))
					return;
				if (privateFocusConnection != null)
					privateFocusConnection.state &= ~RenderState.Focus;
				privateFocusConnection = value;
				if (privateFocusConnection != null)
				{
					FocusNode = privateFocusConnection.To.Node;
					FocusNode = privateFocusConnection.From.Node;
					FocusNode = null;
					var connections = privateFocusConnection.To.Node.connections;
					if (connections[0] != privateFocusConnection)
					{
						connections.Remove(privateFocusConnection);
						connections.Insert(0, privateFocusConnection);
					}

					connections = privateFocusConnection.From.Node.connections;
					if (connections[0] != privateFocusConnection)
					{
						connections.Remove(privateFocusConnection);
						connections.Insert(0, privateFocusConnection);
					}
					NodeHasFocus = false;
					privateFocusConnection.state |= RenderState.Focus;
				}
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

		bool					dragging = false;
		Point					lastLocation;
		PointF					snappedLocation;

		PointF					translation = new PointF();
		float					zoom = 1.0f;

		readonly Matrix			transformation = new Matrix();
		readonly Matrix			inverse_transformation = new Matrix();

		#region UpdateMatrices
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


		#region Nodes
		public IEnumerable<Node> Nodes { get { return graphNodes; } }
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
			FocusNode = node;
			this.Invalidate();
			return true;
		}
		#endregion

		#region AddNodes
		public void AddNodes(IEnumerable<Node> nodes)
		{
			int		index		= 0;
			bool	redraw		= false;
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
						redraw = true;
					} else
						lastNode = node;
				} else
					lastNode = node;
			}
			if (lastNode != null)
				FocusNode = lastNode;
			if (redraw)
				this.Invalidate();
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
			if (node == FocusNode)
				FocusNode = null;
			if (FocusConnection != null &&
				(FocusConnection.From.Node == node ||
				FocusConnection.To.Node == node))
				FocusConnection = null;

			NodeUtility.DisconnectAll(node);
			graphNodes.Remove(node);
			this.Invalidate();
		}
		#endregion

		#region RemoveNodes
		public void RemoveNodes(IEnumerable<Node> nodes)
		{
			bool redraw = false;
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

				if (node == FocusNode)
					FocusNode = null;
				if (FocusConnection != null &&
					(FocusConnection.From.Node == node ||
					FocusConnection.To.Node == node))
					FocusConnection = null;

				NodeUtility.DisconnectAll(node);
				graphNodes.Remove(node);
				redraw = true;
			}
			if (redraw)
				this.Invalidate();
		}
		#endregion


		#region Connect
		public NodeConnection Connect(NodeItem from, NodeItem to)
		{
			return Connect(from.Output, to.Input);
		}

		public NodeConnection Connect(NodeConnector from, NodeConnector to)
		{
			var connection = NodeUtility.Connect(from, to);

			if (connection != null &&
				ConnectionAdded != null)
			{
				var eventArgs = new AcceptNodeConnectionEventArgs(connection);
				ConnectionAdded(this, eventArgs);
				if (eventArgs.Cancel)
				{
					NodeUtility.Disconnect(connection);
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
			
			if (FocusConnection == connection)
				FocusConnection = null;

			NodeUtility.Disconnect(connection);
			return true;
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

			NodeUtility.Render(e.Graphics, graphNodes, ShowLabels);
			if (dragging)
			{
				var points = new PointF[] { snappedLocation };
				inverse_transformation.TransformPoints(points);
				var transformed_location = points[0];

				if (currentOutputConnector != null)
					NodeUtility.RenderOutputConnection(e.Graphics, currentOutputConnector, 
						transformed_location.X, transformed_location.Y, RenderState.Dragging | RenderState.Hover);
				if (currentInputConnector != null)
					NodeUtility.RenderInputConnection(e.Graphics, currentInputConnector, 
						transformed_location.X, transformed_location.Y, RenderState.Dragging | RenderState.Hover);
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
			
			var points = new Point[] { e.Location };
			inverse_transformation.TransformPoints(points);
			var location = points[0];

			selecting = false;
			dragging = false;
			mouseMoved = false;
			snappedLocation = lastLocation = e.Location;

			foreach(var node in graphNodes)
			{
				var inputConnector = NodeUtility.FindInputConnectorAt(node, location);
				if (inputConnector != null)
				{
					if (currentItem != null)
						currentItem.OnEndDrag();
					FocusConnection			= null;
					currentInputConnector	= inputConnector;
					currentItem			= null;
					currentNode				= null;
					currentOutputConnector	= null;
					currentConnection		= null;
					currentInputConnector.state |= RenderState.Dragging;
					selecting = true;
					FocusNode = node;
					this.Refresh();
					return;
				}
				var outputConnector = NodeUtility.FindOutputConnectorAt(node, location);
				if (outputConnector != null)
				{
					if (currentItem != null)
						currentItem.OnEndDrag();
					FocusConnection			= null;
					currentOutputConnector	= outputConnector;
					currentItem			= null;
					currentNode				= null;
					currentInputConnector	= null;
					currentConnection		= null;
					currentOutputConnector.state |= RenderState.Dragging;
					currentOutputConnector.Node.state |= RenderState.Dragging;
					selecting = true;
					FocusNode = node;
					this.Refresh();
					return;
				}
				if (node.bounds.Contains(location))
				{
					var item = NodeUtility.FindItemAt(node, location);
					if (item != null)
					{
						if (item.OnStartDrag(location))
						{
							currentItem			= item;
							currentNode				= node;
							currentInputConnector	= null;
							currentOutputConnector	= null;
							currentConnection		= null;
							selecting = true;
							FocusNode = node;
							this.Refresh();
							return;
						}
					}
					if (currentItem != null)
						currentItem.OnEndDrag();
					currentItem			= null;
					currentNode				= node;
					currentInputConnector	= null;
					currentOutputConnector	= null;
					currentConnection		= null;

					selecting = true;
					FocusNode = node;
					this.Refresh();
					return;
				}
			}


			foreach (var node in graphNodes)
			{
				foreach (var connection in node.connections)
				{
					if ((connection.state & RenderState.Hover) == RenderState.Hover)
					{
						FocusNode = node;

						if (currentItem != null)
							currentItem.OnEndDrag();
						currentInputConnector	= null;
						currentItem			= null;
						currentNode				= null;
						currentOutputConnector	= null;
						FocusConnection			=
						currentConnection		= connection;

						connection.state |= RenderState.Dragging;
						selecting = true;
						this.Refresh();
						return;
					}
				}
			}

			selecting = true;
		}
		#endregion

		#region OnMouseMove
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			var points = new Point[] { e.Location };
			inverse_transformation.TransformPoints(points);
			var location = points[0];
			var deltaX = (lastLocation.X - e.Location.X) / zoom;
			var deltaY = (lastLocation.Y - e.Location.Y) / zoom;

			bool needRedraw = false;

			if (selecting && !dragging)
			{
				if ((Math.Abs(deltaX) > 1) ||
					(Math.Abs(deltaY) > 1))
				{
					dragging = true;
					mouseMoved = true;
				}
			} else
			if (dragging)
			{
				if ((Math.Abs(deltaX) > 0) ||
					(Math.Abs(deltaY) > 0))
				{
					mouseMoved = true;
					if (currentItem != null)
					{
						needRedraw = currentItem.OnDrag(location);
						snappedLocation = lastLocation = e.Location;
						FocusNode = currentNode;
					} else
					if (currentNode != null)
					{
						currentNode.Location = new Point((int)Math.Round(currentNode.Location.X - deltaX),
														 (int)Math.Round(currentNode.Location.Y - deltaY));
						snappedLocation = lastLocation = e.Location;
						needRedraw = true;
						FocusNode = currentNode;
					} else
					if (currentOutputConnector != null ||
						currentInputConnector != null)
					{
						snappedLocation = lastLocation = e.Location;
						needRedraw = true;
					} else
					if (currentConnection != null)
					{
						currentOutputConnector	= currentConnection.From;
						FocusNode				= currentOutputConnector.Node;
						if (Disconnect(currentConnection))
						{
							if (currentItem != null)
								currentItem.OnEndDrag();
							FocusConnection			= null;
							currentItem			= null;
							currentNode				= null;
							currentInputConnector	= null;
							currentConnection		= null;
							currentOutputConnector.state |= RenderState.Dragging;
							currentOutputConnector.Node.state  |= RenderState.Dragging;
						} else
						{
							if (currentItem != null)
								currentItem.OnEndDrag();
							currentItem			= null;
							currentNode				= null;
							currentInputConnector	= null;
							currentOutputConnector	= null;
						}

						selecting = true;
						snappedLocation = lastLocation = e.Location;
						needRedraw = true;
					} else
					{
						translation.X -= deltaX;
						translation.Y -= deltaY;
						snappedLocation = lastLocation = e.Location;
						needRedraw = true;
						this.Refresh();
						return;
					}
				}
			}

			Node		foundHoverNode			= null;
			NodeItem	foundHoverItem			= null;
			bool		foundInputConnector		= false;
			bool		foundOutputConnector	= false;
			foreach(var node in graphNodes)
			{
				var inputConnector = NodeUtility.FindInputConnectorAt(node, location);
				if (inputConnector != null)
				{
					if (hoverInputConnector != inputConnector)
					{
						if (hoverInputConnector != null)
						{
							if ((hoverInputConnector.state & RenderState.Hover) == RenderState.Hover)
							{
								hoverInputConnector.state &= ~RenderState.Hover;
								needRedraw = true;
							}
						}
						if (!selecting || 
							inputConnector == currentInputConnector ||
							(currentOutputConnector != null && currentOutputConnector.Node != node))
						{
							hoverInputConnector = inputConnector;
							if ((hoverInputConnector.state & RenderState.Hover) != RenderState.Hover)
							{
								hoverInputConnector.state |= RenderState.Hover;
								needRedraw = true;
							}
						}
					}
					foundInputConnector = true;
					foundHoverNode = node;
					break;
				}

				var outputConnector = NodeUtility.FindOutputConnectorAt(node, location);
				if (outputConnector != null)
				{
					if (hoverOutputConnector != outputConnector)
					{
						if (hoverOutputConnector != null)
						{
							if ((hoverOutputConnector.state & RenderState.Hover) == RenderState.Hover)
							{
								hoverOutputConnector.state &= ~RenderState.Hover;
								needRedraw = true;
							}
						}
						if (!selecting ||
							outputConnector == currentOutputConnector ||
							(currentInputConnector != null && currentInputConnector.Node != node))
						{
							hoverOutputConnector = outputConnector;
							if ((hoverOutputConnector.state & RenderState.Hover) != RenderState.Hover)
							{
								hoverOutputConnector.state |= RenderState.Hover;
								needRedraw = true;
							}
						}
					}
					foundOutputConnector = true;
					foundHoverNode = node;
					break;
				}

				if (node.bounds.Contains(location))
				{
					foundHoverNode = node;
					break;
				}
			}

			if (currentItem != null)
				foundHoverNode = currentItem.Node;

			if (foundHoverNode != hoverNode)
			{
				if (hoverNode != null)
				{
					if ((currentInputConnector  != null && currentInputConnector.Node  == hoverNode) ||
						(currentOutputConnector != null && currentOutputConnector.Node == hoverNode))
						hoverNode.state &= ~RenderState.Hover;
					else
						hoverNode.state &= ~(RenderState.Hover | RenderState.Dragging);
					needRedraw = true;
				}
				hoverNode = foundHoverNode;
			}

			if (hoverNode != null)
			{
				var newState = hoverNode.state | RenderState.Hover;

				if ((currentInputConnector  != null && currentInputConnector.Node  != hoverNode) ||
					(currentOutputConnector != null && currentOutputConnector.Node != hoverNode))
					newState |= RenderState.Dragging;
				else
				if (currentInputConnector  == null && currentOutputConnector == null)
					newState &= ~RenderState.Dragging;
				if (hoverNode.state != newState)
				{
					hoverNode.state = newState;
					needRedraw = true;
				}

				var item = NodeUtility.FindItemAt(hoverNode, location);
				if (item != null)
					foundHoverItem = item;
			}

			if (foundHoverItem != hoverItem)
			{
				if (hoverItem != null)
				{
					needRedraw = hoverItem.OnLeave() || needRedraw;
					hoverItem = null;
				}
				if (foundHoverItem != null)
				{
					if (foundHoverItem.OnEnter())
					{
						needRedraw = true;
						hoverItem = foundHoverItem;
					}
				}					
			}


			if (!foundInputConnector &&
				hoverInputConnector != null)
			{
				hoverInputConnector.state &= ~RenderState.Hover;
				hoverInputConnector = null;
				needRedraw = true;
			} else
			if (hoverInputConnector != null)
			{
				var pre_points = new PointF[] { 
					new PointF((hoverInputConnector.bounds.Left + hoverInputConnector.bounds.Right) / 2,
							   (hoverInputConnector.bounds.Top  + hoverInputConnector.bounds.Bottom) / 2) };
				transformation.TransformPoints(pre_points);
				snappedLocation = pre_points[0];
			}

			if (!foundOutputConnector &&
				hoverOutputConnector != null)
			{
				hoverOutputConnector.state &= ~RenderState.Hover;
				hoverOutputConnector = null;
				needRedraw = true;
			} else
			if (hoverOutputConnector != null)
			{
				var pre_points = new PointF[] { 
					new PointF( (hoverOutputConnector.bounds.Left + hoverOutputConnector.bounds.Right) / 2,
							   (hoverOutputConnector.bounds.Top + hoverOutputConnector.bounds.Bottom) / 2) };
				transformation.TransformPoints(pre_points);
				snappedLocation = pre_points[0];
			}

			NodeConnection foundConnectionNode = null;
			if (!dragging &&
				foundHoverNode == null &&
				currentItem == null &&
				hoverOutputConnector == null &&
				hoverInputConnector == null)
			{
				var skipConnections = new HashSet<NodeConnection>();
				var foundConnections = new List<NodeConnection>();
				foreach (var node in graphNodes)
				{
					foreach (var connection in node.connections.Reverse<NodeConnection>())
					{
						if (skipConnections.Add(connection)) // if we can add it, we haven't checked it yet
						{
							if (connection.bounds.Contains(location))
								foundConnections.Add(connection);
						}
					}
				}
				foreach (var connection in foundConnections)
				{
					if (connection.textBounds.Contains(location))
					{
						foundConnectionNode = connection;
						break;
					}
				}
				if (foundConnectionNode == null)
				{
					foreach(var connection in foundConnections)
					{
						using (var region = NodeUtility.GetConnectionRegion(connection))
						{
							if (region.IsVisible(location))
							{
								foundConnectionNode = connection;
								break;
							}
						}
					}
				}
				if (foundConnectionNode != null &&
					hoverConnection != foundConnectionNode)
				{
					if (hoverConnection != null)
						hoverConnection.state &= ~RenderState.Hover;
					hoverConnection = foundConnectionNode;
					hoverConnection.state |= RenderState.Hover;
					needRedraw = true;
				}
			}


			if (foundConnectionNode == null &&
				hoverConnection != null)
			{
				hoverConnection.state &= ~RenderState.Hover;
				hoverConnection = null;
				needRedraw = true;
			}

			if (needRedraw)
				this.Refresh();
		}
		#endregion

		#region OnMouseUp
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			try
			{
				if (!selecting)
					return;

				var points	 = new Point[] { e.Location };
				inverse_transformation.TransformPoints(points);
				var location = points[0];

				if (currentInputConnector != null)
				{
					if (hoverOutputConnector != null &&
						hoverOutputConnector.Node != currentInputConnector.Node)
						FocusConnection =
							Connect(hoverOutputConnector, currentInputConnector);
					currentInputConnector.state &= ~RenderState.Dragging;
					currentInputConnector = null;
					this.Refresh();
					return;
				}
				if (currentOutputConnector != null)
				{
					if (hoverInputConnector != null &&
						hoverInputConnector.Node != currentOutputConnector.Node)
						FocusConnection = 
							Connect(currentOutputConnector, hoverInputConnector);
					currentOutputConnector.state &= ~RenderState.Dragging;
					currentOutputConnector.Node.state &= ~RenderState.Dragging;
					currentOutputConnector = null;
					this.Refresh();
					return;
				}

				if (currentConnection != null)
				{
					currentConnection.state &= ~RenderState.Dragging;
					this.Refresh();
					return;
				}

				if (currentNode != null)
				{
					return;
				}

				if (FocusNode != null ||
					FocusConnection != null)
				{
					FocusNode = null;
					FocusConnection = null;
					this.Refresh();
					return;
				}
			}
			finally
			{
				if (currentItem != null)
					currentItem.OnEndDrag();
				currentItem			= null;
				currentNode				= null;
				currentInputConnector	= null;
				currentOutputConnector	= null;
				currentConnection		= null;

				selecting = false;
				dragging = false;
			}
		}
		#endregion

		#region OnDoubleClick
		protected override void OnDoubleClick(EventArgs e)
		{
			base.OnDoubleClick(e);
			if (mouseMoved)
				return;
			
			if (currentConnection != null)
			{
				currentConnection.DoDoubleClick();
				return;
			}

			if (hoverItem == null &&
				currentNode != null)
			{
				if (!dragging)
				{
					currentNode.Collapsed = !currentNode.Collapsed;
					FocusNode = currentNode;
					this.Refresh();
				}
				return;
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
			var location = points[0];

			
			foreach(var node in graphNodes)
			{
				if (!node.bounds.Contains(location))
					continue;
				
				var item = NodeUtility.FindItemAt(node, location);
				if (item != null)
				{
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

		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);
			if (e.KeyCode == Keys.Delete)
			{
				if (FocusNode != null)
				{
					RemoveNode(FocusNode);
				} else
				if (FocusConnection != null)
				{
					Disconnect(FocusConnection);
				}
			}
		}


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

﻿#region License
// Copyright (c) 2009 Sander van Rossen, 2013 Oliver Salzburg
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
using System.Drawing.Drawing2D;

namespace Graph
{
	public static class GraphRenderer
	{
		static IEnumerable<NodeItem> EnumerateNodeItems(Node node)
		{
			if (node == null)
				yield break;

			yield return node.titleItem;
			if (node.Collapsed)
				yield break;
			
			foreach (var item in node.Items)
				yield return item;
		}

		public static SizeF Measure(Graphics context, Node node)
		{
			if (node == null)
				return SizeF.Empty;

			SizeF size = Size.Empty;
			size.Height = //(int)NodeConstants.TopHeight + 
				(int)GraphConstants.BottomHeight;
			foreach (var item in EnumerateNodeItems(node))
			{
				var itemSize = item.Measure(context);
				size.Width = Math.Max(size.Width, itemSize.Width);
				size.Height += GraphConstants.ItemSpacing + itemSize.Height;
			}
			
			if (node.Collapsed)
				size.Height -= GraphConstants.ItemSpacing;

			size.Width += GraphConstants.NodeExtraWidth;
			return size;
		}

		static SizeF PreRenderItem(Graphics graphics, NodeItem item, PointF position)
		{
			var itemSize = (SizeF)item.Measure(graphics);
			item.bounds = new RectangleF(position, itemSize);
			return itemSize;
		}

		static void RenderItem(Graphics graphics, SizeF minimumSize, NodeItem item, PointF position)
		{
			item.Render(graphics, minimumSize, position);
		}

		private static Pen BorderPen = new Pen(Color.FromArgb(64, 64, 64));

		static void RenderConnector(Graphics graphics, RectangleF bounds, RenderState state)
		{
			using (var brush = new SolidBrush(GetArrowLineColor(state)))
			{
				graphics.FillEllipse(brush, bounds);
			}
			
			if (state == RenderState.None)
			{
				graphics.DrawEllipse(Pens.Black, bounds);
			} else
			// When we're compatible, but not dragging from this node we render a highlight
			if ((state & (RenderState.Compatible | RenderState.Dragging)) == RenderState.Compatible) 
			{
				// First draw the normal black border
				graphics.DrawEllipse(Pens.Black, bounds);

				// Draw an additional highlight around the connector
				RectangleF highlightBounds = new RectangleF(bounds.X,bounds.Y,bounds.Width,bounds.Height);
				highlightBounds.Width += 10;
				highlightBounds.Height += 10;
				highlightBounds.X -= 5;
				highlightBounds.Y -= 5;
				graphics.DrawEllipse(Pens.OrangeRed, highlightBounds);
			} else
			{
				graphics.DrawArc(Pens.Black, bounds, 90, 180);
				using (var pen = new Pen(GetArrowLineColor(state)))
				{
					graphics.DrawArc(pen, bounds, 270, 180);
				}
			}			
		}

		static void RenderArrow(Graphics graphics, RectangleF bounds, RenderState connectionState)
		{
			var x = (bounds.Left + bounds.Right) / 2.0f;
			var y = (bounds.Top + bounds.Bottom) / 2.0f;
			using (var brush = new SolidBrush(GetArrowLineColor(connectionState | RenderState.Connected)))
			{
				graphics.FillPolygon(brush, GetArrowPoints(x,y), FillMode.Winding);
			}
		}

		public static void PerformLayout(Graphics graphics, IEnumerable<Node> nodes)
		{
			foreach (var node in nodes.Reverse<Node>())
			{
				GraphRenderer.PerformLayout(graphics, node);
			}
		}

		public static void Render(Graphics graphics, IEnumerable<Node> nodes, bool showLabels)
		{
			var skipConnections = new HashSet<NodeConnection>();
			foreach (var node in nodes.Reverse<Node>())
			{
				GraphRenderer.RenderConnections(graphics, node, skipConnections, showLabels);
			}
			foreach (var node in nodes.Reverse<Node>())
			{
				GraphRenderer.Render(graphics, node);
			}
		}

		public static void PerformLayout(Graphics graphics, Node node)
		{
			if (node == null)
				return;
			var size		= Measure(graphics, node);
			var position	= node.Location;
			node.bounds		= new RectangleF(position, size);
			
			var path				= new GraphicsPath(FillMode.Winding);
			int connectorSize		= (int)GraphConstants.ConnectorSize;
			int halfConnectorSize	= (int)Math.Ceiling(connectorSize / 2.0f);
			var connectorOffset		= (int)Math.Floor((GraphConstants.MinimumItemHeight - GraphConstants.ConnectorSize) / 2.0f);
			var left				= position.X + halfConnectorSize;
			var top					= position.Y;
			var right				= position.X + size.Width - halfConnectorSize;
			var bottom				= position.Y + size.Height;
			
			node.inputConnectors.Clear();
			node.outputConnectors.Clear();
			//node.connections.Clear();

			var itemPosition = position;
			itemPosition.X += connectorSize + (int)GraphConstants.HorizontalSpacing;
			if (node.Collapsed)
			{
				foreach (var item in node.Items)
				{
					var inputConnector	= item.Input;
					if (inputConnector != null && inputConnector.Enabled)
					{
						inputConnector.bounds = Rectangle.Empty;
						node.inputConnectors.Add(inputConnector);
					}
					var outputConnector = item.Output;
					if (outputConnector != null && outputConnector.Enabled)
					{
						outputConnector.bounds = Rectangle.Empty;
						node.outputConnectors.Add(outputConnector);
					}
				}
				var itemSize		= PreRenderItem(graphics, node.titleItem, itemPosition);
				var realHeight		= itemSize.Height - GraphConstants.TopHeight;
				var connectorY		= itemPosition.Y  + (int)Math.Ceiling(realHeight / 2.0f);
				
				node.inputBounds	= new RectangleF(left  - (GraphConstants.ConnectorSize / 2), 
													 connectorY, 
													 GraphConstants.ConnectorSize, 
													 GraphConstants.ConnectorSize);
				node.outputBounds	= new RectangleF(right - (GraphConstants.ConnectorSize / 2), 
													 connectorY, 
													 GraphConstants.ConnectorSize, 
													 GraphConstants.ConnectorSize);
			} else
			{
				node.inputBounds	= Rectangle.Empty;
				node.outputBounds	= Rectangle.Empty;
				
				foreach (var item in EnumerateNodeItems(node))
				{
					var itemSize		= PreRenderItem(graphics, item, itemPosition);
					var realHeight		= itemSize.Height;
					var inputConnector	= item.Input;
					if (inputConnector != null && inputConnector.Enabled)
					{
						if (itemSize.IsEmpty)
						{
							inputConnector.bounds = Rectangle.Empty;
						} else
						{
							inputConnector.bounds = new RectangleF(	left - (GraphConstants.ConnectorSize / 2), 
																	itemPosition.Y + connectorOffset, 
																	GraphConstants.ConnectorSize, 
																	GraphConstants.ConnectorSize);
						}
						node.inputConnectors.Add(inputConnector);
					}
					var outputConnector = item.Output;
					if (outputConnector != null && outputConnector.Enabled)
					{
						if (itemSize.IsEmpty)
						{
							outputConnector.bounds = Rectangle.Empty;
						} else
						{
							outputConnector.bounds = new RectangleF(right - (GraphConstants.ConnectorSize / 2), 
																	itemPosition.Y + realHeight - (connectorOffset + GraphConstants.ConnectorSize), 
																	GraphConstants.ConnectorSize, 
																	GraphConstants.ConnectorSize);
						}
						node.outputConnectors.Add(outputConnector);
					}
					itemPosition.Y += itemSize.Height + GraphConstants.ItemSpacing;
				}
			}
			node.itemsBounds = new RectangleF(left, top, right - left, bottom - top);
		}

		static void Render(Graphics graphics, Node node)
		{
			var size		= node.bounds.Size;
			var position	= node.bounds.Location;
			
			int cornerSize			= (int)GraphConstants.CornerSize * 2;
			int connectorSize		= (int)GraphConstants.ConnectorSize;
			int halfConnectorSize	= (int)Math.Ceiling(connectorSize / 2.0f);
			var connectorOffset		= (int)Math.Floor((GraphConstants.MinimumItemHeight - GraphConstants.ConnectorSize) / 2.0f);
			var left				= position.X + halfConnectorSize;
			var top					= position.Y;
			var right				= position.X + size.Width - halfConnectorSize;
			var bottom				= position.Y + size.Height;
			using (var path = new GraphicsPath(FillMode.Winding))
			{
				path.AddArc(left, top, cornerSize, cornerSize, 180, 90);
				path.AddArc(right - cornerSize, top, cornerSize, cornerSize, 270, 90);

				path.AddArc(right - cornerSize, bottom - cornerSize, cornerSize, cornerSize, 0, 90);
				path.AddArc(left, bottom - cornerSize, cornerSize, cornerSize, 90, 90);
				path.CloseFigure();

				if ((node.state & (RenderState.Dragging | RenderState.Focus)) != 0)
				{
					graphics.FillPath(Brushes.DarkOrange, path);
				} else
				if ((node.state & RenderState.Hover) != 0)
				{
					graphics.FillPath(Brushes.LightSteelBlue, path);
				} else
				{
					graphics.FillPath(Brushes.LightGray, path);
				}
				graphics.DrawPath(BorderPen, path);
			}
			/*
			if (!node.Collapsed)
				graphics.DrawLine(Pens.Black, 
					left  + GraphConstants.ConnectorSize, node.titleItem.bounds.Bottom - GraphConstants.ItemSpacing, 
					right - GraphConstants.ConnectorSize, node.titleItem.bounds.Bottom - GraphConstants.ItemSpacing);
			*/
			var itemPosition = position;
			itemPosition.X += connectorSize + (int)GraphConstants.HorizontalSpacing;
			if (node.Collapsed)
			{
				bool inputConnected = false;
				var inputState	= RenderState.None;
				var outputState = node.outputState;
				foreach (var connection in node.connections)
				{
					if (connection.To.Node == node)
					{
						inputState |= connection.state;
						inputConnected = true;
					}
					if (connection.From.Node == node)
						outputState |= connection.state | RenderState.Connected;
				}

				RenderItem(graphics, new SizeF(node.bounds.Width - GraphConstants.NodeExtraWidth, 0), node.titleItem, itemPosition);
				if (node.inputConnectors.Count > 0)
					RenderConnector(graphics, node.inputBounds, node.inputState);
				if (node.outputConnectors.Count > 0)
					RenderConnector(graphics, node.outputBounds, outputState);
				if (inputConnected)
					RenderArrow(graphics, node.inputBounds, inputState);
			} else
			{
				node.inputBounds	= Rectangle.Empty;
				node.outputBounds	= Rectangle.Empty;
				
				var minimumItemSize = new SizeF(node.bounds.Width - GraphConstants.NodeExtraWidth, 0);
				foreach (var item in EnumerateNodeItems(node))
				{
					RenderItem(graphics, minimumItemSize, item, itemPosition);
					var inputConnector	= item.Input;
					if (inputConnector != null && inputConnector.Enabled)
					{
						if (!inputConnector.bounds.IsEmpty)
						{
							var state		= RenderState.None;
							var connected	= false;
							foreach (var connection in node.connections)
							{
								if (connection.To == inputConnector)
								{
									state |= connection.state;
									connected = true;
								}
							}

							RenderConnector(graphics, 
											inputConnector.bounds,
											inputConnector.state);

							if (connected)
								RenderArrow(graphics, inputConnector.bounds, state);
						}
					}
					var outputConnector = item.Output;
					if (outputConnector != null && outputConnector.Enabled)
					{
						if (!outputConnector.bounds.IsEmpty)
						{
							var state = outputConnector.state;
							foreach (var connection in node.connections)
							{
								if (connection.From == outputConnector)
									state |= connection.state | RenderState.Connected;
							}
							RenderConnector(graphics, outputConnector.bounds, state);
						}
					}
					itemPosition.Y += item.bounds.Height + GraphConstants.ItemSpacing;
				}
			}
		}

		public static void RenderConnections(Graphics graphics, Node node, HashSet<NodeConnection> skipConnections, bool showLabels)
		{
			foreach (var connection in node.connections.Reverse<NodeConnection>())
			{
				if (connection == null ||
					connection.From == null ||
					connection.To == null)
					continue;

				if (skipConnections.Add(connection))
				{
					var to		= connection.To;
					var from	= connection.From;
					RectangleF toBounds;
					RectangleF fromBounds;
					if (to.Node.Collapsed)		toBounds = to.Node.inputBounds;
					else						toBounds = to.bounds;
					if (from.Node.Collapsed)	fromBounds = from.Node.outputBounds;
					else						fromBounds = from.bounds;

					var x1 = (fromBounds.Left + fromBounds.Right) / 2.0f;
					var y1 = (fromBounds.Top + fromBounds.Bottom) / 2.0f;
					var x2 = (toBounds.Left + toBounds.Right) / 2.0f;
					var y2 = (toBounds.Top + toBounds.Bottom) / 2.0f;

					float centerX;
					float centerY;
					using (var path = GetArrowLinePath(x1, y1, x2, y2, out centerX, out centerY, false))
					{
						using (var brush = new SolidBrush(GetArrowLineColor(connection.state | RenderState.Connected)))
						{
							graphics.FillPath(brush, path);
						}
						connection.bounds = path.GetBounds();
					}

					if (showLabels &&
						!string.IsNullOrWhiteSpace(connection.Name))
					{
						var center = new PointF(centerX, centerY);
						RenderLabel(graphics, connection, center, connection.state);
					}
				}
			}
		}

		static void RenderLabel(Graphics graphics, NodeConnection connection, PointF center, RenderState state)
		{
			using (var path = new GraphicsPath(FillMode.Winding))
			{			
				int cornerSize			= (int)GraphConstants.CornerSize * 2;
				int connectorSize		= (int)GraphConstants.ConnectorSize;
				int halfConnectorSize	= (int)Math.Ceiling(connectorSize / 2.0f);

				SizeF size;
				PointF position;
				var text		= connection.Name;
				if (connection.textBounds.IsEmpty ||
					connection.textBounds.Location != center)
				{
					size		= graphics.MeasureString(text, SystemFonts.StatusFont, center, GraphConstants.CenterTextStringFormat);
					position	= new PointF(center.X - (size.Width / 2.0f) - halfConnectorSize, center.Y - (size.Height / 2.0f));
					size.Width	+= connectorSize;
					connection.textBounds = new RectangleF(position, size);
				} else
				{
					size		= connection.textBounds.Size;
					position	= connection.textBounds.Location;
				}

				var halfWidth  = size.Width / 2.0f;
				var halfHeight = size.Height / 2.0f;
				var connectorOffset		= (int)Math.Floor((GraphConstants.MinimumItemHeight - GraphConstants.ConnectorSize) / 2.0f);
				var left				= position.X;
				var top					= position.Y;
				var right				= position.X + size.Width;
				var bottom				= position.Y + size.Height;
				path.AddArc(left, top, cornerSize, cornerSize, 180, 90);
				path.AddArc(right - cornerSize, top, cornerSize, cornerSize, 270, 90);

				path.AddArc(right - cornerSize, bottom - cornerSize, cornerSize, cornerSize, 0, 90);
				path.AddArc(left, bottom - cornerSize, cornerSize, cornerSize, 90, 90);
				path.CloseFigure();

				using (var brush = new SolidBrush(GetArrowLineColor(state)))
				{
					graphics.FillPath(brush, path);
				}
				graphics.DrawString(text, SystemFonts.StatusFont, Brushes.Black, center, GraphConstants.CenterTextStringFormat);

				if (state == RenderState.None)
					graphics.DrawPath(Pens.Black, path);

				//graphics.DrawRectangle(Pens.Black, connection.textBounds.Left, connection.textBounds.Top, connection.textBounds.Width, connection.textBounds.Height);
			}
		}

		public static Region GetConnectionRegion(NodeConnection connection)
		{
			var to		= connection.To;
			var from	= connection.From;
			RectangleF toBounds;
			RectangleF fromBounds;
			if (to.Node.Collapsed)		toBounds = to.Node.inputBounds;
			else						toBounds = to.bounds;
			if (from.Node.Collapsed)	fromBounds = from.Node.outputBounds;
			else						fromBounds = from.bounds;

			var x1 = (fromBounds.Left + fromBounds.Right) / 2.0f;
			var y1 = (fromBounds.Top + fromBounds.Bottom) / 2.0f;
			var x2 = (toBounds.Left + toBounds.Right) / 2.0f;
			var y2 = (toBounds.Top + toBounds.Bottom) / 2.0f;

			Region region;
			float centerX;
			float centerY;
			using (var linePath = GetArrowLinePath(	x1, y1, x2, y2, out centerX, out centerY, true, 5.0f))
			{
				region = new Region(linePath);
			}
			return region;
		}

		static Color GetArrowLineColor(RenderState state)
		{
			if ((state & (RenderState.Hover | RenderState.Dragging)) != 0)
			{
				if ((state & RenderState.Incompatible) != 0)
				{
					return Color.Red;
				} else
				if ((state & RenderState.Compatible) != 0)
				{
					return Color.DarkOrange;
				} else
				if ((state & RenderState.Dragging) != 0)
					return Color.SteelBlue;
				else
					return Color.DarkOrange;
			} else
			if ((state & RenderState.Incompatible) != 0)
			{
				return Color.Gray;
			} else
			if ((state & RenderState.Compatible) != 0)
			{
				return Color.White;
			} else
			if ((state & RenderState.Connected) != 0)
			{
				return Color.Black;
			} else
				return Color.LightGray;
		}
		
		static PointF[] GetArrowPoints(float x, float y, float extra_thickness = 0)
		{
			return new PointF[]{
					new PointF(x - (GraphConstants.ConnectorSize + 1.0f) - extra_thickness, y + (GraphConstants.ConnectorSize / 1.5f) + extra_thickness),
					new PointF(x + 1.0f + extra_thickness, y),
					new PointF(x - (GraphConstants.ConnectorSize + 1.0f) - extra_thickness, y - (GraphConstants.ConnectorSize / 1.5f) - extra_thickness)};
		}

		static List<PointF> GetArrowLinePoints(float x1, float y1, float x2, float y2, out float centerX, out float centerY, float extra_thickness = 0)
		{
			var widthX	= (x2 - x1);
			var lengthX = Math.Max(60, Math.Abs(widthX / 2)) 
				//+ Math.Max(0, -widthX / 2)
				;
			var lengthY = 0;// Math.Max(-170, Math.Min(-120.0f, widthX - 120.0f)) + 120.0f; 
			if (widthX < 120)
				lengthX = 60;
			var yB = ((y1 + y2) / 2) + lengthY;// (y2 + ((y1 - y2) / 2) * 0.75f) + lengthY;
			var yC = y2 + yB;
			var xC = (x1 + x2) / 2;
			var xA = x1 + lengthX;
			var xB = x2 - lengthX;

			/*
			if (widthX >= 120)
			{
				xA = xB = xC = x2 - 60;
			}
			//*/
			
			var points = new List<PointF> { 
				new PointF(x1, y1),
				new PointF(xA, y1),
				new PointF(xB, y2),
				new PointF(x2 - GraphConstants.ConnectorSize - extra_thickness, y2)
			};

			var t  = 1.0f;//Math.Min(1, Math.Max(0, (widthX - 30) / 60.0f));
			var yA = (yB * t) + (yC * (1 - t));

			if (widthX <= 120)
			{
				points.Insert(2, new PointF(xB, yA));
				points.Insert(2, new PointF(xC, yA));
				points.Insert(2, new PointF(xA, yA));
			}
			//*
			using (var tempPath = new GraphicsPath())
			{
				tempPath.AddBeziers(points.ToArray());
				tempPath.Flatten();
				points = tempPath.PathPoints.ToList();
			}
			//*/
			var angles	= new PointF[points.Count - 1];
			var lengths = new float[points.Count - 1];
			float totalLength = 0;
			centerX = 0;
			centerY = 0;
			points.Add(points[points.Count - 1]);
			for (int i = 0; i < points.Count - 2; i++)
			{
				var pt1 = points[i];
				var pt2 = points[i + 1];
				var pt3 = points[i + 2];
				var deltaX = (float)((pt2.X - pt1.X) + (pt3.X - pt2.X));
				var deltaY = (float)((pt2.Y - pt1.Y) + (pt3.Y - pt2.Y));
				var length = (float)Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
				if (length <= 1.0f)
				{
					points.RemoveAt(i);
					i--;
					continue;
				}
				lengths[i] = length;
				totalLength += length;
				angles[i].X = deltaX / length;
				angles[i].Y = deltaY / length;
			}

			float midLength		= (totalLength / 2.0f);// * 0.75f;
			float startWidth	= extra_thickness + 0.75f;
			float endWidth		= extra_thickness + (GraphConstants.ConnectorSize / 3.5f);
			float currentLength = 0;
			var newPoints = new List<PointF>();
			newPoints.Add(points[0]);
			for (int i = 0; i < points.Count - 2; i++)
			{
				var angle	= angles[i];
				var point	= points[i + 1];
				var length	= lengths[i];
				var width	= (((currentLength * (endWidth - startWidth)) / totalLength) + startWidth);
				var angleX	= angle.X * width;
				var angleY	= angle.Y * width;

				var newLength = currentLength + length;
				if (currentLength	<= midLength &&
					newLength		>= midLength)
				{
					var dX = point.X - points[i].X;
					var dY = point.Y - points[i].Y;
					var t1 = midLength - currentLength;
					var l  = length;



					centerX = points[i].X + ((dX * t1) / l);
					centerY = points[i].Y + ((dY * t1) / l);
				}

				var pt1 = new PointF(point.X - angleY, point.Y + angleX);
				var pt2 = new PointF(point.X + angleY, point.Y - angleX);
				if (Math.Abs(newPoints[newPoints.Count - 1].X - pt1.X) > 1.0f ||
					Math.Abs(newPoints[newPoints.Count - 1].Y - pt1.Y) > 1.0f)
					newPoints.Add(pt1);
				if (Math.Abs(newPoints[0].X - pt2.X) > 1.0f ||
					Math.Abs(newPoints[0].Y - pt2.Y) > 1.0f)
					newPoints.Insert(0, pt2);

				currentLength = newLength;
			}

			return newPoints;
		}

		static GraphicsPath GetArrowLinePath(float x1, float y1, float x2, float y2, out float centerX, out float centerY, bool include_arrow, float extra_thickness = 0)
		{
			var newPoints = GetArrowLinePoints(x1, y1, x2, y2, out centerX, out centerY, extra_thickness);

			var path = new GraphicsPath(FillMode.Winding);
			path.AddLines(newPoints.ToArray());
			if (include_arrow)
				path.AddLines(GetArrowPoints(x2, y2, extra_thickness).ToArray());
			path.CloseFigure();
			return path;
		}

		public static void RenderOutputConnection(Graphics graphics, NodeConnector output, float x, float y, RenderState state)
		{
			if (graphics == null ||
				output == null)
				return;
			
			RectangleF outputBounds;
			if (output.Node.Collapsed)	outputBounds = output.Node.outputBounds;
			else						outputBounds = output.bounds;

			var x1 = (outputBounds.Left + outputBounds.Right) / 2.0f;
			var y1 = (outputBounds.Top + outputBounds.Bottom) / 2.0f;
			
			float centerX;
			float centerY;
			using (var path = GetArrowLinePath(x1, y1, x, y, out centerX, out centerY, true, 0.0f))
			{
				using (var brush = new SolidBrush(GetArrowLineColor(state)))
				{
					graphics.FillPath(brush, path);
				}
			}
		}
		
		public static void RenderInputConnection(Graphics graphics, NodeConnector input, float x, float y, RenderState state)
		{
			if (graphics == null || 
				input == null)
				return;
			
			RectangleF inputBounds;
			if (input.Node.Collapsed)	inputBounds = input.Node.inputBounds;
			else						inputBounds = input.bounds;

			var x2 = (inputBounds.Left + inputBounds.Right) / 2.0f;
			var y2 = (inputBounds.Top + inputBounds.Bottom) / 2.0f;

			float centerX;
			float centerY;
			using (var path = GetArrowLinePath(x, y, x2, y2, out centerX, out centerY, true, 0.0f))
			{
				using (var brush = new SolidBrush(GetArrowLineColor(state)))
				{
					graphics.FillPath(brush, path);
				}
			}
		}

		public static GraphicsPath CreateRoundedRectangle(SizeF size, PointF location)
		{
			int cornerSize			= (int)GraphConstants.CornerSize * 2;
			int connectorSize		= (int)GraphConstants.ConnectorSize;
			int halfConnectorSize	= (int)Math.Ceiling(connectorSize / 2.0f);

			var height				= size.Height;
			var width				= size.Width;
			var halfWidth			= width / 2.0f;
			var halfHeight			= height / 2.0f;
			var connectorOffset		= (int)Math.Floor((GraphConstants.MinimumItemHeight - GraphConstants.ConnectorSize) / 2.0f);
			var left				= location.X;
			var top					= location.Y;
			var right				= location.X + width;
			var bottom				= location.Y + height;

			var path = new GraphicsPath(FillMode.Winding);
			path.AddArc(left, top, cornerSize, cornerSize, 180, 90);
			path.AddArc(right - cornerSize, top, cornerSize, cornerSize, 270, 90);

			path.AddArc(right - cornerSize, bottom - cornerSize, cornerSize, cornerSize, 0, 90);
			path.AddArc(left, bottom - cornerSize, cornerSize, cornerSize, 90, 90);
			path.CloseFigure();
			return path;
		}
	}
}

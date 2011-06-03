using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Graph.Items
{
	public sealed class NodeColorItem : NodeItem
	{
		public event EventHandler<NodeItemEventArgs> Clicked;

		public NodeColorItem(string text, Color color, bool inputEnabled, bool outputEnabled) :
			base(inputEnabled, outputEnabled)
		{
			this.Text = text;
			this.Color = color;
		}

		public Color Color { get; set; }

		#region Text
		string internalText = string.Empty;
		public string Text
		{
			get { return internalText; }
			set
			{
				if (internalText == value)
					return;
				internalText = value;
				TextSize = Size.Empty;
			}
		}
		#endregion

		#region Hover
		internal bool Hover { get; set; }
		#endregion

		internal SizeF TextSize;

		public override bool OnEnter()
		{
			base.OnEnter();
			Hover = true;
			return true;
		}

		public override bool OnLeave()
		{
			base.OnLeave();
			Hover = false;
			return true;
		}

		public override bool OnClick()
		{
			base.OnClick();
			if (Clicked != null)
				Clicked(this, new NodeItemEventArgs(this));
			return true;
		}
	}

	[NodeItemDescription(typeof(NodeColorItem))]
	public class NodeColorRenderer : NodeItemRenderer<NodeColorItem>
	{
		const int ColorBoxSize = 16;
		const int Spacing = 2;
		public override SizeF Measure(IDeviceContext context, SizeF minimumSize, NodeColorItem item)
		{
			if (!string.IsNullOrWhiteSpace(item.Text))
			{
				if (item.TextSize.IsEmpty)
				{
					var size = new Size(GraphConstants.MinimumItemWidth, GraphConstants.MinimumItemHeight);

					if (item.Input.Enabled != item.Output.Enabled)
					{
						if (item.Input.Enabled)
							item.TextSize = TextRenderer.MeasureText(context, item.Text, SystemFonts.MenuFont, size, GraphConstants.LeftTextFlags);
						else
							item.TextSize = TextRenderer.MeasureText(context, item.Text, SystemFonts.MenuFont, size, GraphConstants.RightTextFlags);
					} else
						item.TextSize = TextRenderer.MeasureText(context, item.Text, SystemFonts.MenuFont, size, GraphConstants.CenterTextFlags);

					item.TextSize.Width  = Math.Max(size.Width, item.TextSize.Width + ColorBoxSize + Spacing);
					item.TextSize.Height = Math.Max(size.Height, item.TextSize.Height);
				}
				var measuredSize = item.TextSize;
				measuredSize.Width	= Math.Max(minimumSize.Width, measuredSize.Width);
				measuredSize.Height = Math.Max(minimumSize.Height, measuredSize.Height);
				return measuredSize;
			} else
			{
				var measuredSize = new SizeF(GraphConstants.MinimumItemWidth, GraphConstants.TitleHeight + GraphConstants.TopHeight);
				measuredSize.Width  = Math.Max(minimumSize.Width, measuredSize.Width);
				measuredSize.Height = Math.Max(minimumSize.Height, measuredSize.Height);
				return measuredSize;
			}
		}

		public override void Render(Graphics graphics, SizeF minimumSize, NodeColorItem item, PointF location)
		{
			var size = Measure(graphics, minimumSize, item);

			var alignment	= HorizontalAlignment.Center;
			var format		= GraphConstants.CenterTextStringFormat;
			if (item.Input.Enabled != item.Output.Enabled)
			{
				if (item.Input.Enabled)
				{
					alignment	= HorizontalAlignment.Left;
					format		= GraphConstants.LeftTextStringFormat;
				} else
				{
					alignment	= HorizontalAlignment.Right;
					format		= GraphConstants.RightTextStringFormat;
				}
			}

			var rect		= new RectangleF(location, size);
			var colorBox	= new RectangleF(location, size);
			colorBox.Width	= ColorBoxSize;
			switch (alignment)
			{
				case HorizontalAlignment.Left:
					rect.Width	-= ColorBoxSize + Spacing;
					rect.X		+= ColorBoxSize + Spacing;
					break;
				case HorizontalAlignment.Right:
					colorBox.X	= rect.Right - colorBox.Width;
					rect.Width	-= ColorBoxSize + Spacing;
					break;
				case HorizontalAlignment.Center:
					rect.Width	-= ColorBoxSize + Spacing;
					rect.X		+= ColorBoxSize + Spacing;
					break;
			}

			graphics.DrawString(item.Text, SystemFonts.MenuFont, Brushes.Black, rect, format);

			using (var path = NodeUtility.CreateRoundedRectangle(colorBox.Size, colorBox.Location))
			{
				using (var brush = new SolidBrush(item.Color))
				{
					graphics.FillPath(brush, path);
				}
				if (item.Hover)
					graphics.DrawPath(Pens.White, path);
				else
					graphics.DrawPath(Pens.Black, path);
			}
			/*
			using (var brush = new SolidBrush(item.Color))
			{
				graphics.FillRectangle(brush, colorBox.X, colorBox.Y, colorBox.Width, colorBox.Height);
			}
			graphics.DrawRectangle(Pens.Black, colorBox.X, colorBox.Y, colorBox.Width, colorBox.Height);
			*/
		}
	}
}

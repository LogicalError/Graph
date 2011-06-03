using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Graph.Items
{
	public sealed class NodeLabelItem : NodeItem
	{
		public NodeLabelItem(string text, bool inputEnabled, bool outputEnabled) :
			base(inputEnabled, outputEnabled)
		{
			this.Text = text;
		}

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

		internal SizeF TextSize;
	}

	[NodeItemDescription(typeof(NodeLabelItem))]
	public class NodeLabelRenderer : NodeItemRenderer<NodeLabelItem>
	{
		public override SizeF Measure(IDeviceContext context, SizeF minimumSize, NodeLabelItem item)
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

					item.TextSize.Width  = Math.Max(size.Width, item.TextSize.Width);
					item.TextSize.Height = Math.Max(size.Height, item.TextSize.Height);
				}
				var measuredSize = item.TextSize;
				measuredSize.Width	= Math.Max(minimumSize.Width, measuredSize.Width);
				measuredSize.Height = Math.Max(minimumSize.Height, measuredSize.Height);
				return measuredSize;
			} else
			{
				var measuredSize = new SizeF(GraphConstants.MinimumItemWidth, GraphConstants.MinimumItemHeight);
				measuredSize.Width  = Math.Max(minimumSize.Width, measuredSize.Width);
				measuredSize.Height = Math.Max(minimumSize.Height, measuredSize.Height);
				return measuredSize;
			}
		}

		public override void Render(Graphics graphics, SizeF minimumSize, NodeLabelItem item, PointF location)
		{
			var size = Measure(graphics, minimumSize, item);
			if (item.Input.Enabled != item.Output.Enabled)
			{
				if (item.Input.Enabled)
					graphics.DrawString(item.Text, SystemFonts.MenuFont, Brushes.Black, new RectangleF(location, size), GraphConstants.LeftTextStringFormat);
				else
					graphics.DrawString(item.Text, SystemFonts.MenuFont, Brushes.Black, new RectangleF(location, size), GraphConstants.RightTextStringFormat);
			} else
				graphics.DrawString(item.Text, SystemFonts.MenuFont, Brushes.Black, new RectangleF(location, size), GraphConstants.CenterTextStringFormat);
		}
	}
}

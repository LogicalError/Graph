using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Graph.Items
{
	internal sealed class NodeTitleItem : NodeItem
	{
		#region Text
		string			internalTitle = string.Empty;
		public string	Title		
		{
			get { return internalTitle; }
			set
			{
				if (internalTitle == value)
					return;
				internalTitle = value;
				ForceResize();
			}
		}
		#endregion

		internal void ForceResize() { TextSize = Size.Empty; }
		internal SizeF				TextSize;
	}

	[NodeItemDescription(typeof(NodeTitleItem))]
	internal class NodeTitleRenderer : NodeItemRenderer<NodeTitleItem>
	{
		public override SizeF Measure(IDeviceContext context, SizeF minimumSize, NodeTitleItem item)
		{
			if (!string.IsNullOrWhiteSpace(item.Title))
			{
				if (item.TextSize.IsEmpty)
				{
					var size = new Size(GraphConstants.MinimumItemWidth, GraphConstants.TitleHeight);
					item.TextSize			= TextRenderer.MeasureText(context, item.Title, SystemFonts.CaptionFont, size, GraphConstants.TitleTextFlags);

					item.TextSize.Width		= Math.Max(size.Width,  item.TextSize.Width + (GraphConstants.CornerSize * 2));
					item.TextSize.Height	= Math.Max(size.Height, item.TextSize.Height) + GraphConstants.TopHeight;
				}
				var measuredSize = item.TextSize;
				measuredSize.Width	= Math.Max(minimumSize.Width,  measuredSize.Width);
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

		public override void Render(Graphics graphics, SizeF minimumSize, NodeTitleItem item, PointF location)
		{
			var size = Measure(graphics, minimumSize, item);
			size.Height -= GraphConstants.TopHeight;
			location.Y += GraphConstants.TopHeight;
			graphics.DrawString(item.Title, SystemFonts.CaptionFont, Brushes.Black, new RectangleF(location, size), GraphConstants.TitleStringFormat);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Graph.Items
{
	public sealed class NodeCheckboxItem : NodeItem
	{
		public NodeCheckboxItem(string text, bool inputEnabled, bool outputEnabled) :
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

		#region Checked
		bool internalChecked = false;
		public bool Checked
		{
			get { return internalChecked; }
			set
			{
				if (internalChecked == value)
					return;
				internalChecked = value;
				TextSize = Size.Empty;
			}
		}
		#endregion

		#region Hover
		internal bool Hover { get; set; }
		#endregion

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
			Checked = !Checked;
			return true;
		}

		internal SizeF TextSize;
	}

	[NodeItemDescription(typeof(NodeCheckboxItem))]
	public class NodeCheckboxRenderer : NodeItemRenderer<NodeCheckboxItem>
	{
		public override SizeF Measure(IDeviceContext context, SizeF minimumSize, NodeCheckboxItem item)
		{
			if (!string.IsNullOrWhiteSpace(item.Text))
			{
				if (item.TextSize.IsEmpty)
				{
					var size = new Size(GraphConstants.MinimumItemWidth, GraphConstants.MinimumItemHeight);

					item.TextSize = TextRenderer.MeasureText(context, item.Text, SystemFonts.MenuFont, size, GraphConstants.CenterTextFlags);

					item.TextSize.Width = Math.Max(size.Width, item.TextSize.Width);
					item.TextSize.Height = Math.Max(size.Height, item.TextSize.Height);
				}

				var measuredSize = item.TextSize;
				measuredSize.Width  = Math.Max(minimumSize.Width, measuredSize.Width);
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

		public override void Render(Graphics graphics, SizeF minimumSize, NodeCheckboxItem item, PointF location)
		{
			var size = Measure(graphics, minimumSize, item);
			
			using (var path = NodeUtility.CreateRoundedRectangle(size, location))
			{
				var rect = new RectangleF(location, size);
				if (item.Checked)
				{
					using (var brush = new SolidBrush(Color.FromArgb(128+32, Color.White)))
					{
						graphics.FillPath(brush, path);
					}
				} else
				{
					using (var brush = new SolidBrush(Color.FromArgb(64, Color.Black)))
					{
						graphics.FillPath(brush, path);
					}
				}
				graphics.DrawString(item.Text, SystemFonts.MenuFont, Brushes.Black, rect, GraphConstants.CenterTextStringFormat);

				if (item.Hover)
					graphics.DrawPath(Pens.White, path);
				else	
					graphics.DrawPath(Pens.Black, path);
			}
		}
	}
}

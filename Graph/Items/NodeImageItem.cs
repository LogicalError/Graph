using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Graph.Items
{
	public sealed class NodeImageItem : NodeItem
	{
		public event EventHandler<NodeItemEventArgs> Clicked;

		public NodeImageItem(Image image, bool inputEnabled = false, bool outputEnabled = false) :
			base(inputEnabled, outputEnabled)
		{
			this.Image = image;
		}

		public NodeImageItem(Image image, int width, int height, bool inputEnabled = false, bool outputEnabled = false) :
			base(inputEnabled, outputEnabled)
		{
			this.Width = width;
			this.Height = height;
			this.Image = image;
		}

		public int? Width { get; set; }
		public int? Height { get; set; }
		public Image Image { get; set; }
		internal bool Hover { get; set; }

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

	[NodeItemDescription(typeof(NodeImageItem))]
	public class NodeImageRenderer : NodeItemRenderer<NodeImageItem>
	{
		public override SizeF Measure(IDeviceContext context, SizeF minimumSize, NodeImageItem item)
		{
			if (item.Image != null)
			{
				SizeF size = new Size(GraphConstants.MinimumItemWidth, GraphConstants.MinimumItemHeight);

				if (item.Width.HasValue)
					size.Width = Math.Max(size.Width, item.Width.Value + 2);
				else
					size.Width = Math.Max(size.Width, item.Image.Width + 2);

				if (item.Height.HasValue)
					size.Height = Math.Max(size.Height, item.Height.Value + 2);
				else
					size.Height = Math.Max(size.Height, item.Image.Height + 2);
				
				var measuredSize = size;
				measuredSize.Width  = Math.Max(minimumSize.Width, measuredSize.Width);
				measuredSize.Height = Math.Max(minimumSize.Height, measuredSize.Height);
				return measuredSize;
			} else
			{
				var size = new SizeF(GraphConstants.MinimumItemWidth, GraphConstants.MinimumItemHeight);
				if (item.Width.HasValue)
					size.Width = Math.Max(size.Width, item.Width.Value + 2);

				if (item.Height.HasValue)
					size.Height = Math.Max(size.Height, item.Height.Value + 2);
				
				var measuredSize = size;
				measuredSize.Width  = Math.Max(minimumSize.Width, measuredSize.Width);
				measuredSize.Height = Math.Max(minimumSize.Height, measuredSize.Height);
				return measuredSize;
			}
		}

		public override void Render(Graphics graphics, SizeF minimumSize, NodeImageItem item, PointF location)
		{
			var size = Measure(graphics, minimumSize, item);

			if (item.Width.HasValue &&
				size.Width > item.Width.Value)
			{
				location.X += (size.Width - (item.Width.Value + 2)) / 2.0f;
				size.Width = (item.Width.Value + 2);
			}
			var rect = new RectangleF(location, size);

			if (item.Image != null)
			{
				rect.Width -= 2;
				rect.Height -= 2;
				rect.X++;
				rect.Y++;
				graphics.DrawImage(item.Image, rect);
				rect.Width += 2;
				rect.Height += 2;
				rect.X--;
				rect.Y--;
			}

			if (item.Hover)
				graphics.DrawRectangle(Pens.White, rect.Left, rect.Top, rect.Width, rect.Height);
			else
				graphics.DrawRectangle(Pens.Black, rect.Left, rect.Top, rect.Width, rect.Height);
		}
	}
}

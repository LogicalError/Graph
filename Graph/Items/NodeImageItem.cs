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


		internal override SizeF Measure(IDeviceContext context)
		{
			if (this.Image != null)
			{
				SizeF size = new Size(GraphConstants.MinimumItemWidth, GraphConstants.MinimumItemHeight);

				if (this.Width.HasValue)
					size.Width = Math.Max(size.Width, this.Width.Value + 2);
				else
					size.Width = Math.Max(size.Width, this.Image.Width + 2);

				if (this.Height.HasValue)
					size.Height = Math.Max(size.Height, this.Height.Value + 2);
				else
					size.Height = Math.Max(size.Height, this.Image.Height + 2);
				
				return size;
			} else
			{
				var size = new SizeF(GraphConstants.MinimumItemWidth, GraphConstants.MinimumItemHeight);
				if (this.Width.HasValue)
					size.Width = Math.Max(size.Width, this.Width.Value + 2);

				if (this.Height.HasValue)
					size.Height = Math.Max(size.Height, this.Height.Value + 2);
				
				return size;
			}
		}

		internal override void Render(Graphics graphics, SizeF minimumSize, PointF location)
		{
			var size = Measure(graphics);
			size.Width  = Math.Max(minimumSize.Width, size.Width);
			size.Height = Math.Max(minimumSize.Height, size.Height);

			if (this.Width.HasValue &&
				size.Width > this.Width.Value)
			{
				location.X += (size.Width - (this.Width.Value + 2)) / 2.0f;
				size.Width = (this.Width.Value + 2);
			}
			var rect = new RectangleF(location, size);

			if (this.Image != null)
			{
				rect.Width -= 2;
				rect.Height -= 2;
				rect.X++;
				rect.Y++;
				graphics.DrawImage(this.Image, rect);
				rect.Width += 2;
				rect.Height += 2;
				rect.X--;
				rect.Y--;
			}

			if (this.Hover)
				graphics.DrawRectangle(Pens.White, rect.Left, rect.Top, rect.Width, rect.Height);
			else
				graphics.DrawRectangle(Pens.Black, rect.Left, rect.Top, rect.Width, rect.Height);
		}
	}
}

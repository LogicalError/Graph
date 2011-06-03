using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel.Composition;

namespace Graph
{
	[InheritedExport]
	public abstract class NodeItemRenderer<T> : INodeItemRenderer
		where T : NodeItem
	{
		public abstract SizeF Measure(IDeviceContext context, SizeF minimumSize, T item);
		public abstract void Render(Graphics graphics, SizeF minimumSize, T item, PointF location);

		SizeF INodeItemRenderer.Measure(IDeviceContext context, SizeF minimumSize, NodeItem item) { return Measure(context, minimumSize, item as T); }
		void INodeItemRenderer.Render(Graphics graphics, SizeF minimumSize, NodeItem item, PointF location) { Render(graphics, minimumSize, item as T, location); }
	}
}

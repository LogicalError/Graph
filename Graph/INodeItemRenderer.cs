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
	public interface INodeItemRenderer
	{
		SizeF	Measure(IDeviceContext context, SizeF minimumSize, NodeItem item);
		void	Render(Graphics graphics, SizeF minimumSize, NodeItem item, PointF position);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Graph
{
	[Flags]
	public enum RenderState
	{
		None		= 0,
		Connected	= 1,
		Hover		= 2,
		Dragging	= 4,
		Focus		= 8
	}
}

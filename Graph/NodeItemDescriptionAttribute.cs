using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace Graph
{
	[MetadataAttribute]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class NodeItemDescriptionAttribute : ExportAttribute, INodeItemDescription
	{
		public NodeItemDescriptionAttribute(Type nodeItemType) 
			: base(typeof(INodeItemRenderer)) 
		{
			ItemType = nodeItemType;
		}

		public Type ItemType { get; set; }
	}
}

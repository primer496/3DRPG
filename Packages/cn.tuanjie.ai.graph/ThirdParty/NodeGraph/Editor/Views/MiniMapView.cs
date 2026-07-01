using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor
{
	public class MiniMapView : MiniMap
	{
		new BaseGraphView	graphView;
		Vector2				size;

		public MiniMapView(BaseGraphView baseGraphView) : base()
		{
			// ！！！基类MiniMap有bug：当小地图中有元素底边超出maxHeight，或右边超出maxWidth时，该元素的显示位置会出错
			// MiniMap实现在Unity源码里，不好修改，我们暂时保留这个bug
			graphView = baseGraphView;
			maxHeight = 100;
			var label = this.Q<Label>();
			label.style.alignSelf = Align.Center;
			SetPosition(new Rect(0, 30, maxWidth, maxHeight));
		}

		public void ResizeMaxRect(Rect rect)
		{
			maxHeight = rect.height / 10;
			maxWidth = rect.width / 10;
		}
	}
}
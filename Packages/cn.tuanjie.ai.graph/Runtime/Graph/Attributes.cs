using System;

namespace UnityEngine.AIGraph
{
	/// <summary>
	/// Tell that this node class uses asynchronous process method
    /// 为触发型节点设计的异步执行属性，执行时将会调用节点的ProcessAsync函数   
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class UseProcessAsyncAttribute : Attribute { }
	
	/// <summary>
	/// Preview对应的field是否需要隐藏在PreviewContainer中的selector显示，selector显示用于定位资产
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public sealed class HideInPreviewSelector : Attribute
	{
	}
}

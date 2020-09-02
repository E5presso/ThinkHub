using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Collection
{
	/// <summary>
	/// 트리 자료구조를 구현한 클래스입니다.
	/// </summary>
	/// <typeparam name="TName">노드의 키를 정의합니다.</typeparam>
	/// <typeparam name="TValue">노드의 값을 정의합니다.</typeparam>
	public class TreeNode<TName, TValue>
	{
		private readonly Dictionary<TName, TreeNode<TName, TValue>> nodes;

		/// <summary>
		/// 노드의 값을 가져오거나 설정합니다.
		/// </summary>
		public TValue Value { get; set; }
		/// <summary>
		/// 포함된 노드의 갯수를 반환합니다.
		/// </summary>
		public int Count => nodes.Count;

		/// <summary>
		/// 노드를 가져오거나 설정합니다.
		/// </summary>
		/// <param name="name">노드의 이름을 지정합니다.</param>
		/// <returns>지정한 이름에 해당하는 노드를 반환합니다.</returns>
		public TreeNode<TName, TValue> this[TName name]
		{
			get
			{
				if (nodes.ContainsKey(name))
				{
					return nodes[name];
				}
				else
				{
					throw new ArgumentException("존재하지 않는 이름입니다.");
				}
			}
			set
			{
				if (nodes.ContainsKey(name))
				{
					nodes[name] = value;
				}
				else
				{
					throw new ArgumentException("존재하지 않는 이름입니다.");
				}
			}
		}

		/// <summary>
		/// TreeNode 클래스를 초기화합니다.
		/// </summary>
		public TreeNode()
		{
			nodes = new Dictionary<TName, TreeNode<TName, TValue>>();
		}
		/// <summary>
		/// TreeNode 클래스를 초기화합니다.
		/// </summary>
		/// <param name="value">노드의 값을 지정합니다.</param>
		public TreeNode(TValue value)
		{
			nodes = new Dictionary<TName, TreeNode<TName, TValue>>();
			Value = value;
		}

		/// <summary>
		/// 노드의 존재여부를 확인합니다.
		/// </summary>
		/// <param name="name">노드의 이름을 지정합니다.</param>
		/// <returns>노드의 존재여부를 반환합니다.</returns>
		public bool IsExists(TName name)
		{
			return nodes.ContainsKey(name);
		}
		/// <summary>
		/// 노드를 추가합니다.
		/// </summary>
		/// <param name="name">추가하려는 노드의 이름을 지정합니다.</param>
		/// <param name="node">추가하려는 노드를 지정합니다.</param>
		public void Add(TName name, TreeNode<TName, TValue> node)
		{
			if (!nodes.ContainsKey(name))
			{
				nodes.Add(name, node);
			}
			else
			{
				throw new ArgumentException("이미 존재하는 이름입니다.");
			}
		}
		/// <summary>
		/// 노드를 제거합니다.
		/// </summary>
		/// <param name="name">제거하려는 노드의 이름을 지정합니다.</param>
		public void Remove(TName name)
		{
			if (!nodes.ContainsKey(name))
			{
				throw new ArgumentException("존재하지 않는 이름입니다.");
			}
			else
			{
				nodes.Remove(name);
			}
		}
		/// <summary>
		/// 속해있는 하위노드 전체를 가져옵니다.
		/// </summary>
		/// <returns>하위노드 배열을 반환합니다.</returns>
		public TreeNode<TName, TValue>[] GetNodes()
		{
			return nodes.Values.ToArray();
		}
		/// <summary>
		/// 속해있는 하위노드 전체의 값을 가져옵니다.
		/// </summary>
		/// <returns>하위노드의 값 배열을 반환합니다.</returns>
		public TValue[] GetNodeValues()
		{
			return (from names in nodes.Values
					select names.Value).ToArray();
		}
	}
}
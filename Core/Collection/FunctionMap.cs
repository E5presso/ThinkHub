using System;
using System.Collections.Generic;

namespace Core.Collection
{
	/// <summary>
	/// 지정한 키와 수행할 함수의 매핑을 수행하는 클래스입니다.
	/// </summary>
	/// <typeparam name="Tkey">매핑할 키의 형식을 지정합니다.</typeparam>
	/// <typeparam name="Targ1">함수의 첫번째 인자 형식을 지정합니다.</typeparam>
	/// <typeparam name="Targ2">함수의 두번째 인자 형식을 지정합니다.</typeparam>
	public class FunctionMap<Tkey, Targ1, Targ2>
	{
		private readonly Dictionary<Tkey, Action<Targ1, Targ2>> map;

		/// <summary>
		/// FunctionMap 클래스를 초기화합니다.
		/// </summary>
		public FunctionMap()
		{
			map = new Dictionary<Tkey, Action<Targ1, Targ2>>();
		}

		/// <summary>
		/// 지정한 키에 대해 수행할 함수를 추가합니다.
		/// </summary>
		/// <param name="key">매핑할 키를 지정합니다.</param>
		/// <param name="action">수행할 함수를 지정합니다.</param>
		/// <returns>등록 결과를 반환합니다.</returns>
		public bool Add(Tkey key, Action<Targ1, Targ2> action)
		{
			try
			{
				map.Add(key, action);
				return true;
			}
			catch { return false; }
		}
		/// <summary>
		/// 지정한 키에 대해 등록된 함수를 호출합니다.
		/// </summary>
		/// <param name="key">매핑할 키를 지정합니다.</param>
		/// <param name="arg1">첫번째 인수를 지정합니다.</param>
		/// <param name="arg2">두번째 인수를 지정합니다.</param>
		/// <returns>함수의 수행 결과를 반환합니다.</returns>
		public bool Call(Tkey key, Targ1 arg1, Targ2 arg2)
		{
			try
			{
				map[key].Invoke(arg1, arg2);
				return true;
			}
			catch (Exception e) { throw e; }
		}
		/// <summary>
		/// 지정한 키에 대해 등록된 함수를 제거합니다.
		/// </summary>
		/// <param name="key">매핑할 키를 지정합니다.</param>
		/// <returns>제거 결과를 반환합니다.</returns>
		public bool Remove(Tkey key)
		{
			try
			{
				map.Remove(key);
				return true;
			}
			catch { return false; }
		}
		/// <summary>
		/// 이벤트 핸들러에 등록된 함수를 모두 제거합니다.
		/// </summary>
		public void Clear()
		{
			map.Clear();
		}
	}
}
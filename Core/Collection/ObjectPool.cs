using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Collection
{
	internal enum PoolStatus { Idle, Using }
	internal class PoolObject<T> where T : new()
	{
		public int ObjectId;
		public PoolStatus Status;
		public T Object;

		public PoolObject(int id)
		{
			ObjectId = id;
			Status = PoolStatus.Idle;
			Object = new T();
		}
	}
	/// <summary>
	/// 오브젝트 풀링을 구현한 클래스입니다.
	/// </summary>
	/// <typeparam name="T">풀링할 데이터의 형식을 지정합니다.</typeparam>
	public class ObjectPool<T> where T : new()
	{
		private readonly List<PoolObject<T>> pool = new List<PoolObject<T>>();
		private readonly string arg1;
		private readonly Action<T, string> initializer;
		private readonly Action<T> destructor;

		/// <summary>
		/// 오브젝트 풀의 크기를 가져옵니다.
		/// </summary>
		public int PoolSize => pool.Count;

		/// <summary>
		/// ObjectPool 클래스를 초기화합니다.
		/// </summary>
		/// <param name="initialSize">풀의 초기 크기를 지정합니다.</param>
		/// <param name="arg1">초기화할 객체의 첫번째 인자를 지정합니다.</param>
		/// <param name="initializer">객체의 초기화 수행 메서드를 지정합니다.</param>
		/// <param name="destructor">객체의 제거 시 수행할 메서드를 지정합니다.</param>
		public ObjectPool(int initialSize, string arg1, Action<T, string> initializer, Action<T> destructor)
		{
			this.arg1 = arg1;
			this.initializer = initializer;
			this.destructor = destructor;
			for (int i = 0; i < initialSize; i++)
			{
				pool.Add(new PoolObject<T>(i));
				initializer(pool[i].Object, arg1);
			}
		}
		/// <summary>
		/// 유휴 객체를 가져옵니다.
		/// </summary>
		/// <returns>사용 가능한 객체와 그 아이디를 반환합니다.</returns>
		public (int, T) GetObject()
		{
			foreach (PoolObject<T> obj in pool)
			{
				if (obj.Status == PoolStatus.Idle)
				{
					obj.Status = PoolStatus.Using;
					return (obj.ObjectId, obj.Object);
				}
			}
			int objId = PoolSize;
			PoolObject<T> data = new PoolObject<T>(objId)
			{
				Status = PoolStatus.Using
			};
			initializer(data.Object, arg1);
			return (objId, data.Object);
		}
		/// <summary>
		/// 사용한 객체를 반환합니다.
		/// </summary>
		/// <param name="id">객체의 아이디를 지정합니다.</param>
		/// <param name="obj">반환할 객체를 지정합니다.</param>
		public void ReturnObject(int id, T obj)
		{
			pool[id].Object = obj;
			pool[id].Status = PoolStatus.Idle;
		}
		/// <summary>
		/// 풀을 정리합니다.
		/// </summary>
		public void Clear()
		{
			Parallel.ForEach(pool, (obj) =>
			{
				destructor(obj.Object);
			});
			pool.Clear();
		}
	}
}
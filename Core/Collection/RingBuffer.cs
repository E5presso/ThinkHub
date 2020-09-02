using System;

namespace Core.Collection
{
	/// <summary>
	/// FIFO의 원형 버퍼를 구현합니다.
	/// </summary>
	/// <typeparam name="T">데이터의 타입을 지정합니다.</typeparam>
	public class RingBuffer<T>
	{
		private static readonly T[] initialArray = new T[0];

		private T[] array;
		private int head;
		private int tail;

		private const int minimumGrow = 4;
		private const int growFactor = 200;

		/// <summary>
		/// 버퍼에 저장된 데이터의 크기를 가져옵니다.
		/// </summary>
		public int Count { get; private set; }

		/// <summary>
		/// RingBuffer 클래스를 초기화합니다.
		/// </summary>
		public RingBuffer()
		{
			array = initialArray;
		}
		/// <summary>
		/// 최초 크기를 이용하여 RingBuffer 클래스를 초기화합니다.
		/// </summary>
		/// <param name="capacity">버퍼의 최초 크기를 지정합니다.</param>
		public RingBuffer(int capacity)
		{
			if (capacity < 0)
			{
				throw new ArgumentException("인수는 0보다 작을 수 없습니다.");
			}

			array = new T[capacity];
			head = 0;
			tail = 0;
			Count = 0;
		}

		/// <summary>
		/// 버퍼에 지정한 데이터를 기록합니다.
		/// </summary>
		/// <param name="item">버퍼에 기록할 데이터를 지정합니다.</param>
		public void Write(T item)
		{
			if (Count == array.Length)
			{
				int newcapacity = array.Length * growFactor / 100;
				if (newcapacity < array.Length + minimumGrow)
				{
					newcapacity = array.Length + minimumGrow;
				}

				SetCapacity(newcapacity);
			}
			array[tail] = item;
			tail = (tail + 1) % array.Length;
			Count++;
		}
		/// <summary>
		/// 버퍼에 지정한 데이터 블록을 기록합니다.
		/// </summary>
		/// <param name="item">버퍼에 기록할 데이터 블록을 지정합니다.</param>
		public void Write(T[] item)
		{
			int length = item.Length;
			if (Count + length > array.Length)
			{
				int newcapacity = array.Length > length ?
					array.Length * growFactor / 100 :
					length * growFactor / 100;
				if (newcapacity < array.Length + minimumGrow)
				{
					newcapacity = array.Length + minimumGrow;
				}

				SetCapacity(newcapacity);
			}
			if (head > tail || length <= array.Length - tail)
			{
				Array.Copy(item, 0, array, tail, length);
			}
			else
			{
				Array.Copy(item, 0, array, tail, array.Length - tail);
				Array.Copy(item, array.Length - tail, array, 0, length - (array.Length - tail));
			}
			tail = (tail + length) % array.Length;
			Count += length;
		}
		/// <summary>
		/// 버퍼에 지정한 데이터 블록을 기록합니다.
		/// </summary>
		/// <param name="item">버퍼에 기록할 데이터 블록을 지정합니다.</param>
		/// <param name="offset">블록의 시작 오프셋을 지정합니다.</param>
		/// <param name="length">기록할 데이터의 길이를 지정합니다.</param>
		public void Write(T[] item, int offset, int length)
		{
			if (offset + length > item.Length)
			{
				throw new InvalidOperationException("배열의 크기가 지정한 길이보다 짧습니다.");
			}

			if (Count + length > array.Length)
			{
				int newcapacity = array.Length > length ?
					array.Length * growFactor / 100 :
					length * growFactor / 100;
				if (newcapacity < array.Length + minimumGrow)
				{
					newcapacity = array.Length + minimumGrow;
				}

				SetCapacity(newcapacity);
			}
			if (head > tail || length <= array.Length - tail)
			{
				Array.Copy(item, offset, array, tail, length);
			}
			else
			{
				Array.Copy(item, offset, array, tail, array.Length - tail);
				Array.Copy(item, offset + array.Length - tail, array, 0, length - (array.Length - tail));
			}
			tail = (tail + length) % array.Length;
			Count += length;
		}

		/// <summary>
		/// 버퍼에서 데이터를 제거하고 읽어옵니다.
		/// </summary>
		/// <returns>버퍼에서 읽어온 데이터입니다.</returns>
		public T Read()
		{
			if (Count == 0)
			{
				throw new InvalidOperationException("버퍼가 비어있습니다.");
			}

			T removed = array[head];
			array[head] = default;
			head = (head + 1) % array.Length;
			Count--;
			return removed;
		}
		/// <summary>
		/// 버퍼에서 지정한 크기만큼 데이터를 제거하고 읽어옵니다.
		/// </summary>
		/// <param name="length">읽어올 데이터의 크기를 지정합니다.</param>
		/// <returns>버퍼에서 읽어온 데이터입니다.</returns>
		public T[] Read(int length)
		{
			if (length <= 0)
			{
				throw new ArgumentException("길이는 0보다 작거나 같을 수 없습니다.");
			}

			if (Count < length)
			{
				throw new InvalidOperationException("버퍼의 크기가 지정한 길이보다 짧습니다.");
			}

			T[] result = new T[length];
			if (head < tail || length <= array.Length - head)
			{
				Array.Copy(array, head, result, 0, length);
			}
			else
			{
				Array.Copy(array, head, result, 0, array.Length - head);
				Array.Copy(array, 0, result, array.Length - head, length - (array.Length - head));
			}
			head = (head + length) % array.Length;
			Count -= length;
			return result;
		}

		/// <summary>
		/// 버퍼에 저장된 데이터를 제거하고 읽어옵니다.
		/// </summary>
		/// <returns></returns>
		public T[] Flush()
		{
			if (Count == 0)
			{
				throw new InvalidOperationException("버퍼가 비어있습니다.");
			}

			T[] result = new T[Count];
			if (head < tail)
			{
				Array.Copy(array, head, result, 0, Count);
			}
			else
			{
				Array.Copy(array, head, result, 0, array.Length - head);
				Array.Copy(array, 0, result, array.Length - head, tail);
			}
			head = 0;
			tail = 0;
			Count = 0;
			return result;
		}
		/// <summary>
		/// 버퍼를 배열로 변환합니다.
		/// </summary>
		/// <returns>변환된 배열입니다.</returns>
		public T[] ToArray()
		{
			T[] arr = new T[Count];
			if (Count == 0)
			{
				return arr;
			}

			if (head < tail)
			{
				Array.Copy(array, head, arr, 0, Count);
			}
			else
			{
				Array.Copy(array, head, arr, 0, array.Length - head);
				Array.Copy(array, 0, arr, array.Length - head, tail);
			}
			return arr;
		}

		/// <summary>
		/// 버퍼에 기록된 데이터를 지웁니다.
		/// </summary>
		public void Clear()
		{
			head = 0;
			tail = 0;
			Count = 0;
		}

		private void SetCapacity(int capacity)
		{
			T[] newarray = new T[capacity];
			if (Count > 0)
			{
				if (head < tail)
				{
					Array.Copy(array, head, newarray, 0, Count);
				}
				else
				{
					Array.Copy(array, head, newarray, 0, array.Length - head);
					Array.Copy(array, 0, newarray, array.Length - head, tail);
				}
			}
			array = newarray;
			head = 0;
			tail = (Count == capacity) ? 0 : Count;
		}
	}
}
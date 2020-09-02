using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Data
{
	/// <summary>
	/// 수학 관련 함수를 정의한 클래스입니다.
	/// </summary>
	public class Mathematics
	{
		/// <summary>
		/// 두 수의 최대 공약수를 구합니다.
		/// </summary>
		/// <param name="x">첫번째 숫자를 지정합니다.</param>
		/// <param name="y">두번째 숫자를 지정합니다.</param>
		/// <returns>두 수의 최대공약수를 반환합니다.</returns>
		public static int GCD(int x, int y)
		{
			return y == 0 ? x : GCD(y, x % y);
		}
	}
}
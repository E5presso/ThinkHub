using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Core.Utility
{
	/// <summary>
	/// 개체의 Json 직렬화/역직렬화를 정의합니다.
	/// </summary>
	public static class JsonSerializer
	{
		/// <summary>
		/// 개체를 Json 문자열로 직렬화합니다.
		/// </summary>
		/// <param name="value">직렬화를 수행할 개체를 지정합니다.</param>
		/// <returns>직렬화된 Json 문자열입니다.</returns>
		public static string Serialize(object value)
		{
			return JsonConvert.SerializeObject(value);
		}
		/// <summary>
		/// Json 문자열을 역직렬화합니다.
		/// </summary>
		/// <typeparam name="T">역직렬화의 대상 개체타입을 지정합니다.</typeparam>
		/// <param name="json">역직렬화를 수행할 Json 문자열을 지정합니다.</param>
		/// <returns>역직렬화된 개체입니다.</returns>
		public static T Deserialize<T>(string json)
		{
			return JsonConvert.DeserializeObject<T>(json);
		}
		/// <summary>
		/// Json 문자열을 역직렬화합니다.
		/// </summary>
		/// <param name="json">역직렬화를 수행할 Json 문자열을 지정합니다.</param>
		/// <returns>역직렬화된 개체입니다.</returns>
		public static dynamic Deserialize(string json)
		{
			try { return JObject.Parse(json); }
			catch
			{
				try { return JArray.Parse(json).Children<JObject>(); }
				catch { throw; }
			}
		}
	}
}
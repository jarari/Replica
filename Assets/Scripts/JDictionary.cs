using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class JDictionaryException: Exception {
	public JDictionaryException(string message = null) : base(message) { }
}

// 문자열 매개변수의 인덱서가 예외를 던지지 않도록 새로 구현해야하는데
// 인덱서는 오버라이딩이 안되기 때문에 키를 문자열이 아닌 정수형으로 대체 후 해쉬값을 키로 이용
// 해쉬값 충돌 시 인스턴스 내부의 collisions 필드에 따로 저장. 접근 시에는 이 인스턴스를 통해서 접근.
// foreach enumerator의 타입은 JDictionary
// 
// 직접 내부 collection 수정 X
// 강제로 Dictionary<int, JDictionary>로 형변환 해서 사용 X
public class JDictionary: Dictionary<int, JDictionary>, IEnumerable<JDictionary> {

	#region Fields

	private Dictionary<string, JDictionary> _collisions = null;

	private readonly string _key	= null;
	private int				_count	= 0;
	private JValue			_value	= null;
	private JArray			_array	= null;

	// For enumerating
	private JDictionary _first	= null;
	private JDictionary _next	= null;
	private JDictionary _last	= null;

	#endregion

	#region Properties

	/// <summary>
	/// 현재 JDictionary가 값 형식이면 true 반환.
	/// </summary>
	public bool IsValue {
		get {
			return (_value != null || _array != null);
		}
	}

	/// <summary>
	/// 현재 JDictionary의 property name(string key 값) 반환. 
	/// </summary>
	public string Key {
		get {
			return _key;
		}
	}

	public new int Count {
		get {
			return _count;
		}
	}

	#endregion

	#region Constructors

	/// <summary>
	/// 생성자.
	/// </summary>
	/// <param name="key">이름</param>
	public JDictionary(string key = null) {
		_key = key;
	}

	#endregion

	#region Utilities

	// JDictionary는 키가 정수형이기 때문에 문자열을 해쉬값으로 변환해서 접근함.
	/// <summary>
	/// 하위 JDictionary에 접근.
	/// </summary>
	/// <param name="key"></param>
	/// <returns></returns>
	public JDictionary this[string key] {
		get {
			if(this.IsValue)
				return null;

			int hashCode = key.GetHashCode();
			JDictionary subDict;

			// 하위 JDictionary에 접근
			bool subDictExist = this.TryGetValue(hashCode, out subDict);
			if(subDictExist) {
				// 전체 데이터가 가지고 있는 문자열 키 이외에
				// 전혀 엉뚱한 문자열의 해쉬값이 우연히 같을 수 있으므로 문자열 키를 비교
				if(subDict._key.Equals(key))
					return subDict;

				// 문자열 키가 다르지만
				// 현재 해쉬값을 가진 JDictionary가 충돌이 일어났던 JDictionary일 경우
				if(subDict._collisions != null) {
					// 해당 JDictionary의 collisions 필드에서 입력받은 문자열 키로 접근
					JDictionary subDictCollision;
					bool keyExist = subDict._collisions.TryGetValue(key, out subDictCollision);

					return (keyExist ? subDictCollision : null);
				}

				// 문자열 키가 다르고 collision도 안일어 난 경우
				return null;
			}
			else
				return null;
		}
	}

	// null check
	public static implicit operator bool(JDictionary jDict) {
		return jDict != null;
	}

	#endregion

	#region Functions

	/// <summary>
	/// JObject의 구조를 그대로 읽어서 JDictionary 형식으로 복사.
	/// </summary>
	/// <param name="jsonObj"></param>
	/// <returns></returns>
	private void Deserializer(JObject jsonObj) {
		foreach(KeyValuePair<string, JToken> current in jsonObj) {
			// JDictionary는 키가 정수형이기 때문에 현재 키를 해쉬값으로 변환
			int hashCodeKey = current.Key.GetHashCode();

			// 하위 JDictionary 생성
			JDictionary currentDict = new JDictionary(current.Key);
			_count++;

			// For enumerating
			// enumerator의 마지막이 null이면 하위 JDictionary가 처음 생성 되었다는 것이므로 enumerator의 첫번째 초기화
			// 아니라면 기존의 마지막 개체의 next필드를 이 개체로 설정.
			if(_last)
				_last._next = currentDict;
			else
				_first = currentDict;
			// 새로 생성된 JDictionary를 enumerator의 마지막 개체로 설정
			_last = currentDict;

			// 이미 해당 해쉬값이 이미 사용 중(collision)이라면
			JDictionary occupied;
			bool isOccupied = this.TryGetValue(hashCodeKey, out occupied);
			if(isOccupied) {
				Debug.LogWarning("Collision occured! " + occupied._key + " : " + current.Key + " [" + hashCodeKey + "]");
				// 해당 하위 인스턴스의 collisions 필드에 문자열 키로 저장
				if(occupied._collisions == null)
					occupied._collisions = new Dictionary<string, JDictionary>();

				occupied._collisions.Add(current.Key, currentDict);

				// 아예 같은 문자열 키(property)가 Json에 있을 때(해쉬값도 같고 문자열 키도 같음)에는 JObject.Parse 메서드에서 덮어쓰기 처리되어
				// 같은 키가 두 개가 생성되는게 아니라 한 개만 남게되므로 이 if문이 실행될 수 없다.
				// 즉 이 if문 내에서는 무조건 문자열 키가 다른 경우가 된다.
			}
			else {
				this.Add(current.Key.GetHashCode(), currentDict);
			}

			// 하위 property가 있는 token이라면
			if(current.Value.Type == JTokenType.Object) {
				// 현재 token을 JObject로 변환 후 생성한 JDictionary에서 재귀호출
				JObject currentJsonObj = current.Value.ToObject<JObject>();
				currentDict.Deserializer(currentJsonObj);
			}
			// 값을 가지고 있는 token이라면
			else {
				// 생성한 JDictionary의 value 필드를 설정. 배열의 경우 array 필드.
				if(current.Value.Type == JTokenType.Array) {
					currentDict._array = current.Value.ToObject<JArray>();
				}
				else {
					currentDict._value = current.Value.ToObject<JValue>();
				}
			}
		}
	}

	/// <summary>
	/// Json 파일을 JDictionary 형식으로 변환.
	/// </summary>
	/// <param name="paths">Json 파일 경로</param>
	public void DeserializeJson(params string[] paths) {
		foreach(string path in paths) {
			TextAsset jsonText = Resources.Load<TextAsset>(path);

			if(!jsonText)
				throw new JDictionaryException("Resources/" + path + ".json doesn't exist.");

			JObject jsonObj = JObject.Parse(jsonText.text);

			this.Deserializer(jsonObj);
		}
	}

	/// <summary>
	/// 현재 JDictionary의 값 반환. 배열 형식은 Array() 사용.
	/// </summary>
	/// <typeparam name="T">값 타입. Json 파일의 형식과 다를 경우 예외 발생.</typeparam>
	/// <returns></returns>
	public T Value<T>() {
		// 값 형식이 아닌 경우 예외 발생
		if(this.IsValue) {
			// 값이 배열일 경우 예외 발생
			if(_value == null)
				throw new JDictionaryException(_key + " property is array type value. try Array() instead.");

			Type type = typeof(T);
			// 입력받은 형식 T를 JTokenType으로 표시
			JTokenType valueType;
			switch(type.Name) {
				case "Int32":
					valueType = JTokenType.Integer;
					break;

				case "Single":
					valueType = JTokenType.Float;
					break;

				case "Boolean":
					valueType = JTokenType.Boolean;
					break;

				case "String":
					valueType = JTokenType.String;
					break;

				default:
					valueType = JTokenType.None;
					break;
			}

			// 입력받은 형식이 json의 형식과 다른경우 예외 발생
			// 단 정수형의 경우는 float형으로 출력 가능.
			if(_value.Type != valueType)
				if(_value.Type != JTokenType.Integer || valueType != JTokenType.Float)
					throw new JDictionaryException("value type doesn't match with given type. \"" + _key + "\" property is " + _value.Type + " type.");

			return _value.Value<T>();
		}
		else
			throw new JDictionaryException(_key + " property is not value type.");
	}

	/// <summary>
	/// 현재 JDictionary의 배열(JArray) 값 반환.
	/// </summary>
	/// <returns></returns>
	public JArray Array() {
		if(this.IsValue) {
			// 값이 배열이 아닐 경우 예외 발생
			if(_array == null)
				throw new JDictionaryException(_key + " property is not array type value.");

			return _array;
		}
		else
			throw new JDictionaryException(_key + " property is not value type.");
	}

	#endregion

	#region Enumerator

	public new class Enumerator: IEnumerator<JDictionary> {
		readonly JDictionary _sourceJDict = null;
		JDictionary _currentJDict = null;

		public JDictionary Current {
			get {
				return _currentJDict;
			}
		}

		public Enumerator(JDictionary jDict) {
			_sourceJDict = jDict;
		}

		public void Dispose() { }

		// true 반환하면 반복문 실행, false 반환하면 반복문 종료
		public bool MoveNext() {
			// 원소가 없으면 실행 종료
			if(_sourceJDict.Count <= 0)
				return false;

			// 반복문 처음 실행시
			if(!_currentJDict) {
				_currentJDict = _sourceJDict._first;
				return true;
			}
			else {
				JDictionary nextJDict = _currentJDict._next;
				if(nextJDict) {
					_currentJDict = nextJDict;
					return true;
				}
				else
					return false;
			}
		}

		public void Reset() {
			_currentJDict = null;
		}

		object IEnumerator.Current {
			get {
				return this.Current;
			}
		}
	}

	public new IEnumerator<JDictionary> GetEnumerator() {
		return new Enumerator(this);
	}

	#endregion

}
﻿public class HeaderSerializer : Serializer
{
	public bool Serialize(HeaderData data)
	{
		// 기존 데이터를 클리어한다.
		Clear();

		// 각 요소를 차례로 시리얼라이즈한다.
		bool ret = true;
		ret &= Serialize(data.length);
        ret &= Serialize(data.source);
        ret &= Serialize(data.id);

		if (ret == false)
		{
			return false;
		}

		return true;
	}

	public bool Deserialize(ref HeaderData serialized)
	{
		// 디시리얼라이즈하는 데이터를 설정한다.
		bool ret = (GetDataSize() > 0) ? true : false;

		if (ret == false)
		{
			return false;
		}

		// 데이터의 요소별로 디시리얼라이즈한다.
        byte source = 0;
		byte packetId = 0;
        ret &= Deserialize(ref source);
        ret &= Deserialize(ref packetId);
        serialized.source = source;
        serialized.id = packetId;

		return ret;
	}
}
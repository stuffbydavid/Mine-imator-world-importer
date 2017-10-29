using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace import
{
	public enum TagType
	{
		END = 0,
		BYTE = 1,
		SHORT = 2,
		INT = 3,
		LONG = 4,
		FLOAT = 5,
		DOUBLE = 6,
		BYTE_ARRAY = 7,
		STRING = 8,
		LIST = 9,
		COMPOUND = 10,
		INT_ARRAY = 11,
		LONG_ARRAY = 12,
	}

	public enum DataFormat
	{
		RAW,
		GZIP,
		ZLIB
	}

	/// <summary>A NBT tag</summary>
	public class NBTTag
	{
		public TagType type;
		public dynamic value;

		public NBTTag(TagType type, dynamic value)
		{
			this.type = type;
			this.value = value;
		}

		public virtual NBTTag Copy()
		{
			return new NBTTag(type, value);
		}
	}

	/// <summary>A NBT list</summary>
	public class NBTList : NBTTag
	{
		public TagType listType;

		public NBTList(TagType listType) : base(TagType.LIST, new List<NBTTag>())
		{
			this.listType = listType;
		}

		public void Add(dynamic val)
		{
			value.Add(val);
		}
		
		public int Length()
		{
			return value.Count;
		}

		public dynamic Get(int index)
		{
			return value[index];
		}

		public override NBTTag Copy()
		{
			NBTList newList = new NBTList(listType);
			foreach (NBTTag tag in value)
				newList.value.Add(tag.Copy());
			return newList;
		}
	}

	/// <summary>A NBT compound</summary>
	public class NBTCompound : NBTTag
	{
		public NBTCompound() : base(TagType.COMPOUND, new Dictionary<string, NBTTag>())
		{
		}

		public void AddTag(string name, NBTTag tag)
		{
			value[name] = tag;
		}

		public void Add(TagType type, string name, dynamic val)
		{
			value[name] = new NBTTag(type, val);
		}

		public dynamic Get(string name)
		{
			if (!value.ContainsKey(name))
				return null;

			return value[name];
		}

		public override NBTTag Copy()
		{
			NBTCompound newComp = new NBTCompound();
			foreach (KeyValuePair<string, NBTTag> key in value)
				newComp.AddTag(key.Key, key.Value.Copy());
			return newComp;
		}
	}

	/// <summary>Used for writing to the NBT format (Named Binary Tag) that Minecraft files use.</summary>
	public class NBTWriter
	{
		private string outFilename;
		private FileStream outStream;

		/// <summary>Writes a compound tag to the given NBT file</summary>
		/// <param name="filename">Path to write to.</param>
		/// <param name="rootName">Name of the root compound tag.</param>
		/// <param name="root">The root compound tag.</param>
		public void Save(string filename, string rootName, NBTCompound root)
		{
			outFilename = filename;
			outStream = File.Create(filename + ".nbt");
			WriteByte((byte)TagType.COMPOUND);
			WriteString(rootName);
			WriteTAGCompound(root);
			outStream.Close();
			GZIPCompressFile(filename + ".nbt", outFilename);
			File.Delete(filename + ".nbt");
		}

		// Write tags

		private void WriteTAG(NBTTag tag)
		{
			switch (tag.type)
			{
				case TagType.BYTE:
					WriteByte((byte)tag.value);
					break;

				case TagType.SHORT:
					WriteShortBE((short)tag.value);
					break;

				case TagType.INT:
					WriteIntBE((int)tag.value);
					break;

				case TagType.LONG:
					WriteLongBE((long)tag.value);
					break;

				case TagType.FLOAT:
					WriteFloatBE((float)tag.value);
					break;

				case TagType.DOUBLE:
					WriteDoubleBE((double)tag.value);
					break;

				case TagType.BYTE_ARRAY:
					byte[] byteArr = (byte[])tag.value;
					WriteIntBE(byteArr.Length);
					for (var i = 0; i < byteArr.Length; i++)
						WriteByte(byteArr[i]);
					break;

				case TagType.STRING:
					WriteString((string)tag.value);
					break;

				case TagType.LIST:
					WriteTAGList((NBTList)tag);
					break;

				case TagType.COMPOUND:
					WriteTAGCompound((NBTCompound)tag);
					break;

				case TagType.INT_ARRAY:
					int[] intArr = (int[])tag.value;
					WriteIntBE(intArr.Length);
					for (var i = 0; i < intArr.Length; i++)
						WriteIntBE(intArr[i]);
					break;

				case TagType.LONG_ARRAY:
					long[] longArr = (long[])tag.value;
					WriteIntBE(longArr.Length);
					for (var i = 0; i < longArr.Length; i++)
						WriteLongBE(longArr[i]);
					break;
			}
		}

		private void WriteTAGList(NBTList list)
		{
			WriteByte((byte)list.listType);
			WriteIntBE(list.Length());

			foreach (dynamic item in list.value)
				WriteTAG(item);
		}

		private void WriteTAGCompound(NBTCompound comp)
		{
			foreach (KeyValuePair<string, NBTTag> key in comp.value)
			{
				string name = key.Key;
				NBTTag tag = key.Value;

				WriteByte((byte)tag.type);
				WriteString(name);
				WriteTAG(tag);
			}

			WriteByte((byte)TagType.END);
		}

		// Write data

		private void WriteByte(byte val)
		{
			outStream.WriteByte(val);
		}

		private void WriteString(string val)
		{
			WriteShortBE((short)val.Length);
			for (int i = 0; i < val.Length; i++)
				outStream.WriteByte(Convert.ToByte(val[i]));
		}

		private void WriteShortBE(short val)
		{
			byte[] bytes = BitConverter.GetBytes(val);
			for (var b = 1; b >= 0; b--)
				outStream.WriteByte(bytes[b]);
		}

		private void WriteIntBE(int val)
		{
			byte[] bytes = BitConverter.GetBytes(val);
			for (var b = 3; b >= 0; b--)
				outStream.WriteByte(bytes[b]);
		}

		private void WriteLongBE(long val)
		{
			byte[] bytes = BitConverter.GetBytes(val);
			for (var b = 7; b >= 0; b--)
				outStream.WriteByte(bytes[b]);
		}

		private void WriteFloatBE(float val)
		{
			byte[] bytes = BitConverter.GetBytes(val);
			for (var b = 3; b >= 0; b--)
				outStream.WriteByte(bytes[b]);
		}

		private void WriteDoubleBE(double val)
		{
			byte[] bytes = BitConverter.GetBytes(val);
			for (var b = 7; b >= 0; b--)
				outStream.WriteByte(bytes[b]);
		}

		public void GZIPCompressFile(string src, string dest)
		{
			FileStream sourceFileStream = File.OpenRead(src);
			FileStream destFileStream = File.Create(dest);
			GZipStream compressingStream = new GZipStream(destFileStream, CompressionMode.Compress);
			byte[] bytes = new byte[2048];
			int bytesRead;
			while ((bytesRead = sourceFileStream.Read(bytes, 0, bytes.Length)) != 0)
				compressingStream.Write(bytes, 0, bytesRead);
			sourceFileStream.Close();
			compressingStream.Close();
			destFileStream.Close();
		}
	}

	/// <summary>Used for reading the NBT format (Named Binary Tag) that Minecraft files use.</summary>
	public class NBTReader
	{
		private int readPos;
		public byte[] data;

		/// <summary>Reads decompressed or raw NBT data and returns a compound tag.</summary>
		/// <param name="bytes">Bytes to read.</param>
		/// <param name="decompress">Decompression to use, where 0: None, 1: GZIP, 2: ZLIB</param>
		public NBTCompound Open(byte[] bytes, DataFormat format)
		{
			if (format == DataFormat.GZIP)
				data = GZIPDecompress(bytes);
			else if (format == DataFormat.ZLIB)
				data = ZLIBDecompress(bytes);
			else
				data = bytes;

			readPos = 0;

			// Read root
			NBTCompound root = new NBTCompound();
			TagType tagType = (TagType)ReadByte();
			string tagName = ReadString();
			return ReadTAGCompound();
		}

		// Read tags
		
		private NBTTag ReadTAG(TagType type)
		{
			dynamic value = null;

			switch (type)
			{
				case TagType.BYTE:
					value = ReadByte();
					break;

				case TagType.SHORT:
					value = ReadShortBE();
					break;

				case TagType.INT:
					value = ReadIntBE();
					break;

				case TagType.LONG:
					value = ReadLongBE();
					break;

				case TagType.FLOAT:
					value = ReadFloatBE();
					break;

				case TagType.DOUBLE:
					value = ReadDoubleBE();
					break;

				case TagType.BYTE_ARRAY:
					int byteArrLen = ReadIntBE();
					int byteArrPos = readPos;
					readPos += byteArrLen;
					value = byteArrPos;
					break;

				case TagType.STRING:
					value = ReadString();
					break;

				case TagType.LIST:
					return ReadTAGList();

				case TagType.COMPOUND:
					return ReadTAGCompound();

				case TagType.INT_ARRAY:
					int intArrLen = ReadIntBE();
					int intArrPos = readPos;
					readPos += intArrLen * 4;
					value = intArrPos;
					break;

				case TagType.LONG_ARRAY:
					int longArrLen = ReadIntBE();
					int longArrPos = readPos;
					readPos += longArrLen * 8;
					value = longArrPos;
					break;
			}

			return new NBTTag(type, value);
		}

		private NBTList ReadTAGList()
		{
			TagType listType = (TagType)ReadByte();
			int listLen = ReadIntBE();

			NBTList list = new NBTList(listType);
			for (var i = 0; i < listLen; i++)
				list.Add(ReadTAG(listType));

			return list;
		}

		private NBTCompound ReadTAGCompound()
		{
			NBTCompound comp = new NBTCompound();

			while (readPos < data.Length)
			{ 
				TagType tagType = (TagType)ReadByte();
				if (tagType == TagType.END)
					break;

				string tagName = ReadString();
				comp.value[tagName] = ReadTAG(tagType);
			}

			return comp;
		}

		// Read data

		private byte ReadByte()
		{
			if (readPos >= data.Length)
				return 0;
			byte r = data[readPos];
			readPos++;
			return r;
		}

		private short ReadShortBE()
		{
			byte[] r = new byte[2];
			for (int i = 0; i < 2; i++)
				r[1 - i] = ReadByte();
			return BitConverter.ToInt16(r, 0);
		}

		private int ReadIntBE()
		{
			byte[] r = new byte[4];
			for (int i = 0; i < 4; i++)
				r[3 - i] = ReadByte();
			return BitConverter.ToInt32(r, 0);
		}

		private long ReadLongBE()
		{
			byte[] r = new byte[8];
			for (int i = 0; i < 8; i++)
				r[7 - i] = ReadByte();
			return BitConverter.ToInt64(r, 0);
		}

		private float ReadFloatBE()
		{
			byte[] r = new byte[4];
			for (int i = 0; i < 4; i++)
				r[3 - i] = ReadByte();
			return BitConverter.ToSingle(r, 0);
		}

		private double ReadDoubleBE()
		{
			byte[] r = new byte[8];
			for (int i = 0; i < 8; i++)
				r[7 - i] = ReadByte();
			return BitConverter.ToDouble(r, 0);
		}

		private string ReadString()
		{
			short l = ReadShortBE();
			string s = "";
			for (int i = 0; i < l; i++)
				s += (char)(ReadByte());
			return s;
		}

		// Data formats

		public byte[] GZIPDecompress(byte[] bytes)
		{
			using (var ds = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress))
			{
				using (var ms = new MemoryStream())
				{
					ds.CopyTo(ms);
					return ms.ToArray();
				}
			}
		}

		public byte[] ZLIBDecompress(byte[] bytes)
		{
			using (var ds = new DeflateStream(new MemoryStream(bytes), CompressionMode.Decompress))
			{
				using (var ms = new MemoryStream())
				{
					ds.CopyTo(ms);
					return ms.ToArray();
				}
			}
		}
	}
}
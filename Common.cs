using System;
using System.Collections;
using System.IO;
using System.Text;

namespace JavaDeObfuscator
{
	// important enums
	enum AccessFlags
	{
		ACC_PUBLIC =	0x0001,	
		ACC_FINAL =		0x0010,
		ACC_SUPER =		0x0020, 	
		ACC_INTERFACE = 0x0200, 
		ACC_ABSTRACT =	0x0400
	}

	enum FieldAccessFlags
	{
		ACC_PUBLIC =	0x0001, 	// Declared public; may be accessed from outside its package.
		ACC_PRIVATE =	0x0002, 	// Declared private; usable only within the defining class.
		ACC_PROTECTED =	0x0004, 	// Declared protected; may be accessed within subclasses.
		ACC_STATIC =	0x0008, 	// Declared static.
		ACC_FINAL =		0x0010, 	// Declared final; no further assignment after initialization.
		ACC_VOLATILE =	0x0040, 	// Declared volatile; cannot be cached.
		ACC_TRANSIENT =	0x0080 		// Declared transient; not written or read by a persistent object manager.
	}

	enum ConstantPoolInfoTag 
	{
		ConstantClass = 7, 	
		ConstantFieldref = 9, 	
		ConstantMethodref = 10, 	
		ConstantInterfaceMethodref = 11, 	
		ConstantString = 8, 	
		ConstantInteger = 3, 	
		ConstantFloat = 4 ,	
		ConstantLong = 5, 	
		ConstantDouble = 6, 	
		ConstantNameAndType = 12, 	
		ConstantUtf8 = 1
	}

	struct AttributeType
	{
		public const string ConstantValue = "ConstantValue";
		public const string Code = "Code";
		public const string Exceptions = "Exceptions";
		public const string InnerClasses = "InnerClasses";
		public const string Synthetic = "Synthetic";
		public const string SourceFile = "SourceFile";
		public const string LineNumberTable = "LineNumberTable";
		public const string LocalVariableTable = "LocalVariableTable";
		public const string Deprecated = "Deprecated";
	}

	//  ********************************************************************************   //
	//  ************************* CONSTANT POOL STRUCTURES *****************************   //
	//  ********************************************************************************   //

	abstract class ConstantPoolInfo 
	{
		public byte Tag;
		public int References;
		public abstract bool Read(byte tag, BinaryReader Reader);
		public abstract bool Resolve(ArrayList FItems); 
	}
	abstract class ConstantPoolMethodInfo : ConstantPoolInfo
	{
		public ConstantClassInfo ParentClass;
		public ConstantNameAndTypeInfo NameAndType;
		//private ushort ClassIndex;
		//private ushort NameAndTypeIndex;

		public abstract void SetNameAndType(ushort Index, TConstantPool ConstantPool);
		public abstract void SetParent(ushort Index, TConstantPool ConstantPool);
	}
	abstract class ConstantPoolVariableInfo : ConstantPoolInfo
	{
		public Object Value;
	}
	class ConstantClassInfo : ConstantPoolInfo
	{
		public ushort NameIndex;
		public string Name;

		public ConstantClassInfo()
		{
			Name = "";
			NameIndex = 0;
			Tag = (byte) ConstantPoolInfoTag.ConstantClass;
			References = 0;
		}
		public override bool Read(byte tag, BinaryReader Reader)
		{
			try
			{
				Tag = tag;
				NameIndex = Common.ReadWord(Reader);
				NameIndex--;
				return true;
			}
			catch (Exception e)
			{
				return false;
			}
		}
		public void Write(BinaryWriter Writer)
		{
			// write the tag
			Common.WriteByte(Writer, Tag);
			Common.WriteWord(Writer, NameIndex + 1);
		}
		public override bool Resolve(ArrayList FItems)
		{
			// use the index into the constant pool table
			// to find our UTF8 encoded class or interface name
			if (NameIndex < FItems.Count)
			{
				Object o = FItems[NameIndex];
				if (o is ConstantUtf8Info)
				{
					Name = Encoding.UTF8.GetString(((ConstantUtf8Info)o).Bytes);
					((ConstantPoolInfo)o).References++;

					return true;
				}
			}
			
			return false;
		}
		public void SetName(ushort Index, TConstantPool ConstantPool)
		{
			NameIndex = Index;
			Name = ((ConstantUtf8Info)ConstantPool.Item(Index)).Value;
			References++;
		}
	}
	class ConstantFieldrefInfo : ConstantPoolMethodInfo 
	{
		private ushort ClassIndex;
		private ushort NameAndTypeIndex;

		public override bool Read(byte tag, BinaryReader Reader)
		{
			try
			{
				Tag = tag;
				ClassIndex = Common.ReadWord(Reader);
				NameAndTypeIndex = Common.ReadWord(Reader);
				ClassIndex--;
				NameAndTypeIndex--;
				return true;
			}
			catch (Exception e)
			{
				return false;
			}
		}
		public void Write(BinaryWriter Writer)
		{
			// write the tag
			Common.WriteByte(Writer, Tag);
			Common.WriteWord(Writer, ClassIndex + 1);
			Common.WriteWord(Writer, NameAndTypeIndex + 1);
		}
		public override bool Resolve(ArrayList FItems)
		{
			// use the index into the constant pool table
			// to find our UTF8 encoded class or interface name
			if (ClassIndex < FItems.Count && NameAndTypeIndex < FItems.Count)
			{
				Object o = FItems[ClassIndex];
				if (o is ConstantClassInfo)
				{
					ParentClass = (ConstantClassInfo) o;
					((ConstantPoolInfo)o).References++;
				}

				o = FItems[NameAndTypeIndex];
				if (o is ConstantNameAndTypeInfo)
				{
					NameAndType = (ConstantNameAndTypeInfo) o;
					((ConstantPoolInfo)o).References++;
				}

				return true;
			}
			
			return false;
		}
		public override void SetNameAndType(ushort Index, TConstantPool ConstantPool)
		{
			NameAndTypeIndex = Index;
			NameAndType = (ConstantNameAndTypeInfo)ConstantPool.Item(Index);
			NameAndType.References++;
		}
		public override void SetParent(ushort Index, TConstantPool ConstantPool)
		{
			ClassIndex = Index;
			ParentClass = (ConstantClassInfo)ConstantPool.Item(Index);
			ParentClass.References++;
		}
	}
	class ConstantMethodrefInfo : ConstantPoolMethodInfo 
	{
		private ushort ClassIndex;
		private ushort NameAndTypeIndex;

		public override bool Read(byte tag, BinaryReader Reader)
		{
			try
			{
				Tag = tag;
				ClassIndex = Common.ReadWord(Reader);
				NameAndTypeIndex = Common.ReadWord(Reader);
				ClassIndex--;
				NameAndTypeIndex--;
				return true;
			}
			catch (Exception e)
			{
				return false;
			}
		}
		public void Write(BinaryWriter Writer)
		{
			// write the tag
			Common.WriteByte(Writer, Tag);
			Common.WriteWord(Writer, ClassIndex + 1);
			Common.WriteWord(Writer, NameAndTypeIndex + 1);
		}
		public override bool Resolve(ArrayList FItems)
		{
			// use the index into the constant pool table
			// to find our UTF8 encoded class or interface name
			if (ClassIndex < FItems.Count && NameAndTypeIndex < FItems.Count)
			{
				Object o = FItems[ClassIndex];
				if (o is ConstantClassInfo)
				{
					ParentClass = (ConstantClassInfo) o;
					((ConstantPoolInfo)o).References++;
				}

				o = FItems[NameAndTypeIndex];
				if (o is ConstantNameAndTypeInfo)
				{
					NameAndType = (ConstantNameAndTypeInfo) o;
					((ConstantPoolInfo)o).References++;
				}

				return true;
			}
			
			return false;
		}
		public override void SetNameAndType(ushort Index, TConstantPool ConstantPool)
		{
			NameAndTypeIndex = Index;
			NameAndType = (ConstantNameAndTypeInfo)ConstantPool.Item(Index);
			NameAndType.References++;
		}
		public override void SetParent(ushort Index, TConstantPool ConstantPool)
		{
			ClassIndex = Index;
			ParentClass = (ConstantClassInfo)ConstantPool.Item(Index);
			ParentClass.References++;
		}
	}
	class ConstantInterfaceMethodrefInfo : ConstantPoolMethodInfo 
	{
		private ushort ClassIndex;
		private ushort NameAndTypeIndex;

		public override bool Read(byte tag, BinaryReader Reader)
		{
			try
			{
				Tag = tag;
				ClassIndex = Common.ReadWord(Reader);
				NameAndTypeIndex = Common.ReadWord(Reader);
				ClassIndex--;
				NameAndTypeIndex--;
				return true;
			}
			catch (Exception e)
			{
				return false;
			}
		}
		public void Write(BinaryWriter Writer)
		{
			// write the tag
			Common.WriteByte(Writer, Tag);
			Common.WriteWord(Writer, ClassIndex + 1);
			Common.WriteWord(Writer, NameAndTypeIndex + 1);
		}
		public override bool Resolve(ArrayList FItems)
		{
			// use the index into the constant pool table
			// to find our UTF8 encoded class or interface name
			if (ClassIndex <= FItems.Count && NameAndTypeIndex <= FItems.Count)
			{
				Object o = FItems[ClassIndex];
				if (o is ConstantClassInfo)
				{
					ParentClass = (ConstantClassInfo) o;
					((ConstantPoolInfo)o).References++;
				}

				o = FItems[NameAndTypeIndex];
				if (o is ConstantNameAndTypeInfo)
				{
					NameAndType = (ConstantNameAndTypeInfo) o;
					((ConstantPoolInfo)o).References++;
				}

				return true;
			}
			
			return false;
		}
		public override void SetNameAndType(ushort Index, TConstantPool ConstantPool)
		{
			NameAndTypeIndex = Index;
		}
		public override void SetParent(ushort Index, TConstantPool ConstantPool)
		{
			ClassIndex = Index;
		}
	}
	class ConstantStringInfo : ConstantPoolVariableInfo 
	{
		private ushort NameIndex;

		public ConstantStringInfo()
		{
			NameIndex = 0;
			Value = "";
		}

		public override bool Read(byte tag, BinaryReader Reader)
		{
			try
			{
				Tag = tag;
				NameIndex = Common.ReadWord(Reader);
				NameIndex--;
				return true;
			}
			catch (Exception e)
			{
				return false;
			}
		}
		public void Write(BinaryWriter Writer)
		{
			// write the tag
			Common.WriteByte(Writer, Tag);
			Common.WriteWord(Writer, NameIndex + 1);
		}
		public override bool Resolve(ArrayList FItems)
		{
			// use the index into the constant pool table
			// to find our UTF8 encoded class or interface name
			if (NameIndex < FItems.Count)
			{
				Object o = FItems[NameIndex];
				if (o is ConstantUtf8Info)
				{
					Value = Encoding.UTF8.GetString(((ConstantUtf8Info)o).Bytes);
					((ConstantPoolInfo)o).References++;

					return true;
				}
			}
			
			return false;
		}
	}
	class ConstantIntegerInfo : ConstantPoolVariableInfo 
	{
		private uint Bytes;

		public override bool Read(byte tag, BinaryReader Reader)
		{
			try
			{
				Tag = tag;
				Bytes = Common.ReadDWord(Reader);
				return true;
			}
			catch (Exception e)
			{
				return false;
			}
		}
		public void Write(BinaryWriter Writer)
		{
			// write the tag
			Common.WriteByte(Writer, Tag);
			Common.WriteDWord(Writer, Bytes);
		}
		public override bool Resolve(ArrayList FItems)
		{
			Value = (int) Bytes;
			return true;
		}
	}
	class ConstantFloatInfo : ConstantPoolVariableInfo 
	{
		private uint Bytes;

		public override bool Read(byte tag, BinaryReader Reader)
		{
			try
			{
				Tag = tag;
				Bytes = Common.ReadDWord(Reader);
				return true;
			}
			catch (Exception e)
			{
				return false;
			}
		}
		public void Write(BinaryWriter Writer)
		{
			// write the tag
			Common.WriteByte(Writer, Tag);
			Common.WriteDWord(Writer, Bytes);
		}
		public override bool Resolve(ArrayList FItems)
		{
			Value = Convert.ToSingle(Bytes);
			return true;
		}
	}
	class ConstantLongInfo : ConstantPoolVariableInfo 
	{
		private uint HighBytes;
		private uint LowBytes;

		public override bool Read(byte tag, BinaryReader Reader)
		{
			try
			{
				Tag = tag;
				HighBytes = Common.ReadDWord(Reader);
				LowBytes = Common.ReadDWord(Reader);
				Value = ((long)HighBytes << 32) + LowBytes;
				return true;
			}
			catch (Exception e)
			{
				Value = "ERROR_GETTING_LONG";
				return true;
			}
		}
		public void Write(BinaryWriter Writer)
		{
			// write the tag
			Common.WriteByte(Writer, Tag);
			Common.WriteDWord(Writer, HighBytes);
			Common.WriteDWord(Writer, LowBytes);
		}
		public override bool Resolve(ArrayList FItems)
		{
			return true;
		}
	}
	class ConstantDoubleInfo : ConstantPoolVariableInfo 
	{
		private uint HighBytes;
		private uint LowBytes;

		public override bool Read(byte tag, BinaryReader Reader)
		{
			try
			{
				Tag = tag;
				HighBytes = Common.ReadDWord(Reader);
				LowBytes = Common.ReadDWord(Reader);
				Value = "NOT_IMPLEMENTED";
				return true;
			}
			catch (Exception e)
			{
				Value = "ERROR_GETTING_DOUBLE";
				return false;
			}
		}
		public void Write(BinaryWriter Writer)
		{
			// write the tag
			Common.WriteByte(Writer, Tag);
			Common.WriteDWord(Writer, HighBytes);
			Common.WriteDWord(Writer, LowBytes);
		}
		public override bool Resolve(ArrayList FItems)
		{
			Value = Convert.ToDouble(((long)HighBytes << 32) + LowBytes);
			return true;
		}
	}
	class ConstantNameAndTypeInfo : ConstantPoolInfo 
	{
		private ushort FNameIndex;
		private ushort FDescriptorIndex;
		public string Name;
		public string Descriptor;

		public ConstantNameAndTypeInfo()
		{
			Tag = (byte) ConstantPoolInfoTag.ConstantNameAndType;
			FNameIndex = 0;
			FDescriptorIndex = 0;
			Name = "";
			Descriptor = "";
		}
		public ConstantNameAndTypeInfo(ushort IndexName, ushort IndexType, TConstantPool ConstantPool)
		{
			Tag = (byte)ConstantPoolInfoTag.ConstantNameAndType;
			FNameIndex = IndexName;
			FDescriptorIndex = IndexType;
			Name = Encoding.UTF8.GetString(((ConstantUtf8Info)ConstantPool.Item(IndexName)).Bytes);
			ConstantPool.Item(IndexName).References++;
			Descriptor = Encoding.UTF8.GetString(((ConstantUtf8Info)ConstantPool.Item(IndexType)).Bytes);
			ConstantPool.Item(IndexType).References++;
		}
		public void SetName(ushort Index, TConstantPool ConstantPool)
		{
			// where index is a valid index into the constant pool table
			FNameIndex = Index;
			Name = Encoding.UTF8.GetString(((ConstantUtf8Info)ConstantPool.Item(Index)).Bytes);
			ConstantPool.Item(Index).References++;
		}
		public void SetType(ushort Index, TConstantPool ConstantPool)
		{
			// where index is a valid index into the constant pool table
			FDescriptorIndex = Index;
			Descriptor = Encoding.UTF8.GetString(((ConstantUtf8Info)ConstantPool.Item(Index)).Bytes);
			ConstantPool.Item(Index).References++;
		}
		public override bool Read(byte tag, BinaryReader Reader)
		{
			try
			{
				Tag = tag;
				FNameIndex = Common.ReadWord(Reader);
				FNameIndex--;
				FDescriptorIndex = Common.ReadWord(Reader);
				FDescriptorIndex--;
				return true;
			}
			catch (Exception e)
			{
				return false;
			}
		}
		public void Write(BinaryWriter Writer)
		{
			// write the tag
			Common.WriteByte(Writer, Tag);
			Common.WriteWord(Writer, FNameIndex + 1);
			Common.WriteWord(Writer, FDescriptorIndex + 1);
		}
		public override bool Resolve(ArrayList FItems)
		{
			try
			{
				Name = null;

				if (FNameIndex < FItems.Count)
				{
					Object o = FItems[FNameIndex];
					if (o is ConstantUtf8Info)
					{
						Name = Encoding.UTF8.GetString(((ConstantUtf8Info)o).Bytes);
						((ConstantPoolInfo)o).References++;
					}
				}

			}
			catch (Exception e)
			{
				if (Name == null)
					Name = "Error retrieving Name!";
			}

			try
			{
				Descriptor = null;

				if (FDescriptorIndex < FItems.Count)
				{
					Object o = FItems[FDescriptorIndex];
					if (o is ConstantUtf8Info)
					{
						Descriptor = Encoding.UTF8.GetString(((ConstantUtf8Info)o).Bytes);
						((ConstantPoolInfo)o).References++;
					}
				}
			}
			catch (Exception e)
			{
				if (Descriptor == null)
					Descriptor = "Error retrieving Descriptor!";
			}

			return true;
		}
		public ushort NameIndex
		{
			get
			{
				return FNameIndex;
			}
		}
		public ushort TypeIndex
		{
			get
			{
				return FDescriptorIndex;
			}
		}
	}
	class ConstantUtf8Info : ConstantPoolInfo 
	{
		public ushort Length;
		public byte[] Bytes;
		public string Value;

		public ConstantUtf8Info()
		{
			Bytes = null;
			Length = 0;
			Tag = (byte) ConstantPoolInfoTag.ConstantUtf8;
			References = 0;
		}
		public ConstantUtf8Info(string Text)
		{
			Tag = (byte) ConstantPoolInfoTag.ConstantUtf8;
			Bytes = UTF8Encoding.UTF8.GetBytes(Text);
			Length = (ushort) Bytes.Length;
			Value = Encoding.UTF8.GetString(Bytes); 
		}
		public void SetName(string Text)
		{
			Bytes = UTF8Encoding.UTF8.GetBytes(Text);
			Length = (ushort)Bytes.Length;
			Value = Encoding.UTF8.GetString(Bytes); 
		}
		public override bool Read(byte tag, BinaryReader Reader)
		{
			try
			{
				Tag = tag;
				Length = Common.ReadWord(Reader);
				Bytes = Reader.ReadBytes(Length);

				Value = Encoding.UTF8.GetString(Bytes);
				return true;
			}
			catch (Exception e)
			{
				return false;
			}
		}
		public void Write(BinaryWriter Writer)
		{
			// write the tag
			Common.WriteByte(Writer, Tag);
			Common.WriteWord(Writer, Length);
			Writer.Write(Bytes);
		}
		public override bool Resolve(ArrayList FItems)
		{
			return true;
		}
	}

	//  ********************************************************************************   //
	//  ***************************** FIELD STRUCTURES *********************************   //
	//  ********************************************************************************   //

	class FieldInfo 
	{
		ushort FAccessFlags;
		ushort FNameIndex;
		ushort FDescriptorIndex;
		ConstantUtf8Info FName;
		ConstantUtf8Info FDescriptor;
		TAttributes FAttributes;
		// my vars
		long FOffset;

		public FieldInfo(BinaryReader Reader, TConstantPool ConstantPool)
		{
			FAccessFlags = 0;
			FNameIndex = 0;
			FDescriptorIndex = 0;
			FName = null;
			FDescriptor = null;
			FAttributes = null;
			FOffset = 0;

			try
			{
				FOffset = Reader.BaseStream.Position;
				FAccessFlags = Common.ReadWord(Reader);
				FNameIndex = Common.ReadWord(Reader);
				FNameIndex--;
				FDescriptorIndex = Common.ReadWord(Reader);
				FDescriptorIndex--;
				// resolve the references
				FDescriptor = (ConstantUtf8Info)ConstantPool.Item(FDescriptorIndex);
				FDescriptor.References++;
				FName = (ConstantUtf8Info)ConstantPool.Item(FNameIndex);
				FName.References++;
				// Attributes should be able to handle any/all attribute streams
				FAttributes = new TAttributes(Reader, ConstantPool);
			}
			catch (Exception e)
			{
				// do nothing for now
			}
		}
		public void SetName(ushort index, TConstantPool ConstantPool)
		{
			FNameIndex = index;
			FName = (ConstantUtf8Info)ConstantPool.Item(FNameIndex);
			FName.References++;
		}
		public ConstantUtf8Info Name
		{
			get
			{
				return FName;
			}
		}
		public void Write(BinaryWriter Writer)
		{
			try
			{
				FOffset = Writer.BaseStream.Position;
				Common.WriteWord(Writer, FAccessFlags);
				Common.WriteWord(Writer, FNameIndex + 1);
				Common.WriteWord(Writer, FDescriptorIndex + 1);

				// Attributes should be able to handle any/all attribute streams
				FAttributes.Write(Writer);
			}
			catch (Exception e)
			{
				// do nothing for now
			}
		}
		public string Descriptor
		{
			get
			{
				return FDescriptor.Value;
			}
		}
		public ushort NameIndex
		{
			get
			{
				return FNameIndex;
			}
		}
		public TAttributes Attributes
		{
			get
			{
				return FAttributes;
			}
		}
		public long Offset
		{
			get
			{
				return FOffset;
			}
		}
		public FieldInfo Clone()
		{
			return (FieldInfo) MemberwiseClone();
		}
		public void SetType(ushort index, TConstantPool ConstantPool)
		{
			FDescriptorIndex = index;
			FDescriptor = (ConstantUtf8Info)ConstantPool.Item(FDescriptorIndex);
			FDescriptor.References++;
		}
	}

	//  ********************************************************************************   //
	//  *************************** METHODINFO STRUCTURES ******************************   //
	//  ********************************************************************************   //

	class MethodInfo
	{
		ushort FAccessFlags;
		ushort FNameIndex;
		ushort FDescriptorIndex;
		ConstantUtf8Info FName;
		ConstantUtf8Info FDescriptor;
		TAttributes FAttributes;

		public MethodInfo(BinaryReader Reader, TConstantPool ConstantPool)
		{
			FAccessFlags = 0;
			FNameIndex = 0;
			FDescriptorIndex = 0;
			FName = null;
			FDescriptor = null;
			FAttributes = null;

			try
			{
				FAccessFlags = Common.ReadWord(Reader);
				FNameIndex = Common.ReadWord(Reader);
				FNameIndex--;
				FDescriptorIndex = Common.ReadWord(Reader);
				FDescriptorIndex--;
				// resolve the references
				FDescriptor = (ConstantUtf8Info)ConstantPool.Item(FDescriptorIndex);
				FName = (ConstantUtf8Info)ConstantPool.Item(FNameIndex);
				FDescriptor.References++;
				FName.References++;
				// Attributes should be able to handle any/all attribute streams
				FAttributes = new TAttributes(Reader, ConstantPool);
			}
			catch (Exception e)
			{
				// do nothing for now
			}
		}
		public void SetName(ushort index, TConstantPool ConstantPool)
		{
			FNameIndex = index;
			FName = (ConstantUtf8Info)ConstantPool.Item(FNameIndex);
			FName.References++;
		}
		public void Write(BinaryWriter Writer)
		{
			try
			{
				Common.WriteWord(Writer, FAccessFlags);
				Common.WriteWord(Writer, FNameIndex + 1);
				Common.WriteWord(Writer, FDescriptorIndex + 1);

				// Attributes should be able to handle any/all attribute streams
				FAttributes.Write(Writer);
			}
			catch (Exception e)
			{
				// do nothing for now
			}
		}
		public ConstantUtf8Info Name
		{
			get
			{
				return FName;
			}
		}
		public string Descriptor
		{
			get
			{
				return FDescriptor.Value;
			}
		}
		public TAttributes Attributes
		{
			get
			{
				return FAttributes;
			}
		}
		public long Offset
		{
			get
			{
				if (Attributes != null && Attributes.Items.Count > 0)
				{
					try
					{
						return ((CodeAttributeInfo)Attributes.Items[0]).CodeOffset;
					}
					catch (Exception e)
					{
					}
				}

				return 0;
			}
		}
		public ushort NameIndex
		{
			get
			{
				return FNameIndex;
			}
		}
		public MethodInfo Clone()
		{
			return (MethodInfo) MemberwiseClone();
		}
		public void SetType(ushort index, TConstantPool ConstantPool)
		{
			FDescriptorIndex = index;
			FDescriptor = (ConstantUtf8Info)ConstantPool.Item(FDescriptorIndex);
			FDescriptor.References++;
		}
	}

	//  ********************************************************************************   //
	//  *************************** ATTRIBUTE STRUCTURES *******************************   //
	//  ********************************************************************************   //

	abstract class AttributeInfo
	{
		//ushort AttributeNameIndex;
		//ConstantUtf8Info AttributeName;
		//uint AttributeLength;
		//byte[] Bytes;

		public abstract void Write(BinaryWriter Writer);
	}
	class UnknownAttributeInfo : AttributeInfo
	{
		ushort AttributeNameIndex;
		ConstantUtf8Info AttributeName;
		uint AttributeLength;
		byte[] Bytes;

		public UnknownAttributeInfo(int NameIndex, BinaryReader Reader, TConstantPool ConstantPool)
		{
			AttributeNameIndex = 0;
			AttributeName = null;
			AttributeLength = 0;
			Bytes = null;

			try
			{
				AttributeNameIndex = (ushort)NameIndex;
				AttributeLength = Common.ReadDWord(Reader);
				Bytes = Reader.ReadBytes((int)AttributeLength);
				// resolve references
				AttributeName = (ConstantUtf8Info)ConstantPool.Item(AttributeNameIndex);
				AttributeName.References++;
			}
			catch (Exception e)
			{
				// do nothing
			}
		}
		public override void Write(BinaryWriter Writer)
		{
			Common.WriteWord(Writer, AttributeNameIndex + 1);
			Common.WriteDWord(Writer, AttributeLength);
			Writer.Write(Bytes);
		}
	}
	class ConstantValueAttributeInfo : AttributeInfo
	{
		ushort AttributeNameIndex;
		uint AttributeLength;
		ConstantUtf8Info AttributeName;
		ushort ConstantValueIndex;
		ConstantPoolVariableInfo ConstantValue;

		public ConstantValueAttributeInfo(int NameIndex, BinaryReader Reader, TConstantPool ConstantPool)
		{
			AttributeNameIndex = 0;
			AttributeLength = 0;
			AttributeName = null;
			ConstantValueIndex = 0;
			ConstantValue = null;

			try
			{
				AttributeNameIndex = (ushort) NameIndex;
				AttributeLength = Common.ReadDWord(Reader);
				ConstantValueIndex = Common.ReadWord(Reader);
				ConstantValueIndex--;
				// resolve references
				AttributeName = (ConstantUtf8Info)ConstantPool.Item(AttributeNameIndex);
				AttributeName.References++;
				ConstantValue = (ConstantPoolVariableInfo)ConstantPool.Item(ConstantValueIndex);
				ConstantValue.References++;
			}
			catch (Exception e)
			{
				// do nothing
			}
		}
		public override void Write(BinaryWriter Writer)
		{
			Common.WriteWord(Writer, AttributeNameIndex + 1);
			Common.WriteDWord(Writer, AttributeLength);
			Common.WriteWord(Writer, ConstantValueIndex + 1);
		}
	}
	class CodeAttributeInfo : AttributeInfo
	{
		// stuff we need
		ushort AttributeNameIndex;
		ConstantUtf8Info AttributeName;
		uint AttributeLength;
		ushort MaxStack;
		ushort MaxLocals;
		uint CodeLength;
		byte[] Code;
		ushort ExceptionTableLength;
		TExceptionTable[] ExceptionTable;
		TAttributes Attributes;
		// stuff i want
		long FOffsetOfCode;

		public CodeAttributeInfo(int NameIndex, BinaryReader Reader, TConstantPool ConstantPool)
		{
			AttributeNameIndex = 0;
			AttributeName = null;
			AttributeLength = 0;
			MaxStack = 0;
			MaxLocals = 0;
			CodeLength = 0;
			Code = null;
			ExceptionTableLength = 0;
			ExceptionTable = null;
			Attributes = null;

			try
			{
				AttributeNameIndex = (ushort) NameIndex;
				AttributeLength = Common.ReadDWord(Reader);

				MaxStack = Common.ReadWord(Reader);
				MaxLocals = Common.ReadWord(Reader);
				CodeLength = Common.ReadDWord(Reader);

				// save the offset of the code stream
				FOffsetOfCode = Reader.BaseStream.Position;

				Code = Reader.ReadBytes((int) CodeLength);

				ExceptionTableLength = Common.ReadWord(Reader);
				ExceptionTable = new TExceptionTable[ExceptionTableLength];
				// fucking nested arrays! ;/
				for (int i = 0; i < ExceptionTableLength; i++)
				{
					ExceptionTable[i] = new TExceptionTable(Reader, ConstantPool);
				}

				Attributes = new TAttributes(Reader, ConstantPool);

				// resolve references
				AttributeName = (ConstantUtf8Info)ConstantPool.Item(AttributeNameIndex);
				AttributeName.References++;
			}
			catch (Exception e)
			{
				// do nothing
			}
		}
		public override void Write(BinaryWriter Writer)
		{
			Common.WriteWord(Writer, AttributeNameIndex + 1);
			Common.WriteDWord(Writer, AttributeLength);

			Common.WriteWord(Writer, MaxStack);
			Common.WriteWord(Writer, MaxLocals);
			Common.WriteDWord(Writer, CodeLength);
			Writer.Write(Code);

			Common.WriteWord(Writer, ExceptionTableLength);

			for (int i = 0; i < ExceptionTableLength; i++)
			{
				ExceptionTable[i].Write(Writer);
			}

			Attributes.Write(Writer);
		}
		public long CodeOffset
		{
			get
			{
				return FOffsetOfCode;
			}
		}
	}
	class ExceptionsAttributeInfo : AttributeInfo
	{
		ushort AttributeNameIndex;
		ConstantUtf8Info AttributeName;
		uint AttributeLength;
		ushort NumberOfExceptions;
		TException[] ExceptionTable;

		public ExceptionsAttributeInfo(int NameIndex, BinaryReader Reader, TConstantPool ConstantPool)
		{
			AttributeNameIndex = 0;
			AttributeName = null;
			AttributeLength = 0;
			NumberOfExceptions = 0;
			ExceptionTable = null;

			try
			{
				AttributeNameIndex = (ushort) NameIndex;
				AttributeLength = Common.ReadDWord(Reader);

				NumberOfExceptions = Common.ReadWord(Reader);
				ExceptionTable = new TException[NumberOfExceptions];
				// fucking nested arrays! ;/
				for (int i = 0; i < NumberOfExceptions; i++)
				{
					ExceptionTable[i] = new TException(Reader, ConstantPool);
				}
				// resolve references
				AttributeName = (ConstantUtf8Info)ConstantPool.Item(AttributeNameIndex);
				AttributeName.References++;
			}
			catch (Exception e)
			{
				// do nothing
			}
		}
		public override void Write(BinaryWriter Writer)
		{
			Common.WriteWord(Writer, AttributeNameIndex + 1);
			Common.WriteDWord(Writer, AttributeLength);

			Common.WriteWord(Writer, NumberOfExceptions);

			for (int i = 0; i < NumberOfExceptions; i++)
			{
				ExceptionTable[i].Write(Writer);
			}
		}
	}
	class InnerClassesAttributeInfo : AttributeInfo
	{
		ushort AttributeNameIndex;
		ConstantUtf8Info AttributeName;
		uint AttributeLength;
		ushort NumberOfClasses;
		TClasses[] Classes;

		public InnerClassesAttributeInfo(int NameIndex, BinaryReader Reader, TConstantPool ConstantPool)
		{
			AttributeNameIndex = 0;
			AttributeName = null;
			AttributeLength = 0;
			NumberOfClasses = 0;
			Classes = null;

			try
			{
				AttributeNameIndex = (ushort) NameIndex;
				AttributeLength = Common.ReadDWord(Reader);

				NumberOfClasses = Common.ReadWord(Reader);
				Classes = new TClasses[NumberOfClasses];
				// fucking nested arrays! ;/
				for (int i = 0; i < NumberOfClasses; i++)
				{
					Classes[i] = new TClasses(Reader, ConstantPool);
				}
				// resolve references
				AttributeName = (ConstantUtf8Info)ConstantPool.Item(AttributeNameIndex);
				AttributeName.References++;
			}
			catch (Exception e)
			{
				// do nothing
			}
		}
		public override void Write(BinaryWriter Writer)
		{
			Common.WriteWord(Writer, AttributeNameIndex + 1);
			Common.WriteDWord(Writer, AttributeLength);

			Common.WriteWord(Writer, NumberOfClasses);
			for (int i = 0; i < NumberOfClasses; i++)
			{
				Classes[i].Write(Writer);
			}
		}
	}
	class SyntheticAttributeInfo : AttributeInfo
	{
		ushort AttributeNameIndex;
		ConstantUtf8Info AttributeName;
		uint AttributeLength;

		public SyntheticAttributeInfo(int NameIndex, BinaryReader Reader, TConstantPool ConstantPool)
		{
			AttributeNameIndex = 0;
			AttributeName = null;
			AttributeLength = 0;

			try
			{
				AttributeNameIndex = (ushort) NameIndex;
				AttributeLength = Common.ReadDWord(Reader);

				// resolve references
				AttributeName = (ConstantUtf8Info)ConstantPool.Item(AttributeNameIndex);
				AttributeName.References++;
			}
			catch (Exception e)
			{
				// do nothing
			}
		}
		public override void Write(BinaryWriter Writer)
		{
			Common.WriteWord(Writer, AttributeNameIndex + 1);
			Common.WriteDWord(Writer, AttributeLength);
		}
	}
	class SourceFileAttributeInfo : AttributeInfo
	{
		ushort AttributeNameIndex;
		ConstantUtf8Info AttributeName;
		uint AttributeLength;
		ushort SourceFileIndex;
		ConstantUtf8Info SourceFile;

		public SourceFileAttributeInfo(int NameIndex, BinaryReader Reader, TConstantPool ConstantPool)
		{
			AttributeNameIndex = 0;
			AttributeName = null;
			AttributeLength = 0;
			SourceFileIndex = 0;
			SourceFile = null;

			try
			{
				AttributeNameIndex = (ushort) NameIndex;
				AttributeLength = Common.ReadDWord(Reader);
				SourceFileIndex = Common.ReadWord(Reader);
				SourceFileIndex--;
				// resolve references
				AttributeName = (ConstantUtf8Info)ConstantPool.Item(AttributeNameIndex);
				AttributeName.References++;
				SourceFile = (ConstantUtf8Info)ConstantPool.Item(AttributeNameIndex);
				SourceFile.References++;
			}
			catch (Exception e)
			{
				// do nothing
			}
		}
		public override void Write(BinaryWriter Writer)
		{
			Common.WriteWord(Writer, AttributeNameIndex + 1);
			Common.WriteDWord(Writer, AttributeLength);
			Common.WriteWord(Writer, SourceFileIndex + 1);
		}
	}
	class LineNumberAttributeInfo : AttributeInfo
	{
		ushort AttributeNameIndex;
		ConstantUtf8Info AttributeName;
		uint AttributeLength;
		ushort LineNumberTableLength;
		TLineNumberTable[] LineNumberTable;
		long OriginalPos;

		public LineNumberAttributeInfo(int NameIndex, BinaryReader Reader, TConstantPool ConstantPool)
		{
			AttributeNameIndex = 0;
			AttributeName = null;
			AttributeLength = 0;
			LineNumberTableLength = 0;
			LineNumberTable = null;
			OriginalPos = 0;

			try
			{
				AttributeNameIndex = (ushort) NameIndex;
				AttributeLength = Common.ReadDWord(Reader);
				OriginalPos = Reader.BaseStream.Position;

				LineNumberTableLength = Common.ReadWord(Reader);
				LineNumberTable = new TLineNumberTable[LineNumberTableLength];
				// fucking nested arrays! ;/
				for (int i = 0; i < LineNumberTableLength; i++)
				{
					LineNumberTable[i] = new TLineNumberTable(Reader);
				}
				// resolve references
				AttributeName = (ConstantUtf8Info)ConstantPool.Item(AttributeNameIndex);
				AttributeName.References++;

				Reader.BaseStream.Position = OriginalPos + AttributeLength;
			}
			catch (Exception e)
			{
				// do nothing
			}
		}
		public override void Write(BinaryWriter Writer)
		{
			Common.WriteWord(Writer, AttributeNameIndex + 1);
			Common.WriteDWord(Writer, AttributeLength);

			Common.WriteWord(Writer, LineNumberTableLength);
			for (int i = 0; i < LineNumberTableLength; i++)
			{
				LineNumberTable[i].Write(Writer);
			}
		}
	}
	class LocalVariablesAttributeInfo : AttributeInfo
	{
		ushort AttributeNameIndex;
		ConstantUtf8Info AttributeName;
		uint AttributeLength;
		ushort LocalVariableTableLength;
		TLocalVariableTable[] LocalVariableTable;

		public LocalVariablesAttributeInfo(int NameIndex, BinaryReader Reader, TConstantPool ConstantPool)
		{
			AttributeNameIndex = 0;
			AttributeName = null;
			AttributeLength = 0;
			LocalVariableTableLength = 0;
			LocalVariableTable = null;

			try
			{
				AttributeNameIndex = (ushort) NameIndex;
				AttributeLength = Common.ReadDWord(Reader);

				LocalVariableTableLength = Common.ReadWord(Reader);
				LocalVariableTable = new TLocalVariableTable[LocalVariableTableLength];
				// fucking nested arrays! ;/
				for (int i = 0; i < LocalVariableTableLength; i++)
				{
					LocalVariableTable[i] = new TLocalVariableTable(Reader, ConstantPool);
				}
				// resolve references
				AttributeName = (ConstantUtf8Info)ConstantPool.Item(AttributeNameIndex);
				AttributeName.References++;
			}
			catch (Exception e)
			{
				// do nothing
			}
		}

		public override void Write(BinaryWriter Writer)
		{
			Common.WriteWord(Writer, AttributeNameIndex + 1);
			Common.WriteDWord(Writer, AttributeLength);

			Common.WriteWord(Writer, LocalVariableTableLength);
			for (int i = 0; i < LocalVariableTableLength; i++)
			{
				LocalVariableTable[i].Write(Writer);
			}
		}
	}
	class DeprecatedAttributeInfo : AttributeInfo
	{
		ushort AttributeNameIndex;
		ConstantUtf8Info AttributeName;
		uint AttributeLength;

		public DeprecatedAttributeInfo(int NameIndex, BinaryReader Reader, TConstantPool ConstantPool)
		{
			AttributeNameIndex = 0;
			AttributeName = null;
			AttributeLength = 0;

			try
			{
				AttributeNameIndex = (ushort) NameIndex;
				// length should be zero.. 
				// TODO: maybe put a check in?? probably no need at this point..
				AttributeLength = Common.ReadDWord(Reader);
				// resolve references
				AttributeName = (ConstantUtf8Info)ConstantPool.Item(AttributeNameIndex);
				AttributeName.References++;
			}
			catch (Exception e)
			{
				// do nothing
			}
		}
		public override void Write(BinaryWriter Writer)
		{
			Common.WriteWord(Writer, AttributeNameIndex + 1);
			Common.WriteDWord(Writer, AttributeLength);
		}
	}
	
	// the inner attribute classes
	class TLocalVariableTable
	{
		ushort StartPC;
    	ushort Length;
    	ushort NameIndex;
		ConstantUtf8Info Name;
    	ushort DescriptorIndex;
		ConstantUtf8Info Descriptor;
    	ushort Index;

		public TLocalVariableTable(BinaryReader Reader, TConstantPool ConstantPool)
		{
			StartPC = 0;
			Length = 0;
			NameIndex = 0;
			Name = null;
			DescriptorIndex = 0;
			Descriptor = null;
			Index = 0;

			try
			{
				StartPC = Common.ReadWord(Reader);
				StartPC--;
				Length = Common.ReadWord(Reader);
				NameIndex = Common.ReadWord(Reader);
				NameIndex--;
				DescriptorIndex = Common.ReadWord(Reader);
				DescriptorIndex--;
				Index = Common.ReadWord(Reader);
				Index--;
				// resolve references
				Name = (ConstantUtf8Info)ConstantPool.Item(NameIndex);
				Name.References++;
				Descriptor = (ConstantUtf8Info)ConstantPool.Item(DescriptorIndex);
				Descriptor.References++;
			}
			catch (Exception e)
			{
				// do nothing
			}
		}
		public void Write(BinaryWriter Writer)
		{
			Common.WriteWord(Writer, StartPC);
			Common.WriteWord(Writer, Length);
			Common.WriteWord(Writer, NameIndex + 1);
			Common.WriteWord(Writer, DescriptorIndex + 1);
			Common.WriteWord(Writer, Index + 1);
		}
	}
	class TLineNumberTable
	{
		ushort StartPC;
    	ushort LineNumber;

		public TLineNumberTable(BinaryReader Reader)
		{
			LineNumber = 0;
			StartPC = 0;

			try
			{
				StartPC = Common.ReadWord(Reader);
				LineNumber = Common.ReadWord(Reader);
			}
			catch (Exception e)
			{
				// do nothing
			}
		}

		public void Write(BinaryWriter Writer)
		{
			Common.WriteWord(Writer, StartPC);
			Common.WriteWord(Writer, LineNumber);
		}
	}
	class TClasses
	{
		ushort InnerClassInfoIndex;
		ConstantClassInfo InnerClassInfo;
    	ushort OuterClassInfoIndex;
		ConstantClassInfo OuterClassInfo;
    	ushort InnerNameIndex;
		ConstantUtf8Info InnerName;
    	ushort InnerClassAccessFlags;

		public TClasses(BinaryReader Reader, TConstantPool ConstantPool)
		{
			InnerClassInfoIndex = 0;
			InnerClassInfo = null;
			OuterClassInfoIndex = 0; ;
			OuterClassInfo = null;
			InnerNameIndex = 0; ;
			InnerName = null;
			InnerClassAccessFlags = 0; ;

			try
			{
				InnerClassInfoIndex = Common.ReadWord(Reader);
				InnerClassInfoIndex--;
				OuterClassInfoIndex = Common.ReadWord(Reader);
				OuterClassInfoIndex--;
				InnerNameIndex = Common.ReadWord(Reader);
				InnerNameIndex--;
				InnerClassAccessFlags = Common.ReadWord(Reader);

				// resolve references
				if (InnerNameIndex >= 0)
				{
					InnerName = (ConstantUtf8Info)ConstantPool.Item(InnerNameIndex);
					InnerName.References++;
				}
				if (InnerNameIndex >= 0)
				{
					InnerClassInfo = (ConstantClassInfo)ConstantPool.Item(InnerClassInfoIndex);
					InnerClassInfo.References++;
				}
				if (InnerNameIndex >= 0)
				{
					OuterClassInfo = (ConstantClassInfo)ConstantPool.Item(OuterClassInfoIndex);
					OuterClassInfo.References++;
				}
			}
			catch (Exception e)
			{
				// do nothing
			}
		}
		public void Write(BinaryWriter Writer)
		{
			Common.WriteWord(Writer, InnerClassInfoIndex + 1);
			Common.WriteWord(Writer, OuterClassInfoIndex + 1);
			Common.WriteWord(Writer, InnerNameIndex + 1);
			Common.WriteWord(Writer, InnerClassAccessFlags);
		}
	}
	class TException
	{
		ushort ExceptionIndex;
    	ConstantClassInfo Exception;

		public TException(BinaryReader Reader, TConstantPool ConstantPool)
		{
			ExceptionIndex = 0;

			try
			{
				ExceptionIndex = Common.ReadWord(Reader);
				ExceptionIndex--;
				// resolve references
				Exception = (ConstantClassInfo)ConstantPool.Item(ExceptionIndex);
			}
			catch (Exception e)
			{
				Exception = null;
			}
		}
		public void Write(BinaryWriter Writer)
		{
			Common.WriteWord(Writer, ExceptionIndex + 1);
		}
	}
	class TExceptionTable
	{
		ushort StartPC;
    	ushort EndPC;
    	ushort HandlerPC;
    	ushort CatchType;
		ConstantClassInfo Catch;

		public TExceptionTable(BinaryReader Reader, TConstantPool ConstantPool)
		{
			StartPC = 0;
			EndPC = 0;
			HandlerPC = 0;
			CatchType = 0;
			Catch = null;
			
			try
			{
				StartPC = Common.ReadWord(Reader);
				StartPC--;
				EndPC = Common.ReadWord(Reader);
				EndPC--;
				HandlerPC = Common.ReadWord(Reader);
				HandlerPC--;
				CatchType = Common.ReadWord(Reader);
				CatchType--;

				if (CatchType >= 0)
				{
					Catch = (ConstantClassInfo)ConstantPool.Item(CatchType);
				}
			}
			catch (Exception e)
			{
			}
		}
		public void Write(BinaryWriter Writer)
		{
			Common.WriteWord(Writer, StartPC + 1);
			Common.WriteWord(Writer, EndPC + 1);
			Common.WriteWord(Writer, HandlerPC + 1);
			Common.WriteWord(Writer, CatchType + 1);
		}
	}

	//  ********************************************************************************   //
	//  *************************** INTERFACE STRUCTURES *******************************   //
	//  ********************************************************************************   //

	struct InterfaceInfo
	{
		private ushort FValue;
		private ConstantClassInfo FInterface;

		public InterfaceInfo(BinaryReader Reader, TConstantPool ConstantPool)
		{
			try
			{
				FValue = Common.ReadWord(Reader);
				FValue--;

				FInterface = (ConstantClassInfo)ConstantPool.Item(FValue);
			}
			catch (Exception e)
			{
				FValue = 0;
				FInterface = null;
			}
		}

		public void Write(BinaryWriter Writer)
		{
			try
			{
				Common.WriteWord(Writer, FValue + 1);
			}
			catch (Exception e)
			{
			}
		}

		public int Value
		{
			get
			{
				return FValue;
			}
		}

		public ConstantClassInfo Interface
		{
			get
			{
				return FInterface;
			}
		}

		public string Name
		{
			get
			{
				if (FInterface != null)
				{
					return FInterface.Name;
				}

				return "";
			}
		}
	    
	    public void SetName(string NewName)
	    {
	        
	    }
	}

	//  ********************************************************************************   //
	//  ***************************** COMMON FUNCTIONS *********************************   //
	//  ********************************************************************************   //

    class RenameData
    {
        string FType;
        string FName; 

        public RenameData(string Type, string Name)
        {
            FType = Type;
            FName = Name;
        }
        public string[] GetData()
        {
            string[] s = new string[2];
            s[0] = FType;
            s[1] = FName;
            
            return s;
        }
        public string FieldType
        {
            get { return FType; }
            set { FType = value; }
        }
        public string FieldName
        {
            get { return FName; }
            set { FName = value; }
        }
    }
    class RenameDatabase
    {
        private Hashtable FRenameMethods = null;
        private Hashtable FRenameFields = null;
        private Hashtable FRenameClass = null;
        
        public RenameDatabase()
        {
            FRenameMethods = new Hashtable();
            FRenameFields = new Hashtable();
            FRenameClass = new Hashtable();
        }
        public void AddRename(Hashtable DestTable, string ClassName, string OldDescriptor, string OldName, string NewDescriptor, string NewName)
        {
            ArrayList al = (ArrayList)DestTable[ClassName];

            if (al == null)
            {
                al = new ArrayList();
                DestTable[ClassName] = al;
            }
            else
            {
                // make sure it doesnt already exist
                for (int i = 0; i < al.Count; i += 2)
                {
                    RenameData rd = (RenameData)al[i];

                    if (rd.FieldName == OldName && rd.FieldType == OldDescriptor)
                    {
                        // if it does, overwrite it, don't add in a new one
                        rd.FieldName = NewName;
                        return;
                    }
                }
            }

            al.Add(new RenameData(OldDescriptor, OldName));
            al.Add(new RenameData(NewDescriptor, NewName));
        }
        public RenameData GetRenameInfo(Hashtable DestTable, string ClassName, string OldDescriptor, string OldName)
        {
            ArrayList al = (ArrayList)DestTable[ClassName];

            if (al == null)
                return null;

            for (int i = 0; i < al.Count; i += 2)
            {
                RenameData rd = (RenameData)al[i];

                if (rd.FieldName == OldName && rd.FieldType == OldDescriptor)
                {
                    return (RenameData)al[i + 1];
                }
            }

            return null;
        }
        public void AddRenameMethod(string ClassName, string OldDescriptor, string OldName, string NewDescriptor, string NewName)
        {
            AddRename(FRenameMethods, ClassName, OldDescriptor, OldName, NewDescriptor, NewName);
        }
        public void AddRenameField(string ClassName, string OldDescriptor, string OldName, string NewDescriptor, string NewName)
        {
            AddRename(FRenameFields, ClassName, OldDescriptor, OldName, NewDescriptor, NewName);
        }
        public RenameData GetNewMethodInfo(string ClassName, string OldDescriptor, string OldName)
        {
            // searches for a matching method in the methodlist
            return GetRenameInfo(FRenameMethods, ClassName, OldDescriptor, OldName);
        }
        public RenameData GetNewFieldInfo(string ClassName, string OldDescriptor, string OldName)
        {
            // searches for a matching method in the methodlist
            return GetRenameInfo(FRenameFields, ClassName, OldDescriptor, OldName);
        }
        public void AddRenameClass(string OldClassName, string NewClassName)
        {
            FRenameClass[OldClassName] = NewClassName;
        }
        public string GetNewClassName(string OldClassName)
        {
            return (string) FRenameClass[OldClassName];
        }
        public string GetNewClassNameOnly(string OldClassName)
        {
            string temp = GetNewClassName(OldClassName);
            
            if (temp == null)
                return null;
            
            string[] strspl = temp.Split(':');

            if (strspl.Length > 0)
            {
                return strspl[0].Trim();
            }
            
            return null;
        }
        /*public bool ReadFromFile(string FileName)
        {
            // serialize this to read in .xml file ?
            return false;
        }*/
    }
	class Common
	{
	    public static string GetClassName(string FullName)
	    {
	        // gets the class name from a class path
	        if (FullName.Contains("/"))
                return FullName.Substring(FullName.LastIndexOf('/') + 1, FullName.Length - FullName.LastIndexOf('/') - 1);
	        else 
	            return FullName;
	    }
        public static string GetClassPath(string FullName)
        {
            // gets the class name from a class path
            return FullName.Substring(0, FullName.LastIndexOf('/') + 1);
        }
		public static string NewClassName(string OriginalClassName, string NewName)
		{
            NewName = Common.GetClassName(NewName);
			// new name should be the short name
			// original class name should be original long name
			if (OriginalClassName.LastIndexOf('/') > 0)
			{
				//string old_name = OriginalClassName.Substring(OriginalClassName.LastIndexOf('/') + 1, OriginalClassName.Length - OriginalClassName.LastIndexOf('/') - 1);
				OriginalClassName = OriginalClassName.Remove(OriginalClassName.LastIndexOf('/') + 1, OriginalClassName.Length - OriginalClassName.LastIndexOf('/') - 1);
				//OriginalClassName += NewName + old_name;
                OriginalClassName += NewName;

				return OriginalClassName;
			}
	
			//return NewName + OriginalClassName;
            return NewName;
		}
       	public static string FixDescriptor(string Descriptor, string OldClassName, string NewClassName)
		{
			return Descriptor.Replace("L" + OldClassName + ";", "L" + NewClassName + ";");
		}
        private static ushort SwapBytes(ushort value)
		{
			ushort a = (ushort) (value >> 8);
			ushort b = (ushort) (value << 8);

			return ((ushort) (a | b));
		}
		public static ushort ReadWord(BinaryReader Reader)
		{
			if (Reader == null)
				return 0;

			ushort val = (ushort) Reader.ReadInt16();
			val = SwapBytes(val);

			return val;
		}
    	public static void WriteWord(BinaryWriter Writer, int Data)
		{
			if (Writer == null)
				return;

			//convert the data from small endian to big endian
			Data = SwapBytes((ushort)Data);

			Writer.Write((ushort)Data);
		}
		public static uint ReadDWord(BinaryReader Reader)
		{
			if (Reader == null)
				return 0;

			// get the value, and then change it from big endian to small endian
			uint val = (uint) Reader.ReadInt32();
			uint temp = val >> 16;
			temp = SwapBytes((ushort) temp);
			val = val & 0x0FFFF;
			val = SwapBytes((ushort) val);
			val = (val << 16) | temp;

			return val;
		}
		public static void WriteDWord(BinaryWriter Writer, uint Data)
		{
			if (Writer == null)
				return;
			
			//convert the data from small endian to big endian
			uint temp = Data >> 16;
			temp = SwapBytes((ushort)temp);
			Data = Data & 0x0FFFF;
			Data = SwapBytes((ushort)Data);
			Data = (Data << 16) | temp;

			Writer.Write(Data);
		}
		public static byte ReadByte(BinaryReader Reader)
		{
			if (Reader == null)
				return 0;
			
			return Reader.ReadByte();
		}
		public static void WriteByte(BinaryWriter Writer, byte Data)
		{
			if (Writer == null)
				return;

			Writer.Write(Data);
		}
	}
}
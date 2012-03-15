using System;
using System.Collections;
using System.IO;

namespace JavaDeObfuscator
{
	//  ********************************************************************************   //
	//  ************************** ACTUAL DEOBFUSCATOR  ********************************   //
	//  ********************************************************************************   //
	//  These class does the actual deobfuscation

	class TDeObfuscator
	{
		// event delegates
		public delegate void ProgressHandler(int Progress);
		public static event ProgressHandler Progress;

		// private variables
		private ArrayList FFiles;
		private ArrayList FClassFiles;
		private ArrayList FInterfaces;
		private ArrayList FChangeList;
		private bool FThoroughMode;
		private bool FRenameClasses;

		public bool UseUniqueNums { get; set; }
		public string OutputDir { get; set; }

		/// <summary>
		/// The DeObfuscating engine
		/// </summary>
		/// <param name="Files">All of the files in the project. Must be full path + filename</param>
		public TDeObfuscator(ArrayList Files)
		{
			if (Files == null)
				return;

			FFiles = Files;

			foreach (string f in FFiles)
			{
				if (!File.Exists(f))
					return;
			}

			FThoroughMode = true;
			FRenameClasses = true;
		}
		public bool DoRename(string Name)
		{
			ArrayList bad_names;

			bad_names = new ArrayList();
			bad_names.Add("for");
			bad_names.Add("char");
			bad_names.Add("void");
			bad_names.Add("byte");
			bad_names.Add("do");
			bad_names.Add("int");
			bad_names.Add("long");
			bad_names.Add("else");
			bad_names.Add("case");
			bad_names.Add("new");
			bad_names.Add("goto");
			bad_names.Add("try");
			bad_names.Add("null");

			Name = Common.GetClassName(Name);


			if (Name[0] == '<')
				return false;

			if (Name.Length > 0 && Name.Length <= 2)
				return true;

			if (Name.Length > 0 && Name.Length <= 3 && Name.Contains("$"))
				return true;

			foreach (string s in bad_names)
			{
				if (s == Name)
					return true;
			}

			return false;
		}
		private bool ClassNameExists(String Name)
		{
			foreach (Object ClassFile in FClassFiles.ToArray())
			{
				if (((TClassFile)ClassFile).ThisClassName == Name)
					return true;
			}

			return false;
		}
		private ArrayList DeObfuscateSingleFile(int index, RenameDatabase RenameStore)
		{
			TClassFile ClassFile = (TClassFile)FClassFiles[index];

			if (ClassFile == null)
				return null;

			// add the class name to the head of the changelist
			FChangeList = new ArrayList();
			FChangeList.Add(ClassFile.ThisClassName);

			string OriginalClassName = ClassFile.ThisClassName;
			string OriginalClassAndType = ClassFile.ThisClassName + " : " + ClassFile.SuperClassName;

			// rename the class and add the new class name to the changelist at [1]
			if (FRenameClasses && RenameStore.GetNewClassNameOnly(OriginalClassAndType) != null)
			{
				// check if we need to use a user-supplied class name first
				string NewClassName = RenameStore.GetNewClassNameOnly(OriginalClassAndType);

				while (ClassNameExists(NewClassName))
				{
					NewClassName += "_";
				}
				FChangeList.Add(ClassFile.ChangeClassName(NewClassName));
			}
			else if (FRenameClasses && DoRename(OriginalClassName))
			{
				string NewClassName;

				if (UseUniqueNums)
				{
					string format = "{0:D" + (FClassFiles.Count.ToString().Length + 2) + "}";
					string uniqueNum = string.Format(format, Convert.ToInt64(ClassFile.ThisClassCode.ToString() + index.ToString()));

					NewClassName = String.Format("Class_{0}_{1}", Common.GetClassName(OriginalClassName), uniqueNum);
				}
				else
					NewClassName = String.Format("Class_{0}", Common.GetClassName(OriginalClassName));

				// test if the filename we are changing to hasnt already been used!
				while (ClassNameExists(NewClassName))
				{
					NewClassName += "_";
				}
				FChangeList.Add(ClassFile.ChangeClassName(NewClassName));
			}
			else
				FChangeList.Add(OriginalClassName);

			// process the Methods
			for (int i = 0; i < ClassFile.Methods.Items.Count; i++)
			{
				MethodInfo mi = (MethodInfo)ClassFile.Methods.Items[i];
				RenameData rd = RenameStore.GetNewMethodInfo(OriginalClassAndType, mi.Descriptor, mi.Name.Value);

				// this is the rule for renaming
				if (DoRename(mi.Name.Value) || rd != null)
				{
					// clone the original method
					TMethodChangeRecord mcr = new TMethodChangeRecord(mi);
					// rename all of the functions something meaningful
					string NewName;
					// if the offset is zero, it probably means its an abstract method
					if (ClassFile.AccessFlags == AccessFlags.ACC_INTERFACE)
						NewName = String.Format("sub_iface_{0:x}", i);
					else if (mi.Offset != 0)
						NewName = String.Format("sub_{0:x}", mi.Offset);
					else
						NewName = String.Format("sub_null_{0:x}", i);

					/*if (FThoroughMode)
					{
						int j = 0;
						while (ClassFile.Methods.MethodNameExists(NewName))
						{
							// rename the method
							NewName = NewName + "_" + j;
							j++;
						}
					}*/

					// user supplied names take precedence
					if (rd != null)
					{
						NewName = rd.FieldName;
					}

					// change the method name
					ClassFile.ChangeMethodName(i, NewName);
					// set the 
					mcr.ChangedTo(mi);
					FChangeList.Add(mcr);
				}

				// fix the descriptor regardless
				ClassFile.ChangeMethodParam(i, OriginalClassName, ClassFile.ThisClassName);
			}

			// process the Fields
			for (int i = 0; i < ClassFile.Fields.Items.Count; i++)
			{
				FieldInfo fi = (FieldInfo)ClassFile.Fields.Items[i];
				RenameData rd = RenameStore.GetNewFieldInfo(OriginalClassAndType, fi.Descriptor, fi.Name.Value);

				if (DoRename(fi.Name.Value) || rd != null)
				{
					// clone the original method
					TFieldChangeRecord fcr = new TFieldChangeRecord(fi);
					// rename all of the fields something meaningful
					string NewName;
					// if the offset is zero, it probably means its a null/abstract method
					if (fi.Offset != 0)
						NewName = String.Format("var_{0:x}", fi.Offset);
					else
						NewName = String.Format("var_null_{0:x}", fi.Offset);

					/*if (FThoroughMode)
					{
						int j = 0;
						while (ClassFile.Methods.FieldNameExists(NewName))
						{
							// rename the field
							NewName = NewName + "_" + j;
							j++;
						}
					}*/

					if (rd != null)
					{
						NewName = rd.FieldName;
					}

					ClassFile.ChangeFieldName(i, NewName);

					fcr.ChangedTo(fi);
					FChangeList.Add(fcr);
				}

				// fix the descriptor regardless
				ClassFile.ChangeFieldType(i, OriginalClassName, ClassFile.ThisClassName);
			}

			return FChangeList;
		}
		/// <summary>
		/// This function runs over a class, fixing up any references from a deobfuscated file.
		/// </summary>
		/// <param name="Index">This is the index of the ClassFile to have its references updated</param>
		/// <param name="ChangeList">This is a list of before/after values from a previously deobfuscated file</param>
		private void FixReferencePass1(int Index, ArrayList ChangeList, ArrayList OwnerChangeList)
		{
			/* the first pass does the following:
			 *  - replaces the Super Class name (if it needs replacing)
			 *  - replaces any constant method/field names (if they need replacing)
			 *  - replaces the class field names (if needed)
			 * it does NOT change the original class name */
			TClassFile ClassFile = (TClassFile)FClassFiles[Index];

			if (ClassFile == null)
				return;

			// - ChangeList[0] is always a string, which is the parent name of the deobfuscated class
			// - ChangeList[1] is always the deobfuscated (new) class name... yes i know this is lame :P
			string OldParentName = (string)ChangeList[0];
			string NewParentName = (string)ChangeList[1];

			// check the Super class name if it needs renaming
			if (ClassFile.SuperClassName == OldParentName)
			{
				ClassFile.ChangeSuperClassName(NewParentName);
			}

			// loop through the constant pool for field/method references
			// check the parent of each, and if the parent is the class we have
			// just modified, try and match it to one of the changes
			// in the changearray
			for (int i = 0; i < ClassFile.ConstantPool.MaxItems(); i++)
			{
				if (ClassFile.ConstantPool.Item(i) is ConstantPoolMethodInfo)
				{
					ConstantPoolMethodInfo ci = (ConstantPoolMethodInfo)ClassFile.ConstantPool.Item(i);

					// check its parent
					if (ci.ParentClass.Name == OldParentName || ci.ParentClass.Name == NewParentName)
					{
						// check the descriptor
						// - for fields this is the field type
						// - for methods this is the parameter list

						// if parents are the same, check the name and descriptor 
						// against the list of originals
						for (int j = 2; j < ChangeList.Count; j++)
						{
							if ((ChangeList[j] is TMethodChangeRecord) && (ci is ConstantMethodrefInfo || ci is ConstantInterfaceMethodrefInfo))
							{
								if (ci is ConstantInterfaceMethodrefInfo)
								{
									// handle interface references differently
									TMethodChangeRecord mcr = (TMethodChangeRecord)ChangeList[j];

									// if found update it to the overridden version
									if (mcr.OriginalMethod.Name.Value == ci.NameAndType.Name &&
										mcr.OriginalMethod.Descriptor == ci.NameAndType.Descriptor)
									{
										// find the overridden version
										for (int k = 2; k < OwnerChangeList.Count; k++)
										{
											if (OwnerChangeList[k] is TMethodChangeRecord)
											{
												TMethodChangeRecord mcr2 = (TMethodChangeRecord)OwnerChangeList[k];
												if (mcr2.OriginalMethod.Name.Value == mcr.OriginalMethod.Name.Value &&
													mcr2.OriginalMethod.Descriptor == mcr.OriginalMethod.Descriptor)
												{
													ClassFile.ChangeConstantFieldName(i, mcr2.NewMethod.Name.Value);
													break;
												}
											}
										}
									}
								}
								else
								{
									TMethodChangeRecord mcr = (TMethodChangeRecord)ChangeList[j];

									// if found update it to the new version...
									if (mcr.OriginalMethod.Name.Value == ci.NameAndType.Name &&
										mcr.OriginalMethod.Descriptor == ci.NameAndType.Descriptor)
									{
										ClassFile.ChangeConstantFieldName(i, mcr.NewMethod.Name.Value);
										break;
									}
								}
							}
							else if ((ChangeList[j] is TFieldChangeRecord) && (ci is ConstantFieldrefInfo))
							{
								TFieldChangeRecord fcr = (TFieldChangeRecord)ChangeList[j];

								// if found update it to the new version...
								if (fcr.OriginalField.Name.Value == ci.NameAndType.Name &&
									fcr.OriginalField.Descriptor == ci.NameAndType.Descriptor)
								{
									ClassFile.ChangeConstantFieldName(i, fcr.NewField.Name.Value);
									break;
								}
							}
						}
					}
				}
			}

			// also loop through the Fields array to change all the Types
			for (int i = 0; i < ClassFile.Fields.MaxItems(); i++)
			{
				ClassFile.ChangeFieldType(i, OldParentName, NewParentName);
			}
			// do the same for methods (fix the parameter list)
			for (int i = 0; i < ClassFile.Methods.MaxItems(); i++)
			{
				ClassFile.ChangeMethodParam(i, OldParentName, NewParentName);
			}
			// and the same for all the interfaces
			for (int i = 0; i < ClassFile.Interfaces.Items.Count; i++)
			{
				if (ClassFile.Interfaces.Item(i).Name == OldParentName)
					ClassFile.ChangeInterfaceName(i, NewParentName);
			}
		}
		/// <summary>
		/// Stage 2 simply goes through the constant pool searching for a ClassInfo structure that matches
		/// the obfuscated class name, and replaces it with the de-obfuscated class name.
		/// 
		/// This is to ensure that any field/variable that references that class will be updated, simply by 
		/// changing the class info structure at the source.
		/// </summary>
		/// <param name="Index">Index of the class file we want to update</param>
		/// <param name="ChangeList">The array of changes we made deobfuscating a file</param>
		private void FixReferencePass2(int Index, ArrayList ChangeList)
		{
			TClassFile ClassFile = (TClassFile)FClassFiles[Index];

			if (ClassFile == null)
				return;

			string OldParentName = (string)ChangeList[0];
			string NewParentName = (string)ChangeList[1];

			// iterate through the constant pool looking for class references
			// that match the old class name
			for (int i = 0; i < ClassFile.ConstantPool.MaxItems(); i++)
			{
				if (ClassFile.ConstantPool.Item(i) is ConstantClassInfo)
				{
					ConstantClassInfo ci = (ConstantClassInfo)ClassFile.ConstantPool.Item(i);

					// if we found a ClassInfo constant with the same name as the old name
					if (ci.Name == OldParentName)
					{
						// create a new UTF string constant
						ConstantUtf8Info ui = new ConstantUtf8Info();
						// set it to the new parent name
						ui.SetName(NewParentName);
						// add it to the constant pool
						ushort index = ClassFile.ConstantPool.Add(ui);
						// set our original ClassInfo constant's name to the newly added UTF string constant
						ci.SetName(index, ClassFile.ConstantPool);
					}
					// special condition for array type references
					else if (ci.Name.IndexOf("L" + OldParentName + ";") >= 0)
					{
						// create a new UTF string constant
						ConstantUtf8Info ui = new ConstantUtf8Info();
						// set it to the new parent name
						ui.SetName(ci.Name.Replace("L" + OldParentName + ";", "L" + NewParentName + ";"));
						// add it to the constant pool
						ushort index = ClassFile.ConstantPool.Add(ui);
						// set our original ClassInfo constant's name to the newly added UTF string constant
						ci.SetName(index, ClassFile.ConstantPool);

					}
				}
				else if (ClassFile.ConstantPool.Item(i) is ConstantPoolMethodInfo)
				{
					// check the descriptor
					// - for fields this is the field type
					// - for methods this is the parameter list
					ClassFile.ChangeConstantFieldType(i, OldParentName, NewParentName);
				}
			}
		}
		private void FixReferences(ArrayList MasterChangeList)
		{
			// loop through the change record's and apply them to each file
			// (except itself)
			for (int i = 0; i < FClassFiles.Count; i++)
			{
				for (int j = 0; j < MasterChangeList.Count; j++)
				{
					FixReferencePass1(i, (ArrayList)MasterChangeList[j], (ArrayList)MasterChangeList[i]);
				}
			}

			for (int i = 0; i < FClassFiles.Count; i++)
			{
				for (int j = 0; j < MasterChangeList.Count; j++)
				{
					FixReferencePass2(i, (ArrayList)MasterChangeList[j]);
				}
			}
		}
		/// <summary>
		/// Find the index of the parent of the classfile, if it exists in the project.
		/// Returns: positive integer index if found, else -1 if not found
		/// </summary>
		/// <param name="Index">Index of class file to find parent of</param>
		/// <returns></returns>
		int FindParent(int Index)
		{
			string ParentName = ((TClassFile)FClassFiles[Index]).SuperClassName;

			for (int i = 0; i < FClassFiles.Count; i++)
			{
				if (i != Index && ((TClassFile)FClassFiles[i]).ThisClassName == ParentName)
				{
					return i;
				}
			}

			return -1;
		}
		/*int FindClass(string ClassName)
		{
			for (int i = 0; i < FClassFiles.Count; i++)
			{
				if (((TClassFile)FClassFiles[i]).ThisClassName == ClassName)
				{
					return i;
				}
			}

			return -1;
		}*/
		int FindInterface(string ClassName)
		{
			for (int i = 0; i < FClassFiles.Count; i++)
			{
				if (((TClassFile)FClassFiles[i]).AccessFlags == AccessFlags.ACC_INTERFACE &&
					((TClassFile)FClassFiles[i]).ThisClassName == ClassName)
				{
					return i;
				}
			}

			return -1;
		}
		ArrayList AddInheritance(int Index, ArrayList MasterChangeList)
		{
			int Parent = FindParent(Index);

			if (Parent >= 0)
			{
				ArrayList OriginalChangeList = (ArrayList)MasterChangeList[Index];
				ArrayList ParentChangeList = (ArrayList)MasterChangeList[Parent];

				for (int i = 2; i < ParentChangeList.Count; i++)
				{
					// add the rest of the parent entries to the original
					OriginalChangeList.Add(ParentChangeList[i]);
				}

				// last of all, if the parent has another parent, recurse and do it all again
				if (FindParent(Parent) >= 0)
				{
					MasterChangeList = AddInheritance(Parent, MasterChangeList);
				}
			}

			return MasterChangeList;
		}
		ArrayList AddInterfaces(int Index, ArrayList MasterChangeList)
		{
			// this needs to work differently to inheritance
			// it does the following:
			// 1. loop through each interface
			// 2. check the MasterChangeList for a matching interface
			// 3. if found, for all methods in the deobfuscated interface, find corresponding entry in 
			//    current classes change list, and update it
			//   
			TClassFile ClassFile = (TClassFile)FClassFiles[Index];

			// for each class file, check each of its interfaces
			for (int i = 0; i < ClassFile.Interfaces.Items.Count; i++)
			{
				// check each interface if it matches any deobfuscated classfile/interface in the project
				for (int j = 0; j < FClassFiles.Count; j++)
				{
					string OldName = (string)((ArrayList)MasterChangeList[j])[0];

					if (OldName == ClassFile.Interfaces.Item(i).Name)
					{
						ArrayList OriginalChangeList = (ArrayList)MasterChangeList[Index];
						ArrayList InterfaceChangeList = (ArrayList)MasterChangeList[j];

						for (int k = 2; k < InterfaceChangeList.Count; k++)
						{
							// add the rest of the parent entries to the original
							// NOTE: this might work best if added to the START of the list!
							OriginalChangeList.Insert(2, InterfaceChangeList[k]);
						}

						break;
					}
				}
			}

			return MasterChangeList;
		}
		ArrayList FixInheritance(ArrayList MasterChangeList)
		{
			for (int i = 0; i < FClassFiles.Count; i++)
			{
				MasterChangeList = AddInheritance(i, MasterChangeList);
				//MasterChangeList = AddInterfaces(i, MasterChangeList);
			}

			return MasterChangeList;
		}
		public ArrayList DeObfuscateAll()
		{
			return DeObfuscateAll(null);
		}
		public ArrayList DeObfuscateAll(RenameDatabase RenameStore)
		{
			FClassFiles = new ArrayList();
			FInterfaces = new ArrayList();
			ArrayList MasterChangeList = new ArrayList();
			ArrayList NewFileNameList = new ArrayList();
			int curr_progress = 0;

			Progress(0);

			// open each class file and add to array
			foreach (string fn in FFiles)
			{
				TClassFile cf = new TClassFile(fn);

				if (cf != null)
				{
					if (cf.Open())
					{
						FClassFiles.Add(cf);

						Progress(++curr_progress);
					}
				}
			}

			// do all the work in memory
			for (int i = 0; i < FClassFiles.Count; i++)
			{
				// this deobfuscates a single class, and keeps a record of all the changes
				// in an arraylist of ChangeRecords
				//
				// we need more here!
				//
				// first, if the file we deobfuscated had a parent, we have to add the entire change list
				// from the parent to the end of the current (recursively), minus the old/new name
				// note: this duplications of data fixes problems with inheritance
				//
				MasterChangeList.Add(DeObfuscateSingleFile(i, RenameStore));

				Progress(i + 1);
			}

			Progress(0);
			curr_progress = 0;

			// iterate through all the class files using the change records saved
			// after the deobfuscation was done
			MasterChangeList = FixInheritance(MasterChangeList);

			// iterate through all the class files using the change records saved
			// after the deobfuscation was done
			FixReferences(MasterChangeList);

			// save all the class files
			for (int i = 0; i < FClassFiles.Count; i++)
			{
				TClassFile cf = (TClassFile)FClassFiles[i];

				// extract the actual filename from the path and replace it with the new ClassName
				string file_name;//= Path.GetDirectoryName(cf.FileName) + Path.DirectorySeparatorChar + Common.GetClassName(cf.ThisClassName) + ".class";

				file_name = Path.Combine(this.OutputDir, Common.GetClassName(cf.ThisClassName) + ".class");


				if (File.Exists(file_name))
				{
					file_name = Path.Combine(this.OutputDir, Common.GetClassName(cf.ThisClassName) + cf.ThisClassCode + ".class");
				}

				if (File.Exists(file_name))
				{
					file_name = Path.Combine(this.OutputDir, Common.GetClassName(cf.ThisClassName) + ((i * cf.ThisClassCode) + i) + ".class");
				}


				//file_name = file_name.Replace('/', '\\');			    

				//if ((file_name != cf.FileName) && FCleanup)
				//{
				//    File.Delete(cf.FileName);
				//}


				// if for some reason the directory doesn't exist, create it
				if (!Directory.Exists(Path.GetDirectoryName(file_name)))
					Directory.CreateDirectory(Path.GetDirectoryName(file_name));

				cf.Save(file_name);

				// return the new filename so the main gui knows what to reload
				NewFileNameList.Add(file_name);

				Progress(++curr_progress);
			}

			return NewFileNameList;
		}

		public bool ThoroughMode
		{
			get
			{
				return FThoroughMode;
			}
			set
			{
				FThoroughMode = value;
			}
		}
		public bool RenameClasses
		{
			get
			{
				return FRenameClasses;
			}
			set
			{
				FRenameClasses = value;
			}
		}
	}

	//  ********************************************************************************   //
	//  *************************** JAVA CLASS WRAPPER  ********************************   //
	//  ********************************************************************************   //
	//  These class encapsulates the java .class file
	//  With a few special methods jammed in to help rename methods and fields (and refs)

	class TClassFile
	{
		// my internal variables
		string FThisClassName;
		string FSuperClassName;
		// internal class file members as designated by Sun
		private uint FMagic;
		private ushort FMinorVersion;
		private ushort FMajorVersion;
		//private ushort FConstantPoolCount;
		private TConstantPool FConstantPool;
		private AccessFlags FAccessFlags;
		private ushort FThisClass;
		private ushort FSuperClass;
		//private ushort FInterfacesCount;
		private TInterfaces FInterfaces;
		//private ushort FFieldsCount;
		private TFields FFields;
		//private ushort FMethodsCount;
		private TMethods FMethods;
		//private ushort FAttributesCount;
		private TAttributes FAttributes;

		// internal variables
		private string FClassFileName = "";
		private BinaryReader FReader = null;
		public ushort ThisClassCode
		{
			get { return FThisClass; }
		}

		public TClassFile(String ClassFileName)
		{
			FClassFileName = ClassFileName;
			//FHasBeenOpened = false;
			FThisClassName = "";
			FSuperClassName = "";
		}

		public bool Open()
		{
			if (File.Exists(FClassFileName))
			{
				try
				{
					// read the .class file systematically
					FileStream fs = new FileStream(FClassFileName, FileMode.Open, FileAccess.Read);
					FReader = new BinaryReader(fs);
					// read header
					FMagic = Common.ReadDWord(FReader);

					if (FMagic != 0x0CAFEBABE)
						return false;

					FMinorVersion = Common.ReadWord(FReader);
					FMajorVersion = Common.ReadWord(FReader);
					// read constant pool
					// this also reads the "FConstantPoolCount"
					// so instead use FConstantPool.MaxItems or somesuch
					FConstantPool = new TConstantPool(FReader);
					// more constants
					FAccessFlags = (AccessFlags)Common.ReadWord(FReader);
					FThisClass = Common.ReadWord(FReader);
					FThisClass--;
					FSuperClass = Common.ReadWord(FReader);
					FSuperClass--;

					FThisClassName = ((ConstantClassInfo)FConstantPool.Item(FThisClass)).Name;
					(FConstantPool.Item(FThisClass)).References++;
					FSuperClassName = ((ConstantClassInfo)FConstantPool.Item(FSuperClass)).Name;
					(FConstantPool.Item(FSuperClass)).References++;

					FInterfaces = new TInterfaces(FReader, FConstantPool);
					FFields = new TFields(FReader, FConstantPool);
					FMethods = new TMethods(FReader, FConstantPool);
					FAttributes = new TAttributes(FReader, FConstantPool);

					//FHasBeenOpened = true;

					fs.Close();
					return true;
				}
				catch (Exception e)
				{
					// catch any unhandled exceptions here
					// and exit gracefully.
					// garbage collection does the rest ;D
					return false;
				}
			}

			return false;
		}
		public bool Save(string FileName)
		{
			if (true)//FHasBeenOpened)
			{
				try
				{
					// read the .class file systematically
					FileStream fs = new FileStream(FileName, FileMode.Create, FileAccess.Write);
					BinaryWriter FWriter = new BinaryWriter(fs);
					// write header
					Common.WriteDWord(FWriter, FMagic);

					Common.WriteWord(FWriter, FMinorVersion);
					Common.WriteWord(FWriter, FMajorVersion);
					// write constant pool
					// this also writes the "FConstantPoolCount"
					FConstantPool.Write(FWriter);
					// more constants
					Common.WriteWord(FWriter, (int)FAccessFlags);
					Common.WriteWord(FWriter, FThisClass + 1);
					Common.WriteWord(FWriter, FSuperClass + 1);

					FInterfaces.Write(FWriter);
					FFields.Write(FWriter);
					FMethods.Write(FWriter);
					FAttributes.Write(FWriter);

					FWriter.Close();
					return true;
				}
				catch (Exception e)
				{
					// catch any unhandled exceptions here
					// and exit gracefully.
					// garbage collection does the rest ;D
					return false;
				}
			}
		}
		public uint Magic
		{
			get
			{
				return FMagic;
			}
			set
			{
				FMagic = value;
			}
		}
		public string Version()
		{
			return FMajorVersion.ToString() + "." + FMinorVersion.ToString();
		}
		public string FileName
		{
			get
			{
				return FClassFileName;
			}
		}
		public AccessFlags AccessFlags
		{
			get
			{
				return FAccessFlags;
			}
		}

		public TConstantPool ConstantPool
		{
			get
			{
				return FConstantPool;
			}
		}
		public TInterfaces Interfaces
		{
			get
			{
				return FInterfaces;
			}
		}
		public TFields Fields
		{
			get
			{
				return FFields;
			}
		}
		public TMethods Methods
		{
			get
			{
				return FMethods;
			}
		}
		public TAttributes Attributes
		{
			get
			{
				return FAttributes;
			}
		}

		public TChangeRecord ChangeMethodName(int MethodNumber, string NewName)
		{
			MethodInfo Method = (MethodInfo)FMethods.Items[MethodNumber];
			//MethodInfo OriginalMethod = Method.Clone();
			//MethodInfo NewMethod = null;
			TChangeRecord Result = null;
			ConstantMethodrefInfo MethodRef = null;
			ushort NewNameIndex;

			// first we need to loop through the constant pool for method 
			// references that match our new method name
			for (int i = 0; i < FConstantPool.MaxItems(); i++)
			{
				if (FConstantPool.Item(i).Tag == (byte)ConstantPoolInfoTag.ConstantMethodref)
				{
					MethodRef = (ConstantMethodrefInfo)FConstantPool.Item(i);
					if (MethodRef.ParentClass.Name == FThisClassName &&
						MethodRef.NameAndType.Name == Method.Name.Value &&
						MethodRef.NameAndType.Descriptor == Method.Descriptor)
					{
						// jackpot, we found the reference!
						// there should be only one, so we will break and fix it up after we generate the new name
						break;
					}
				}

				MethodRef = null;
			}

			Method.Name.References--;
			// add a new string constant to the pool
			ConstantUtf8Info NewUtf = new ConstantUtf8Info(NewName);

			NewNameIndex = ConstantPool.Add(NewUtf);

			// set the method its new name
			Method.SetName(NewNameIndex, ConstantPool);
			Method.Name.References = 1;

			//NewMethod = Method.Clone();

			if (MethodRef == null)
				return Result;

			if (MethodRef.NameAndType.References <= 1)
			{
				// if this is the only reference to the name/type descriptor
				// we can overwrite the value
				MethodRef.NameAndType.SetName(NewNameIndex, FConstantPool);
			}
			else
			{
				// we have to make a new one !
				MethodRef.NameAndType.References--;
				// add a new string constant to the pool
				ConstantNameAndTypeInfo NewNaT = new ConstantNameAndTypeInfo(NewNameIndex, MethodRef.NameAndType.TypeIndex, FConstantPool);

				ushort NewIndex = ConstantPool.Add(NewNaT);

				// set the method its new name
				MethodRef.SetNameAndType(NewIndex, ConstantPool);
				MethodRef.NameAndType.References = 1;
			}

			return Result;
		}
		public TChangeRecord ChangeFieldName(int FieldNumber, string NewName)
		{
			FieldInfo Field = (FieldInfo)FFields.Items[FieldNumber];
			//FieldInfo OriginalFieldInfo = Field.Clone();
			//FieldInfo NewField = null;
			TChangeRecord Result = null;
			ConstantFieldrefInfo FieldRef = null;
			ushort NewNameIndex;

			// first we need to loop through the constant pool for method 
			// references that match our new method name
			for (int i = 0; i < FConstantPool.MaxItems(); i++)
			{
				if (FConstantPool.Item(i).Tag == (byte)ConstantPoolInfoTag.ConstantFieldref)
				{
					FieldRef = (ConstantFieldrefInfo)FConstantPool.Item(i);
					if (FieldRef.ParentClass.Name == FThisClassName &&
						FieldRef.NameAndType.Name == Field.Name.Value &&
						FieldRef.NameAndType.Descriptor == Field.Descriptor)
					{
						// jackpot, we found the reference!
						// there should be only one, so we will break and fix it up after we generate the new name
						break;
					}
				}

				FieldRef = null;
			}

			Field.Name.References--;

			// add a new string constant to the pool
			ConstantUtf8Info NewUtf = new ConstantUtf8Info(NewName);

			NewNameIndex = ConstantPool.Add(NewUtf);

			// set the method its new name
			Field.SetName(NewNameIndex, ConstantPool);
			Field.Name.References = 1;

			//NewField = Field.Clone();

			if (FieldRef == null)
				return Result;

			if (FieldRef.NameAndType.References <= 1)
			{
				// if this is the only reference to the name/type descriptor
				// we can overwrite the value
				FieldRef.NameAndType.SetName(NewNameIndex, FConstantPool);
			}
			else
			{
				// we have to make a new one !
				FieldRef.NameAndType.References--;
				// add a new string constant to the pool
				ConstantNameAndTypeInfo NewNaT = new ConstantNameAndTypeInfo(NewNameIndex, FieldRef.NameAndType.TypeIndex, FConstantPool);

				ushort NewIndex = ConstantPool.Add(NewNaT);

				// set the method its new name
				FieldRef.SetNameAndType(NewIndex, ConstantPool);
				FieldRef.NameAndType.References = 1;
			}

			return Result;
		}
		public void ChangeConstantFieldName(int FieldNumber, string NewName)
		{
			// takes an index into the constantpool
			// simple changes the name of a method/field in the constant pool
			// always create new name 
			// TODO: check this!

			ConstantPoolMethodInfo FieldRef = (ConstantPoolMethodInfo)FConstantPool.Item(FieldNumber);

			ConstantUtf8Info NewNameString = new ConstantUtf8Info(NewName);
			ushort NewNameIndex = FConstantPool.Add(NewNameString);

			// we have to make a new one !
			FieldRef.NameAndType.References--;
			// add a new string constant to the pool
			ConstantNameAndTypeInfo NewNaT = new ConstantNameAndTypeInfo(NewNameIndex, FieldRef.NameAndType.TypeIndex, FConstantPool);

			ushort NewIndex = FConstantPool.Add(NewNaT);

			// set the method its new name
			FieldRef.SetNameAndType(NewIndex, FConstantPool);
			FieldRef.NameAndType.References = 1;
		}
		public void ChangeConstantFieldParent(int FieldNumber, int ParentNumber)
		{
			ConstantPoolMethodInfo FieldRef = (ConstantPoolMethodInfo)FConstantPool.Item(FieldNumber);

			FieldRef.ParentClass.References--;
			FieldRef.SetParent((ushort)ParentNumber, FConstantPool);
		}
		public void ChangeConstantFieldType(int FieldNumber, string OldParentName, string NewParentName)
		{
			// takes an index into the constantpool
			// simple changes the name of a method/field in the constant pool
			// always create new name 
			// TODO: check this!

			ConstantPoolMethodInfo FieldRef = (ConstantPoolMethodInfo)FConstantPool.Item(FieldNumber);
			string OldName = FieldRef.NameAndType.Descriptor;
			string NewName = Common.FixDescriptor(FieldRef.NameAndType.Descriptor, OldParentName, NewParentName);

			if (OldName == NewName)
				return;

			ConstantUtf8Info NewTypeString = new ConstantUtf8Info(NewName);
			ushort NewTypeIndex = FConstantPool.Add(NewTypeString);

			FieldRef.NameAndType.SetType(NewTypeIndex, FConstantPool);
		}
		public void ChangeFieldType(int FieldNumber, string OldParentName, string NewParentName)
		{
			// takes an index into the constantpool
			// simple changes the name of a method/field in the constant pool
			// TODO: check this!

			FieldInfo FieldRef = FFields.Item(FieldNumber);

			string OldName = FieldRef.Descriptor;
			string NewName = Common.FixDescriptor(FieldRef.Descriptor, OldParentName, NewParentName);

			if (OldName == NewName)
				return;

			ConstantUtf8Info NewTypeString = new ConstantUtf8Info(NewName);
			ushort NewTypeIndex = FConstantPool.Add(NewTypeString);

			// set the method its new name
			FieldRef.SetType(NewTypeIndex, FConstantPool);
		}
		public void ChangeMethodParam(int MethodNumber, string OldParentName, string NewParentName)
		{
			// takes an index into the constantpool
			// simple changes the name of a method/field in the constant pool
			// TODO: check this!

			MethodInfo MethodRef = FMethods.Item(MethodNumber);

			string OldName = MethodRef.Descriptor;
			string NewName = Common.FixDescriptor(MethodRef.Descriptor, OldParentName, NewParentName);

			if (OldName == NewName)
				return;

			ConstantUtf8Info NewTypeString = new ConstantUtf8Info(NewName);
			ushort NewTypeIndex = FConstantPool.Add(NewTypeString);

			// set the method its new name
			MethodRef.SetType(NewTypeIndex, FConstantPool);
		}
		public void ChangeInterfaceName(int InterfaceNumber, string NewName)
		{
			// takes an index into the interface list
			// simple changes the name of a method/field in the constant pool
			// TODO: check this!

			InterfaceInfo IntInfo = FInterfaces.Item(InterfaceNumber);

			if (IntInfo.Name == NewName)
				return;

			ConstantUtf8Info NewTypeString = new ConstantUtf8Info(NewName);
			ushort NewTypeIndex = FConstantPool.Add(NewTypeString);

			// set the interface its new name
			ConstantClassInfo cci = (ConstantClassInfo)ConstantPool.Item(IntInfo.Value);
			cci.SetName(NewTypeIndex, FConstantPool);
		}
		public string ThisClassName
		{
			get
			{
				return FThisClassName;
			}
		}
		public string SuperClassName
		{
			get
			{
				return FSuperClassName;
			}
		}
		public string ChangeClassName(string Name)
		{
			ConstantClassInfo ClassInfo = (ConstantClassInfo)FConstantPool.Item(FThisClass);
			ConstantUtf8Info UtfInfo = (ConstantUtf8Info)FConstantPool.Item(ClassInfo.NameIndex);

			// change the class name, not the directory structure
			Name = Common.NewClassName(ThisClassName, Name);

			// we have to make a new one !
			UtfInfo.References--;
			// add a new string constant to the pool
			ConstantUtf8Info NewUtf = new ConstantUtf8Info(Name);

			ushort NewIndex = ConstantPool.Add(NewUtf);

			// set the method its new name
			ClassInfo.SetName(NewIndex, FConstantPool);
			NewUtf.References = 1;

			FThisClassName = ((ConstantClassInfo)FConstantPool.Item(FThisClass)).Name;

			return Name;
		}
		public int ChangeSuperClassName(string NewName)
		{
			ConstantClassInfo ClassInfo = (ConstantClassInfo)FConstantPool.Item(FSuperClass);
			ConstantUtf8Info UtfInfo = (ConstantUtf8Info)FConstantPool.Item(ClassInfo.NameIndex);

			// skip this coz we already passing the full name in
			//NewName = Common.NewClassName(FSuperClassName, NewName);

			if (UtfInfo.References <= 1)
			{
				// if this is the only reference to the name/type descriptor
				// we can overwrite the value
				UtfInfo.SetName(NewName);
			}
			else
			{
				// we have to make a new one !
				UtfInfo.References--;
				// add a new string constant to the pool
				ConstantUtf8Info NewUtf = new ConstantUtf8Info(NewName);

				ushort NewIndex = ConstantPool.Add(NewUtf);

				// set the method its new name
				ClassInfo.NameIndex = NewIndex;
				NewUtf.References = 1;
			}

			FSuperClassName = ((ConstantClassInfo)FConstantPool.Item(FSuperClass)).Name;

			return FSuperClass;
		}
		public int AddConstantClassName(string NewName)
		{
			ConstantClassInfo ClassInfo = new ConstantClassInfo();
			ConstantUtf8Info UtfInfo = new ConstantUtf8Info();

			ushort NewClassIndex = FConstantPool.Add(ClassInfo);
			ushort NewUtfIndex = FConstantPool.Add(UtfInfo);

			UtfInfo.SetName(NewName);
			ClassInfo.SetName(NewUtfIndex, FConstantPool);

			return NewClassIndex;
		}

	}

	//  ********************************************************************************   //
	//  *************************** CLASS CHANGE RECORD ********************************   //
	//  ********************************************************************************   //
	//  These classes are used to keep track of all the changes i make during deobfuscation
	//  of a single class. They are then used to iterate through all the rest of the files
	//  in the current "project" and fix up any references to the methods/fields we changed

	abstract class TChangeRecord { }
	class TMethodChangeRecord : TChangeRecord
	{
		// just a simple class to hold the information temporarily
		private MethodInfo FOriginalMethod;
		private MethodInfo FNewMethod;

		public TMethodChangeRecord(MethodInfo Original)
		{
			FOriginalMethod = Original.Clone();
		}
		public void ChangedTo(MethodInfo New)
		{
			FNewMethod = New.Clone();
		}
		public MethodInfo OriginalMethod
		{
			get
			{
				return FOriginalMethod;
			}
		}
		public MethodInfo NewMethod
		{
			get
			{
				return FNewMethod;
			}
		}
	}
	class TFieldChangeRecord : TChangeRecord
	{
		// just a simple class to hold the information temporarily
		private FieldInfo FOriginalField;
		private FieldInfo FNewField;

		public TFieldChangeRecord(FieldInfo Original)
		{
			FOriginalField = Original.Clone();
		}
		public void ChangedTo(FieldInfo New)
		{
			FNewField = New.Clone();
		}
		public FieldInfo OriginalField
		{
			get
			{
				return FOriginalField;
			}
		}
		public FieldInfo NewField
		{
			get
			{
				return FNewField;
			}
		}
	}

	//  ********************************************************************************   //
	//  **************************** INDIVIDUAL CLASSES ********************************   //
	//  ********************************************************************************   //
	//  These are all used by TClassFile to import each of its major sections

	class TConstantPool
	{
		BinaryReader FReader;
		ArrayList FItems = null;

		int FMaxItems = 0;

		public TConstantPool(BinaryReader Reader)
		{
			FReader = Reader;

			FMaxItems = Common.ReadWord(FReader) - 1;
			FItems = new ArrayList();
			int count = 0;

			// goes from 1 -> constantpoolcount - 1
			while (count < FMaxItems)
			{
				byte tag = Common.ReadByte(FReader);

				switch (tag)
				{
					case (byte)ConstantPoolInfoTag.ConstantClass:
						{
							ConstantClassInfo cc = new ConstantClassInfo();
							cc.Read(tag, FReader);
							FItems.Add(cc);
							break;
						}
					case (byte)ConstantPoolInfoTag.ConstantString:
						{
							ConstantStringInfo cc = new ConstantStringInfo();
							cc.Read(tag, FReader);
							FItems.Add(cc);
							break;
						}
					case (byte)ConstantPoolInfoTag.ConstantFieldref:
						{
							ConstantFieldrefInfo cc = new ConstantFieldrefInfo();
							cc.Read(tag, FReader);
							FItems.Add(cc);
							break;
						}
					case (byte)ConstantPoolInfoTag.ConstantMethodref:
						{
							ConstantMethodrefInfo cc = new ConstantMethodrefInfo();
							cc.Read(tag, FReader);
							FItems.Add(cc);
							break;
						}
					case (byte)ConstantPoolInfoTag.ConstantInterfaceMethodref:
						{
							ConstantInterfaceMethodrefInfo cc = new ConstantInterfaceMethodrefInfo();
							cc.Read(tag, FReader);
							FItems.Add(cc);
							break;
						}
					case (byte)ConstantPoolInfoTag.ConstantInteger:
						{
							ConstantIntegerInfo cc = new ConstantIntegerInfo();
							cc.Read(tag, FReader);
							FItems.Add(cc);
							break;
						}
					case (byte)ConstantPoolInfoTag.ConstantFloat:
						{
							ConstantFloatInfo cc = new ConstantFloatInfo();
							cc.Read(tag, FReader);
							FItems.Add(cc);
							break;
						}
					case (byte)ConstantPoolInfoTag.ConstantLong:
						{
							ConstantLongInfo cc = new ConstantLongInfo();
							cc.Read(tag, FReader);
							FItems.Add(cc);
							// longs take up two entries in the pool table
							count++;
							FItems.Add(cc);
							break;
						}
					case (byte)ConstantPoolInfoTag.ConstantDouble:
						{
							ConstantDoubleInfo cc = new ConstantDoubleInfo();
							cc.Read(tag, FReader);
							FItems.Add(cc);
							// so do doubles
							count++;
							FItems.Add(cc);
							break;
						}
					case (byte)ConstantPoolInfoTag.ConstantNameAndType:
						{
							ConstantNameAndTypeInfo cc = new ConstantNameAndTypeInfo();
							cc.Read(tag, FReader);
							FItems.Add(cc);
							break;
						}
					case (byte)ConstantPoolInfoTag.ConstantUtf8:
						{
							ConstantUtf8Info cc = new ConstantUtf8Info();
							cc.Read(tag, FReader);
							FItems.Add(cc);
							break;
						}

					default:
						// fail safe ?
						count++;
						break;
				}

				count++;
			}

			foreach (ConstantPoolInfo cc in FItems)
			{
				cc.Resolve(FItems);
			}
		}
		public void Write(BinaryWriter Writer)
		{
			// i am assuming we have a valid constant pool list...
			// i dont do any error checking here except bare minimum!

			// write the number of constant pool entries
			Common.WriteWord(Writer, FMaxItems + 1);
			int count = 0;

			// goes from 1 -> constantpoolcount - 1
			while (count < FMaxItems)
			{
				ConstantPoolInfo Item = (ConstantPoolInfo)FItems[count];

				switch (Item.Tag)
				{
					case (byte)ConstantPoolInfoTag.ConstantClass:
						{
							ConstantClassInfo cc = (ConstantClassInfo)Item;
							cc.Write(Writer);

							break;
						}
					case (byte)ConstantPoolInfoTag.ConstantString:
						{
							ConstantStringInfo cc = (ConstantStringInfo)Item;
							cc.Write(Writer);

							break;
						}
					case (byte)ConstantPoolInfoTag.ConstantFieldref:
						{
							ConstantFieldrefInfo cc = (ConstantFieldrefInfo)Item;
							cc.Write(Writer);

							break;
						}
					case (byte)ConstantPoolInfoTag.ConstantMethodref:
						{
							ConstantMethodrefInfo cc = (ConstantMethodrefInfo)Item;
							cc.Write(Writer);

							break;
						}
					case (byte)ConstantPoolInfoTag.ConstantInterfaceMethodref:
						{
							ConstantInterfaceMethodrefInfo cc = (ConstantInterfaceMethodrefInfo)Item;
							cc.Write(Writer);

							break;
						}
					case (byte)ConstantPoolInfoTag.ConstantInteger:
						{
							ConstantIntegerInfo cc = (ConstantIntegerInfo)Item;
							cc.Write(Writer);

							break;
						}
					case (byte)ConstantPoolInfoTag.ConstantFloat:
						{
							ConstantFloatInfo cc = (ConstantFloatInfo)Item;
							cc.Write(Writer);

							break;
						}
					case (byte)ConstantPoolInfoTag.ConstantLong:
						{
							ConstantLongInfo cc = (ConstantLongInfo)Item;
							cc.Write(Writer);

							// longs take up two entries in the pool table
							count++;
							break;
						}
					case (byte)ConstantPoolInfoTag.ConstantDouble:
						{
							ConstantDoubleInfo cc = (ConstantDoubleInfo)Item;
							cc.Write(Writer);

							// so do doubles
							count++;
							break;
						}
					case (byte)ConstantPoolInfoTag.ConstantNameAndType:
						{
							ConstantNameAndTypeInfo cc = (ConstantNameAndTypeInfo)Item;
							cc.Write(Writer);

							break;
						}
					case (byte)ConstantPoolInfoTag.ConstantUtf8:
						{
							ConstantUtf8Info cc = (ConstantUtf8Info)Item;
							cc.Write(Writer);

							break;
						}

					default:
						// fail safe ?
						// BADDDDDDDDDDDDDDDDDDDDD, prolly should check/fix this
						count++;
						break;
				}

				count++;
			}
		}
		public int MaxItems()
		{
			return FMaxItems;
		}
		public ConstantPoolInfo Item(int Index)
		{
			if (FItems != null && Index < FMaxItems)
				return (ConstantPoolInfo)FItems[Index];

			return null;
		}
		public ushort Add(ConstantPoolInfo NewItem)
		{
			FItems.Add(NewItem);
			FMaxItems++;
			return (ushort)(FItems.Count - 1);
		}
	}
	class TInterfaces
	{
		BinaryReader FReader;
		ArrayList FItems = null;

		int FMaxItems = 0;

		public TInterfaces(BinaryReader Reader, TConstantPool ConstantPool)
		{
			FReader = Reader;

			FMaxItems = Common.ReadWord(FReader) - 1;
			FItems = new ArrayList();
			int count = 0;

			// goes from 1 -> interfacecount - 1
			while (count <= FMaxItems)
			{
				InterfaceInfo ii = new InterfaceInfo(FReader, ConstantPool);
				FItems.Add(ii);

				count++;
			}
		}

		public void Write(BinaryWriter Writer)
		{
			Common.WriteWord(Writer, FMaxItems + 1);

			int count = 0;

			// goes from 1 -> interfacecount - 1
			while (count <= FMaxItems)
			{
				InterfaceInfo ii = (InterfaceInfo)FItems[count];
				ii.Write(Writer);

				count++;
			}
		}

		public int MaxItems()
		{
			return FMaxItems;
		}

		public InterfaceInfo Item(int Index)
		{
			if (Index >= 0 && Index < FItems.Count)
				return (InterfaceInfo)FItems[Index];

			// TODO: fix this fucking gay piece of shit
			return (InterfaceInfo)FItems[0];
		}

		public ArrayList Items
		{
			get
			{
				return FItems;
			}
		}
	}
	class TFields
	{
		BinaryReader FReader;
		ArrayList FItems = null;

		int FMaxItems = 0;

		public TFields(BinaryReader Reader, TConstantPool ConstantPool)
		{
			FReader = Reader;

			FMaxItems = Common.ReadWord(FReader);
			FItems = new ArrayList();
			int count = 0;

			// goes from 1 -> fieldcount - 1
			while (count < FMaxItems)
			{
				FieldInfo fi = new FieldInfo(FReader, ConstantPool);
				FItems.Add(fi);

				count++;
			}
		}

		public void Write(BinaryWriter Writer)
		{
			Common.WriteWord(Writer, FMaxItems);

			int count = 0;

			// goes from 1 -> fieldcount - 1
			while (count < FMaxItems)
			{
				FieldInfo fi = (FieldInfo)FItems[count];
				fi.Write(Writer);

				count++;
			}
		}

		public int MaxItems()
		{
			return FMaxItems;
		}

		public FieldInfo Item(int Index)
		{
			if (FItems != null && Index < FMaxItems)
				return (FieldInfo)FItems[Index];

			return null;
		}

		public ArrayList Items
		{
			get
			{
				return FItems;
			}
		}
	}
	class TMethods
	{
		BinaryReader FReader;
		ArrayList FItems = null;

		int FMaxItems = 0;

		public TMethods(BinaryReader Reader, TConstantPool ConstantPool)
		{
			FReader = Reader;

			FMaxItems = Common.ReadWord(FReader);
			FItems = new ArrayList();
			int count = 0;

			// goes from 1 -> fieldcount - 1
			while (count < FMaxItems)
			{
				MethodInfo mi = new MethodInfo(FReader, ConstantPool);
				FItems.Add(mi);

				count++;
			}
		}

		public void Write(BinaryWriter Writer)
		{
			Common.WriteWord(Writer, FMaxItems);

			int count = 0;

			// goes from 1 -> fieldcount - 1
			while (count < FMaxItems)
			{
				MethodInfo mi = (MethodInfo)FItems[count];
				mi.Write(Writer);

				count++;
			}
		}

		public int MaxItems()
		{
			return FMaxItems;
		}

		public MethodInfo Item(int Index)
		{
			if (FItems != null && Index < FMaxItems)
				return (MethodInfo)FItems[Index];

			return null;
		}

		public ArrayList Items
		{
			get
			{
				return FItems;
			}
		}

		public bool MethodNameExists(string Name)
		{
			for (int i = 0; i < FMaxItems; i++)
			{
				if (Name == ((MethodInfo)FItems[i]).Name.Value)
					return true;
			}

			return false;
		}

		public bool FieldNameExists(string Name)
		{
			for (int i = 0; i < FMaxItems; i++)
			{
				if (Name == ((FieldInfo)FItems[i]).Name.Value)
					return true;
			}

			return false;
		}
	}
	class TAttributes
	{
		BinaryReader FReader;
		ArrayList FItems = null;

		int FMaxItems = 0;

		public TAttributes(BinaryReader Reader, TConstantPool ConstantPool)
		{
			FReader = Reader;

			FMaxItems = Common.ReadWord(FReader) - 1;
			FItems = new ArrayList();
			int count = 0;

			// goes from 1 -> attributescount - 1
			while (count <= FMaxItems)
			{
				ushort NameIndex = Common.ReadWord(FReader);
				NameIndex--;
				ConstantUtf8Info Name = (ConstantUtf8Info)ConstantPool.Item(NameIndex);

				switch (Name.Value)
				{
					case AttributeType.Code:
						{
							CodeAttributeInfo ai = new CodeAttributeInfo(NameIndex, FReader, ConstantPool);

							FItems.Add(ai);
							break;
						}
					case AttributeType.ConstantValue:
						{
							ConstantValueAttributeInfo ai = new ConstantValueAttributeInfo(NameIndex, FReader, ConstantPool);

							FItems.Add(ai);
							break;
						}
					case AttributeType.Deprecated:
						{
							DeprecatedAttributeInfo ai = new DeprecatedAttributeInfo(NameIndex, FReader, ConstantPool);

							FItems.Add(ai);
							break;
						}
					case AttributeType.Exceptions:
						{
							ExceptionsAttributeInfo ai = new ExceptionsAttributeInfo(NameIndex, FReader, ConstantPool);

							FItems.Add(ai);
							break;
						}
					case AttributeType.InnerClasses:
						{
							InnerClassesAttributeInfo ai = new InnerClassesAttributeInfo(NameIndex, FReader, ConstantPool);

							FItems.Add(ai);
							break;
						}
					case AttributeType.LineNumberTable:
						{
							LineNumberAttributeInfo ai = new LineNumberAttributeInfo(NameIndex, FReader, ConstantPool);

							FItems.Add(ai);
							break;
						}
					case AttributeType.LocalVariableTable:
						{
							LocalVariablesAttributeInfo ai = new LocalVariablesAttributeInfo(NameIndex, FReader, ConstantPool);

							FItems.Add(ai);
							break;
						}
					case AttributeType.SourceFile:
						{
							SourceFileAttributeInfo ai = new SourceFileAttributeInfo(NameIndex, FReader, ConstantPool);

							FItems.Add(ai);
							break;
						}
					case AttributeType.Synthetic:
						{
							SyntheticAttributeInfo ai = new SyntheticAttributeInfo(NameIndex, FReader, ConstantPool);

							FItems.Add(ai);
							break;
						}

					default:
						{
							AttributeInfo ai = new UnknownAttributeInfo(NameIndex, FReader, ConstantPool);

							FItems.Add(ai);
							break;
						}
				}

				count++;
			}
		}

		public void Write(BinaryWriter Writer)
		{
			Common.WriteWord(Writer, FMaxItems + 1);

			int count = 0;

			// goes from 1 -> attributescount - 1
			while (count <= FMaxItems)
			{
				AttributeInfo Item = (AttributeInfo)FItems[count];

				Item.Write(Writer);

				count++;
			}
		}

		public ArrayList Items
		{
			get
			{
				return FItems;
			}
		}
	}

} // end
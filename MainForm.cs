using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace JavaDeObfuscator
{
	public class MainForm : System.Windows.Forms.Form
	{
		TDeObfuscator DeObfuscator = null;
		ArrayList Files = null;
	    RenameDatabase RenameStore = null;

		private Label label1;
		private OpenFileDialog OpenFileDialog;
		private TextBox ClassFileTextBox;
		private Button ButtonFileBrowse;
		private TreeView TreeClassView;
        private Button ProcessButton;
		private ToolTip ToolTip;
		private CheckBox RenameClassCheckBox;
		private CheckBox SmartRenameMethods;
        private ProgressBar Progress;
        private CheckBox CleanupCheckBox;
		private IContainer components;

		public MainForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.ClassFileTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ButtonFileBrowse = new System.Windows.Forms.Button();
            this.TreeClassView = new System.Windows.Forms.TreeView();
            this.OpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.ProcessButton = new System.Windows.Forms.Button();
            this.ToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.RenameClassCheckBox = new System.Windows.Forms.CheckBox();
            this.SmartRenameMethods = new System.Windows.Forms.CheckBox();
            this.Progress = new System.Windows.Forms.ProgressBar();
            this.CleanupCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // ClassFileTextBox
            // 
            this.ClassFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ClassFileTextBox.Location = new System.Drawing.Point(82, 16);
            this.ClassFileTextBox.Name = "ClassFileTextBox";
            this.ClassFileTextBox.Size = new System.Drawing.Size(448, 20);
            this.ClassFileTextBox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 23);
            this.label1.TabIndex = 1;
            this.label1.Text = "Add Class:";
            // 
            // ButtonFileBrowse
            // 
            this.ButtonFileBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonFileBrowse.Location = new System.Drawing.Point(532, 14);
            this.ButtonFileBrowse.Name = "ButtonFileBrowse";
            this.ButtonFileBrowse.Size = new System.Drawing.Size(24, 24);
            this.ButtonFileBrowse.TabIndex = 2;
            this.ButtonFileBrowse.Text = "...";
            this.ButtonFileBrowse.Click += new System.EventHandler(this.button1_Click);
            // 
            // TreeClassView
            // 
            this.TreeClassView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TreeClassView.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TreeClassView.Location = new System.Drawing.Point(8, 43);
            this.TreeClassView.Name = "TreeClassView";
            this.TreeClassView.ShowNodeToolTips = true;
            this.TreeClassView.Size = new System.Drawing.Size(546, 290);
            this.TreeClassView.TabIndex = 13;
            this.TreeClassView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreeClassView_NodeMouseClick);
            // 
            // OpenFileDialog
            // 
            this.OpenFileDialog.Filter = "Class Files|*.class";
            this.OpenFileDialog.Multiselect = true;
            // 
            // ProcessButton
            // 
            this.ProcessButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ProcessButton.Location = new System.Drawing.Point(454, 339);
            this.ProcessButton.Name = "ProcessButton";
            this.ProcessButton.Size = new System.Drawing.Size(100, 23);
            this.ProcessButton.TabIndex = 15;
            this.ProcessButton.Text = "Deobfuscate";
            this.ProcessButton.Click += new System.EventHandler(this.ProcessButton_Click);
            // 
            // ToolTip
            // 
            this.ToolTip.IsBalloon = true;
            // 
            // RenameClassCheckBox
            // 
            this.RenameClassCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.RenameClassCheckBox.AutoSize = true;
            this.RenameClassCheckBox.Checked = true;
            this.RenameClassCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.RenameClassCheckBox.Location = new System.Drawing.Point(131, 344);
            this.RenameClassCheckBox.Name = "RenameClassCheckBox";
            this.RenameClassCheckBox.Size = new System.Drawing.Size(105, 17);
            this.RenameClassCheckBox.TabIndex = 18;
            this.RenameClassCheckBox.Text = "Rename Classes";
            // 
            // SmartRenameMethods
            // 
            this.SmartRenameMethods.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SmartRenameMethods.AutoSize = true;
            this.SmartRenameMethods.Checked = true;
            this.SmartRenameMethods.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SmartRenameMethods.Enabled = false;
            this.SmartRenameMethods.Location = new System.Drawing.Point(252, 343);
            this.SmartRenameMethods.Name = "SmartRenameMethods";
            this.SmartRenameMethods.Size = new System.Drawing.Size(140, 17);
            this.SmartRenameMethods.TabIndex = 19;
            this.SmartRenameMethods.Text = "Smart Rename Methods";
            // 
            // Progress
            // 
            this.Progress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Progress.Location = new System.Drawing.Point(12, 367);
            this.Progress.Name = "Progress";
            this.Progress.Size = new System.Drawing.Size(546, 15);
            this.Progress.Step = 1;
            this.Progress.TabIndex = 20;
            this.Progress.Visible = false;
            // 
            // CleanupCheckBox
            // 
            this.CleanupCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CleanupCheckBox.AutoSize = true;
            this.CleanupCheckBox.Checked = true;
            this.CleanupCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CleanupCheckBox.Location = new System.Drawing.Point(12, 343);
            this.CleanupCheckBox.Name = "CleanupCheckBox";
            this.CleanupCheckBox.Size = new System.Drawing.Size(109, 17);
            this.CleanupCheckBox.TabIndex = 21;
            this.CleanupCheckBox.Text = "Remove Old Files";
            this.CleanupCheckBox.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(565, 385);
            this.Controls.Add(this.CleanupCheckBox);
            this.Controls.Add(this.Progress);
            this.Controls.Add(this.SmartRenameMethods);
            this.Controls.Add(this.RenameClassCheckBox);
            this.Controls.Add(this.ProcessButton);
            this.Controls.Add(this.TreeClassView);
            this.Controls.Add(this.ButtonFileBrowse);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ClassFileTextBox);
            this.Name = "MainForm";
            this.Text = "Java DeObfuscator v1.6b";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MainForm());
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (OpenFileDialog.ShowDialog() == DialogResult.OK)
			{
                if (Files == null)
                    Files = new ArrayList();
	
                foreach (String fn in OpenFileDialog.FileNames)
                {
                    Files.Add(fn);
                }

				UpdateTree();
			}
		}

		private void UpdateTree()
		{
			TreeClassView.Nodes.Clear();

            DeObfuscator = new TDeObfuscator(Files);

            foreach (string fn in Files)
			{
				TClassFile ClassFile = new TClassFile(fn);

				if (!ClassFile.Open())
				{
					TreeClassView.Nodes.Add("Invalid class file: " + fn);
					continue;
				}

				if (ClassFile != null)
				{
				    TreeNode bigroot;
				    
				    // check if the user wants to rename the class file
                    string original_class_name = ClassFile.ThisClassName + " : " + ClassFile.SuperClassName;
                    string class_name = RenameStore.GetNewClassName(original_class_name);

                    if (class_name == null)
                    {
                        class_name = original_class_name;
                        bigroot = TreeClassView.Nodes.Add(class_name);
                    }
				    else
                    {
                        bigroot = TreeClassView.Nodes.Add(class_name);
                        bigroot.BackColor = Color.DodgerBlue;
                    }
	    
                    bigroot.Tag = original_class_name;

					TreeNode root = bigroot.Nodes.Add("Constants");
					TreeNode methodsroot = root.Nodes.Add("Methods/Interfaces/Fields");
					TreeNode methods = methodsroot.Nodes.Add("Methods");
					TreeNode interfaces = methodsroot.Nodes.Add("Interfaces");
					TreeNode fields = methodsroot.Nodes.Add("Fields");
					TreeNode variables = root.Nodes.Add("Values");
					TreeNode classes = root.Nodes.Add("Classes");

					for (int i = 0; i < ClassFile.ConstantPool.MaxItems(); i++)
					{
						ConstantPoolInfo cc = ClassFile.ConstantPool.Item(i);

						if (cc is ConstantPoolMethodInfo)
						{
							if (cc is ConstantMethodrefInfo)
							{
								TreeNode temp = methods.Nodes.Add("\"" + ((ConstantMethodrefInfo)cc).NameAndType.Name + "\"");
								temp.Nodes.Add("Descriptor = " + ((ConstantMethodrefInfo)cc).NameAndType.Descriptor);
								temp.Nodes.Add("Parent = " + ((ConstantMethodrefInfo)cc).ParentClass.Name);

								if (DeObfuscator.DoRename(((ConstantMethodrefInfo)cc).NameAndType.Name))
									temp.BackColor = Color.Red;

								continue;
							}

							if (cc is ConstantInterfaceMethodrefInfo)
							{
								TreeNode temp = interfaces.Nodes.Add("\"" + ((ConstantInterfaceMethodrefInfo)cc).NameAndType.Name + "\"");
								temp.Nodes.Add("Descriptor = " + ((ConstantInterfaceMethodrefInfo)cc).NameAndType.Descriptor);
								temp.Nodes.Add("Parent = " + ((ConstantInterfaceMethodrefInfo)cc).ParentClass.Name);

								if (DeObfuscator.DoRename(((ConstantInterfaceMethodrefInfo)cc).NameAndType.Name))
									temp.BackColor = Color.Red;

								continue;
							}

							if (cc is ConstantFieldrefInfo)
							{
								TreeNode temp = fields.Nodes.Add("\"" + ((ConstantFieldrefInfo)cc).NameAndType.Name + "\"");
								temp.Nodes.Add("Descriptor = " + ((ConstantFieldrefInfo)cc).NameAndType.Descriptor);
								if (((ConstantFieldrefInfo)cc).ParentClass != null)
									temp.Nodes.Add("Parent = " + ((ConstantFieldrefInfo)cc).ParentClass.Name);

								if (DeObfuscator.DoRename(((ConstantFieldrefInfo)cc).NameAndType.Name))
									temp.BackColor = Color.Red;

								continue;
							}
						}
						else
							if (cc is ConstantPoolVariableInfo)
							{
								TreeNode temp = variables.Nodes.Add("\"" + ((ConstantPoolVariableInfo)cc).Value.ToString() + "\"");
								temp.Nodes.Add("References = " + cc.References);
							}
							else
								if (cc is ConstantClassInfo)
								{
									TreeNode temp = classes.Nodes.Add("\"" + ((ConstantClassInfo)cc).Name + "\"");
									temp.Nodes.Add("References = " + cc.References);
								}
					}

					root = bigroot.Nodes.Add("Interfaces");
					foreach (InterfaceInfo ii in ClassFile.Interfaces.Items)
					{
						root.Nodes.Add(ii.Interface.Name);
					}

					root = bigroot.Nodes.Add("Fields");
					foreach (FieldInfo fi in ClassFile.Fields.Items)
					{
					    RenameData rd = RenameStore.GetNewFieldInfo(
                            original_class_name,
					        fi.Descriptor, 
					        fi.Name.Value);
					    if (rd != null)
					    {
                            TreeNode temp = root.Nodes.Add(rd.FieldName);
                            temp.Nodes.Add(rd.FieldType);
                            temp.BackColor = Color.DodgerBlue;
					    }
					    else 
					    {
					        TreeNode temp = root.Nodes.Add(fi.Name.Value);
					        temp.Nodes.Add(fi.Descriptor);
                            temp.Tag = fi.Name.Value;
    					        
					        if (DeObfuscator.DoRename(fi.Name.Value))
					            temp.BackColor = Color.Red;
					    }
					}

					root = bigroot.Nodes.Add("Methods");
					foreach (MethodInfo mi in ClassFile.Methods.Items)
					{
					    RenameData rd = RenameStore.GetNewMethodInfo(
                            original_class_name,
					        mi.Descriptor, 
					        mi.Name.Value);
                        if (rd != null)
                        {
                            TreeNode temp = root.Nodes.Add(rd.FieldName);
                            temp.Nodes.Add(rd.FieldType);
                            temp.BackColor = Color.DodgerBlue;
                        }
                        else
                        {
                            TreeNode temp = root.Nodes.Add(mi.Name.Value);
                            temp.Nodes.Add(mi.Descriptor);
                            temp.Tag = mi.Name.Value;
                            //temp.Nodes.Add(String.Format("Offset = {0:X}", mi.Offset));

                            if (DeObfuscator.DoRename(mi.Name.Value))
                                temp.BackColor = Color.Red;
                        }
					}
				}
			}
		}

		private void ProcessButton_Click(object sender, EventArgs e)
		{
			if (Files == null)
				return;

			DeObfuscator = new TDeObfuscator(Files);

			DeObfuscator.Cleanup = CleanupCheckBox.Checked;
            DeObfuscator.RenameClasses = RenameClassCheckBox.Checked;

			Progress.Maximum = Files.Count;
			Progress.Visible = true;

			TDeObfuscator.Progress += new TDeObfuscator.ProgressHandler(OnProgress);

			// update the classfile with the new deobfuscated version
            ArrayList NewFileList = DeObfuscator.DeObfuscateAll(RenameStore);
			if (NewFileList != null)
            {
				MessageBox.Show("DeObfuscated everything ok!", "DeObfuscator");
                Files = NewFileList;
            }
			else
				MessageBox.Show("Error!!!", "DeObfuscator");

			Progress.Visible = false;
		    RenameStore = new RenameDatabase();
			UpdateTree();
		}

        private void OnProgress(int progress)
		{
			// Progress 
            if (progress > Progress.Maximum)
                Progress.Value = 0;

			Progress.Value = progress;
		}

        private void TreeClassView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // detect right click on a valid member to popup a 'change name' box.
            if (e.Button == MouseButtons.Right && e.Node.Parent != null && e.Node.Parent.Parent != null)
            {
                ChangeName FChangeName = new ChangeName();
                FChangeName.NameBox.Text = e.Node.Text;
                // get the full path of the node we clicked on, so we have all the information
                // relating to it
                // get parentmost node
                TreeNode pn = e.Node;
                while (pn.Parent != null)
                {
                    pn = pn.Parent;
                }
                
                // get trailing node
                TreeNode tn = e.Node;
                while (tn.Nodes.Count > 0)
                {
                    tn = tn.Nodes[0];
                }

                string class_name = pn.Tag.ToString();                   // classname
                
                string[] sl = tn.FullPath.Split('\\');
                string type = sl[1];
                string old_name = tn.Parent.Tag.ToString();

                if (class_name == null || type == null || 
                    old_name == null)
                {
                    return;
                }

                // check which subsection we are in, so we can add it to the right list
                if ((type == "Methods" || type == "Fields") &&            // section
                    (FChangeName.ShowDialog() == DialogResult.OK))
                {
                    string old_descriptor = sl[3];

                    if (old_descriptor == null)
                        return;
                    
                    if (type == "Methods")
                    {
                        RenameStore.AddRenameMethod(class_name, old_descriptor, old_name,
                            old_descriptor, FChangeName.NameBox.Text);
                    }
                    else if (type == "Fields")
                    {
                        RenameStore.AddRenameField(class_name, old_descriptor, old_name,
                            old_descriptor, FChangeName.NameBox.Text);
                    }

                    // update the tree without reloading it
                    tn.Parent.Text = FChangeName.NameBox.Text;
                    tn.Parent.ToolTipText = "was '" + tn.Parent.Tag.ToString() + "'";
                    tn.Parent.BackColor = Color.DodgerBlue;
                }
            }
            else if (e.Button == MouseButtons.Right && e.Node.Parent == null)
            {
                ChangeName FChangeName = new ChangeName();
                string[] s = e.Node.Text.Split(':');

                string old_name = s[0].Trim();
                string old_descriptor = s[1].Trim();
                
                if (s.Length == 0)
                    return;
 
                FChangeName.NameBox.Text = old_name;
                
                // change the class name, since its a root node
                if (FChangeName.ShowDialog() == DialogResult.OK)
                {
                    string new_name_and_type = FChangeName.NameBox.Text + " : " + old_descriptor;
                    RenameStore.AddRenameClass(e.Node.Tag.ToString(), new_name_and_type);

                    e.Node.BackColor = Color.DodgerBlue;
                    e.Node.Text = new_name_and_type;
                    e.Node.ToolTipText = "was '" + e.Node.Tag.ToString() + "'";
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RenameStore = new RenameDatabase();
        }

        
	}
}

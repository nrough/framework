namespace Configuration
{
    partial class Configuration
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.ConfigurationName = new System.Windows.Forms.ComboBox();
            this.ConfigurationNameLabel = new System.Windows.Forms.Label();
            this.Tab = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.GeneralSettingsGroup = new System.Windows.Forms.GroupBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.DatabaseSettingGroup = new System.Windows.Forms.GroupBox();
            this.PasswordLabel = new System.Windows.Forms.Label();
            this.UserLabel = new System.Windows.Forms.Label();
            this.Password = new System.Windows.Forms.TextBox();
            this.UserId = new System.Windows.Forms.TextBox();
            this.AuthenticationTypeLabel = new System.Windows.Forms.Label();
            this.AuthenticationType = new System.Windows.Forms.ComboBox();
            this.Port = new System.Windows.Forms.TextBox();
            this.DatabaseName = new System.Windows.Forms.TextBox();
            this.ServerName = new System.Windows.Forms.TextBox();
            this.PortLabel = new System.Windows.Forms.Label();
            this.DatabaseNameLabel = new System.Windows.Forms.Label();
            this.ServerNameLabel = new System.Windows.Forms.Label();
            this.DatabaseTypeLabel = new System.Windows.Forms.Label();
            this.DatabaseType = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.okBtn = new System.Windows.Forms.Button();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.helpBtn = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.Tab.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.DatabaseSettingGroup.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.Tab, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(792, 589);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 400F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.ConfigurationName, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.ConfigurationNameLabel, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 38);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(786, 29);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // ConfigurationName
            // 
            this.ConfigurationName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConfigurationName.FormattingEnabled = true;
            this.ConfigurationName.Location = new System.Drawing.Point(123, 3);
            this.ConfigurationName.Name = "ConfigurationName";
            this.ConfigurationName.Size = new System.Drawing.Size(394, 21);
            this.ConfigurationName.TabIndex = 0;
            this.ConfigurationName.SelectedIndexChanged += new System.EventHandler(this.ConfigurationName_SelectedIndexChanged);
            // 
            // ConfigurationNameLabel
            // 
            this.ConfigurationNameLabel.AutoSize = true;
            this.ConfigurationNameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConfigurationNameLabel.Location = new System.Drawing.Point(3, 0);
            this.ConfigurationNameLabel.Name = "ConfigurationNameLabel";
            this.ConfigurationNameLabel.Size = new System.Drawing.Size(114, 29);
            this.ConfigurationNameLabel.TabIndex = 1;
            this.ConfigurationNameLabel.Text = "Configuration";
            this.ConfigurationNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.ConfigurationNameLabel.Click += new System.EventHandler(this.label1_Click);
            // 
            // Tab
            // 
            this.Tab.Controls.Add(this.tabPage1);
            this.Tab.Controls.Add(this.tabPage2);
            this.Tab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Tab.Location = new System.Drawing.Point(3, 73);
            this.Tab.Name = "Tab";
            this.Tab.SelectedIndex = 0;
            this.Tab.Size = new System.Drawing.Size(786, 463);
            this.Tab.TabIndex = 1;
            this.Tab.Tag = "";
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tableLayoutPanel5);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(778, 437);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "General";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel5.Controls.Add(this.GeneralSettingsGroup, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 431F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(772, 431);
            this.tableLayoutPanel5.TabIndex = 0;
            // 
            // GeneralSettingsGroup
            // 
            this.GeneralSettingsGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GeneralSettingsGroup.Location = new System.Drawing.Point(3, 3);
            this.GeneralSettingsGroup.Name = "GeneralSettingsGroup";
            this.GeneralSettingsGroup.Size = new System.Drawing.Size(766, 425);
            this.GeneralSettingsGroup.TabIndex = 0;
            this.GeneralSettingsGroup.TabStop = false;
            this.GeneralSettingsGroup.Text = "Settings";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tableLayoutPanel6);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(778, 437);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Database";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 1;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel6.Controls.Add(this.DatabaseSettingGroup, 0, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 431F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(772, 431);
            this.tableLayoutPanel6.TabIndex = 0;
            // 
            // DatabaseSettingGroup
            // 
            this.DatabaseSettingGroup.Controls.Add(this.PasswordLabel);
            this.DatabaseSettingGroup.Controls.Add(this.UserLabel);
            this.DatabaseSettingGroup.Controls.Add(this.Password);
            this.DatabaseSettingGroup.Controls.Add(this.UserId);
            this.DatabaseSettingGroup.Controls.Add(this.AuthenticationTypeLabel);
            this.DatabaseSettingGroup.Controls.Add(this.AuthenticationType);
            this.DatabaseSettingGroup.Controls.Add(this.Port);
            this.DatabaseSettingGroup.Controls.Add(this.DatabaseName);
            this.DatabaseSettingGroup.Controls.Add(this.ServerName);
            this.DatabaseSettingGroup.Controls.Add(this.PortLabel);
            this.DatabaseSettingGroup.Controls.Add(this.DatabaseNameLabel);
            this.DatabaseSettingGroup.Controls.Add(this.ServerNameLabel);
            this.DatabaseSettingGroup.Controls.Add(this.DatabaseTypeLabel);
            this.DatabaseSettingGroup.Controls.Add(this.DatabaseType);
            this.DatabaseSettingGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DatabaseSettingGroup.Location = new System.Drawing.Point(3, 3);
            this.DatabaseSettingGroup.Name = "DatabaseSettingGroup";
            this.DatabaseSettingGroup.Size = new System.Drawing.Size(766, 425);
            this.DatabaseSettingGroup.TabIndex = 0;
            this.DatabaseSettingGroup.TabStop = false;
            this.DatabaseSettingGroup.Text = "Settings";
            // 
            // PasswordLabel
            // 
            this.PasswordLabel.AutoSize = true;
            this.PasswordLabel.Location = new System.Drawing.Point(69, 170);
            this.PasswordLabel.Name = "PasswordLabel";
            this.PasswordLabel.Size = new System.Drawing.Size(56, 13);
            this.PasswordLabel.TabIndex = 27;
            this.PasswordLabel.Text = "Password:";
            // 
            // UserLabel
            // 
            this.UserLabel.AutoSize = true;
            this.UserLabel.Location = new System.Drawing.Point(93, 143);
            this.UserLabel.Name = "UserLabel";
            this.UserLabel.Size = new System.Drawing.Size(32, 13);
            this.UserLabel.TabIndex = 26;
            this.UserLabel.Text = "User:";
            // 
            // Password
            // 
            this.Password.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Configuration.Properties.Settings.Default, "Password", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.Password.Location = new System.Drawing.Point(135, 166);
            this.Password.Name = "Password";
            this.Password.PasswordChar = '*';
            this.Password.Size = new System.Drawing.Size(320, 20);
            this.Password.TabIndex = 25;
            this.Password.Text = global::Configuration.Properties.Settings.Default.Password;
            // 
            // UserId
            // 
            this.UserId.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Configuration.Properties.Settings.Default, "User", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.UserId.Location = new System.Drawing.Point(135, 139);
            this.UserId.Name = "UserId";
            this.UserId.Size = new System.Drawing.Size(320, 20);
            this.UserId.TabIndex = 24;
            this.UserId.Text = global::Configuration.Properties.Settings.Default.User;
            // 
            // AuthenticationTypeLabel
            // 
            this.AuthenticationTypeLabel.AutoSize = true;
            this.AuthenticationTypeLabel.Location = new System.Drawing.Point(24, 115);
            this.AuthenticationTypeLabel.Name = "AuthenticationTypeLabel";
            this.AuthenticationTypeLabel.Size = new System.Drawing.Size(101, 13);
            this.AuthenticationTypeLabel.TabIndex = 23;
            this.AuthenticationTypeLabel.Text = "Authentication type:";
            // 
            // AuthenticationType
            // 
            this.AuthenticationType.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Configuration.Properties.Settings.Default, "AuthenticationType", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.AuthenticationType.FormattingEnabled = true;
            this.AuthenticationType.Location = new System.Drawing.Point(135, 111);
            this.AuthenticationType.Name = "AuthenticationType";
            this.AuthenticationType.Size = new System.Drawing.Size(320, 21);
            this.AuthenticationType.TabIndex = 22;
            this.AuthenticationType.Text = global::Configuration.Properties.Settings.Default.AuthenticationType;
            // 
            // Port
            // 
            this.Port.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Configuration.Properties.Settings.Default, "ServerPort", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.Port.Location = new System.Drawing.Point(550, 57);
            this.Port.Name = "Port";
            this.Port.Size = new System.Drawing.Size(100, 20);
            this.Port.TabIndex = 21;
            this.Port.Text = global::Configuration.Properties.Settings.Default.ServerPort;
            // 
            // DatabaseName
            // 
            this.DatabaseName.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Configuration.Properties.Settings.Default, "DatabaseName", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.DatabaseName.Location = new System.Drawing.Point(135, 84);
            this.DatabaseName.Name = "DatabaseName";
            this.DatabaseName.Size = new System.Drawing.Size(320, 20);
            this.DatabaseName.TabIndex = 20;
            this.DatabaseName.Text = global::Configuration.Properties.Settings.Default.DatabaseName;
            // 
            // ServerName
            // 
            this.ServerName.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Configuration.Properties.Settings.Default, "ServerName", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.ServerName.Location = new System.Drawing.Point(135, 57);
            this.ServerName.Name = "ServerName";
            this.ServerName.Size = new System.Drawing.Size(320, 20);
            this.ServerName.TabIndex = 19;
            this.ServerName.Text = global::Configuration.Properties.Settings.Default.ServerName;
            // 
            // PortLabel
            // 
            this.PortLabel.AutoSize = true;
            this.PortLabel.Location = new System.Drawing.Point(475, 61);
            this.PortLabel.Name = "PortLabel";
            this.PortLabel.Size = new System.Drawing.Size(59, 13);
            this.PortLabel.TabIndex = 18;
            this.PortLabel.Text = "Server port";
            // 
            // DatabaseNameLabel
            // 
            this.DatabaseNameLabel.AutoSize = true;
            this.DatabaseNameLabel.Location = new System.Drawing.Point(40, 88);
            this.DatabaseNameLabel.Name = "DatabaseNameLabel";
            this.DatabaseNameLabel.Size = new System.Drawing.Size(85, 13);
            this.DatabaseNameLabel.TabIndex = 17;
            this.DatabaseNameLabel.Text = "Database name:";
            // 
            // ServerNameLabel
            // 
            this.ServerNameLabel.AutoSize = true;
            this.ServerNameLabel.Location = new System.Drawing.Point(55, 61);
            this.ServerNameLabel.Name = "ServerNameLabel";
            this.ServerNameLabel.Size = new System.Drawing.Size(70, 13);
            this.ServerNameLabel.TabIndex = 16;
            this.ServerNameLabel.Text = "Server name:";
            // 
            // DatabaseTypeLabel
            // 
            this.DatabaseTypeLabel.AutoSize = true;
            this.DatabaseTypeLabel.Location = new System.Drawing.Point(46, 34);
            this.DatabaseTypeLabel.Name = "DatabaseTypeLabel";
            this.DatabaseTypeLabel.Size = new System.Drawing.Size(79, 13);
            this.DatabaseTypeLabel.TabIndex = 15;
            this.DatabaseTypeLabel.Text = "Database type:";
            // 
            // DatabaseType
            // 
            this.DatabaseType.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Configuration.Properties.Settings.Default, "DatabaseType", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.DatabaseType.FormattingEnabled = true;
            this.DatabaseType.Location = new System.Drawing.Point(135, 30);
            this.DatabaseType.Name = "DatabaseType";
            this.DatabaseType.Size = new System.Drawing.Size(320, 21);
            this.DatabaseType.TabIndex = 14;
            this.DatabaseType.Text = global::Configuration.Properties.Settings.Default.DatabaseType;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel4, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 542);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(786, 44);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 4;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel4.Controls.Add(this.okBtn, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.cancelBtn, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.applyButton, 2, 0);
            this.tableLayoutPanel4.Controls.Add(this.helpBtn, 3, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(396, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(387, 38);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // okBtn
            // 
            this.okBtn.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.okBtn.Location = new System.Drawing.Point(3, 12);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(90, 23);
            this.okBtn.TabIndex = 0;
            this.okBtn.Text = "OK";
            this.okBtn.UseVisualStyleBackColor = true;
            // 
            // cancelBtn
            // 
            this.cancelBtn.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.cancelBtn.Location = new System.Drawing.Point(99, 12);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(90, 23);
            this.cancelBtn.TabIndex = 1;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            // 
            // applyButton
            // 
            this.applyButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.applyButton.Location = new System.Drawing.Point(195, 12);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(90, 23);
            this.applyButton.TabIndex = 2;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            // 
            // helpBtn
            // 
            this.helpBtn.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.helpBtn.Location = new System.Drawing.Point(291, 12);
            this.helpBtn.Name = "helpBtn";
            this.helpBtn.Size = new System.Drawing.Size(93, 23);
            this.helpBtn.TabIndex = 3;
            this.helpBtn.Text = "Help";
            this.helpBtn.UseVisualStyleBackColor = true;
            // 
            // Configuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 589);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Configuration";
            this.Text = "Infovision Configuration";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.Tab.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.DatabaseSettingGroup.ResumeLayout(false);
            this.DatabaseSettingGroup.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private void FormInit()
        {
            this.AuthenticationType.Items.AddRange(System.Enum.GetNames(typeof(AuthenticationType)));
            this.DatabaseType.Items.AddRange(System.Enum.GetNames(typeof(DatabaseType)));
        }

        private void FromClose()
        {
            
        }

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.ComboBox ConfigurationName;
        private System.Windows.Forms.Label ConfigurationNameLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Button helpBtn;
        private System.Windows.Forms.TabControl Tab;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.GroupBox GeneralSettingsGroup;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.GroupBox DatabaseSettingGroup;
        private System.Windows.Forms.Label PasswordLabel;
        private System.Windows.Forms.Label UserLabel;
        private System.Windows.Forms.TextBox Password;
        private System.Windows.Forms.TextBox UserId;
        private System.Windows.Forms.Label AuthenticationTypeLabel;
        private System.Windows.Forms.ComboBox AuthenticationType;
        private System.Windows.Forms.TextBox Port;
        private System.Windows.Forms.TextBox DatabaseName;
        private System.Windows.Forms.TextBox ServerName;
        private System.Windows.Forms.Label PortLabel;
        private System.Windows.Forms.Label DatabaseNameLabel;
        private System.Windows.Forms.Label ServerNameLabel;
        private System.Windows.Forms.Label DatabaseTypeLabel;
        private System.Windows.Forms.ComboBox DatabaseType;
    }
}


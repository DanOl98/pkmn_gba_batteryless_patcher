namespace BatterylessPatcher
{
    partial class MainWindow
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
            btnBrowse = new Button();
            repackCheck = new CheckBox();
            btnPatchGood2 = new Button();
            btnRepackOnly = new Button();
            ofd = new OpenFileDialog();
            txtRomPath = new TextBox();
            outputText = new RichTextBox();
            SuspendLayout();
            // 
            // btnBrowse
            // 
            btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowse.Location = new Point(796, 10);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(115, 37);
            btnBrowse.TabIndex = 0;
            btnBrowse.Text = "Choose ROM";
            btnBrowse.Click += BtnBrowse_Click;
            // 
            // repackCheck
            // 
            repackCheck.Checked = true;
            repackCheck.CheckState = CheckState.Checked;
            repackCheck.Location = new Point(12, 109);
            repackCheck.Name = "repackCheck";
            repackCheck.Size = new Size(160, 50);
            repackCheck.TabIndex = 2;
            repackCheck.Text = "Repack while applying patch";
            // 
            // btnPatchGood2
            // 
            btnPatchGood2.Location = new Point(12, 53);
            btnPatchGood2.Name = "btnPatchGood2";
            btnPatchGood2.Size = new Size(160, 50);
            btnPatchGood2.TabIndex = 5;
            btnPatchGood2.Text = "Apply Patch";
            btnPatchGood2.Click += BtnPatchGood2_Click;
            // 
            // btnRepackOnly
            // 
            btnRepackOnly.Location = new Point(12, 165);
            btnRepackOnly.Name = "btnRepackOnly";
            btnRepackOnly.Size = new Size(160, 50);
            btnRepackOnly.TabIndex = 6;
            btnRepackOnly.Text = "Repack only";
            btnRepackOnly.Click += BtnRepackOnly_Click;
            // 
            // txtRomPath
            // 
            txtRomPath.Location = new Point(12, 18);
            txtRomPath.Name = "txtRomPath";
            txtRomPath.ReadOnly = true;
            txtRomPath.Size = new Size(778, 23);
            txtRomPath.TabIndex = 1;
            txtRomPath.Text = "No ROM selected";
            // 
            // outputText
            // 
            outputText.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            outputText.Location = new Point(178, 53);
            outputText.Name = "outputText";
            outputText.Size = new Size(733, 336);
            outputText.TabIndex = 7;
            outputText.Text = "";
            outputText.TextChanged += outputText_TextChanged;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(923, 401);
            Controls.Add(outputText);
            Controls.Add(txtRomPath);
            Controls.Add(btnBrowse);
            Controls.Add(repackCheck);
            Controls.Add(btnPatchGood2);
            Controls.Add(btnRepackOnly);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainWindow";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "GBA Pokemon Batteryless Patcher";
            Load += MainWindow_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button btnBrowse;
        private Button btnPatchGood2;
        private Button btnRepackOnly;
        private CheckBox repackCheck;
        private OpenFileDialog ofd;
        private TextBox txtRomPath;
        private RichTextBox outputText;
    }
}
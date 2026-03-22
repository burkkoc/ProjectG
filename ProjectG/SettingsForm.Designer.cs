namespace ProjectG.PresentationLayer
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblNtfyNotifyUrl = new Label();
            txtNtfyNotifyUrl = new TextBox();
            btnSave = new Button();
            btnCancel = new Button();
            SuspendLayout();
            //
            // lblNtfyNotifyUrl
            //
            lblNtfyNotifyUrl.AutoSize = true;
            lblNtfyNotifyUrl.Location = new Point(12, 15);
            lblNtfyNotifyUrl.Name = "lblNtfyNotifyUrl";
            lblNtfyNotifyUrl.Size = new Size(120, 15);
            lblNtfyNotifyUrl.TabIndex = 0;
            lblNtfyNotifyUrl.Text = "NTFY URL";
            //
            // txtNtfyNotifyUrl
            //
            txtNtfyNotifyUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtNtfyNotifyUrl.Location = new Point(12, 33);
            txtNtfyNotifyUrl.Name = "txtNtfyNotifyUrl";
            txtNtfyNotifyUrl.Size = new Size(476, 23);
            txtNtfyNotifyUrl.TabIndex = 1;
            //
            // btnSave
            //
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.Location = new Point(332, 72);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 28);
            btnSave.TabIndex = 2;
            btnSave.Text = "Kaydet";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            //
            // btnCancel
            //
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Location = new Point(413, 72);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 28);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "İptal";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            //
            // SettingsForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(500, 112);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(txtNtfyNotifyUrl);
            Controls.Add(lblNtfyNotifyUrl);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            StartPosition = FormStartPosition.Manual;
            Text = "Ayarlar";
            Load += SettingsForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblNtfyNotifyUrl;
        private TextBox txtNtfyNotifyUrl;
        private Button btnSave;
        private Button btnCancel;
    }
}

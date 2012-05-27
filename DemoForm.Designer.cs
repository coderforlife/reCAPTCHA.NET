// Example of using the desktop .NET reCPATCHA control.
//
// Copyright (c) 2010 reCAPTCHA -- http://recaptcha.net
//
// AUTHOR: Jeffrey Bush
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace reCAPTCHADemo
{
    partial class DemoForm
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.PictureBox reload;
            System.Windows.Forms.PictureBox help;
            System.Windows.Forms.Button verify;
            this.response = new System.Windows.Forms.TextBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.changeType = new System.Windows.Forms.PictureBox();
            this.recaptcha = new Recaptcha.RecaptchaControl();
            reload = new System.Windows.Forms.PictureBox();
            help = new System.Windows.Forms.PictureBox();
            verify = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(reload)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(help)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.changeType)).BeginInit();
            this.SuspendLayout();
            // 
            // reload
            // 
            reload.Image = global::reCAPTCHA.Properties.Resources.reload;
            reload.Location = new System.Drawing.Point(318, 12);
            reload.Name = "reload";
            reload.Size = new System.Drawing.Size(16, 16);
            reload.TabIndex = 2;
            reload.TabStop = false;
            this.toolTip.SetToolTip(reload, "Get a new challenge");
            reload.Click += new System.EventHandler(this.reload_Click);
            // 
            // help
            // 
            help.Image = global::reCAPTCHA.Properties.Resources.help;
            help.Location = new System.Drawing.Point(318, 56);
            help.Name = "help";
            help.Size = new System.Drawing.Size(16, 16);
            help.TabIndex = 4;
            help.TabStop = false;
            help.Click += new System.EventHandler(this.help_Click);
            // 
            // response
            // 
            this.response.Location = new System.Drawing.Point(12, 75);
            this.response.Name = "response";
            this.response.Size = new System.Drawing.Size(300, 20);
            this.response.TabIndex = 1;
            // 
            // changeType
            // 
            this.changeType.Image = global::reCAPTCHA.Properties.Resources.audio;
            this.changeType.Location = new System.Drawing.Point(318, 34);
            this.changeType.Name = "changeType";
            this.changeType.Size = new System.Drawing.Size(16, 16);
            this.changeType.TabIndex = 3;
            this.changeType.TabStop = false;
            this.toolTip.SetToolTip(this.changeType, "Get an audio challenge");
            this.changeType.Click += new System.EventHandler(this.changeType_Click);
            // 
            // verify
            // 
            verify.Location = new System.Drawing.Point(259, 101);
            verify.Name = "verify";
            verify.Size = new System.Drawing.Size(75, 23);
            verify.TabIndex = 5;
            verify.Text = "Verify";
            verify.UseVisualStyleBackColor = true;
            verify.Click += new System.EventHandler(this.verify_Click);
            // 
            // recaptcha
            // 
            this.recaptcha.Location = new System.Drawing.Point(12, 12);
            this.recaptcha.Name = "recaptcha";
            this.recaptcha.PublicKey = "6Ld4iQsAAAAAAM3nfX_K0vXaUudl2Gk0lpTF3REf";
            this.recaptcha.Referer = "http://www.google.com/recaptcha/demo/";
            this.recaptcha.Size = new System.Drawing.Size(300, 57);
            this.recaptcha.TabIndex = 0;
            this.recaptcha.TabStop = false;
            // 
            // DemoForm
            // 
            this.AcceptButton = verify;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(346, 136);
            this.Controls.Add(verify);
            this.Controls.Add(help);
            this.Controls.Add(this.changeType);
            this.Controls.Add(reload);
            this.Controls.Add(this.response);
            this.Controls.Add(this.recaptcha);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "DemoForm";
            this.Text = "reCAPTCHA.NET Demo";
            ((System.ComponentModel.ISupportInitialize)(reload)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(help)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.changeType)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Recaptcha.RecaptchaControl recaptcha;
        private System.Windows.Forms.TextBox response;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.PictureBox changeType;
    }
}


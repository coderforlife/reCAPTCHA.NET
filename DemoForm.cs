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

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace reCAPTCHADemo
{
    public partial class DemoForm : Form
    {
        public DemoForm() { InitializeComponent(); }
        private void reload_Click(object sender, EventArgs e) { recaptcha.Reload(); }
        private void changeType_Click(object sender, EventArgs e)
        {
            if (recaptcha.IsVisualChallenge) {
                changeType.Image = global::reCAPTCHA.Properties.Resources.text;
                toolTip.SetToolTip(changeType, "Get a visual challenge");
            } else {
                changeType.Image = global::reCAPTCHA.Properties.Resources.audio;
                toolTip.SetToolTip(changeType, "Get an audio challenge");
            }
            recaptcha.SwitchChallengeType();
        }
        private void help_Click(object sender, EventArgs e) { recaptcha.ShowHelp(); }
        private void verify_Click(object sender, EventArgs e)
        {
            // Get the values to send
            string challenge = this.recaptcha.Challenge;
            string response = this.response.Text;

            // Create the post data
            string post;
            post  = "recaptcha_response_field="  + Uri.EscapeDataString(response) + "&";
            post += "recaptcha_challenge_field=" + Uri.EscapeDataString(challenge);
            byte[] data = Encoding.UTF8.GetBytes(post);

            // Make and prepare the request
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://www.google.com/recaptcha/demo/");
            if (this.recaptcha.Referer != null && this.recaptcha.Referer.Length > 0)
            req.Referer = this.recaptcha.Referer;
            req.Method = "POST";
            req.UserAgent = "reCAPTCHA/.NET";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;

            // Send the data
            Stream s = req.GetRequestStream();
            s.Write(data, 0, data.Length);
            s.Close();

            // Get the response
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            StreamReader r = new StreamReader(res.GetResponseStream());
            char[] buf = new char[1024];
            StringBuilder text = new StringBuilder(1024);
            int c;
            while ((c = r.Read(buf, 0, 1024)) > 0) {
                text.Append(buf, 0, c);
            }
            string results = text.ToString();
            res.Close();
            r.Close();

            // Process the response
            if (results.Contains("Correct!")) {
                MessageBox.Show(this, "The reCAPTCHA was entered correctly!", "reCAPTCHA Verification");
            } else if (results.Contains("Incorrect.")) {
                MessageBox.Show(this, "The reCAPTCHA was entered incorrectly.", "reCAPTCHA Verification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            } else {
                MessageBox.Show(this, "There was a problem verifying the reCAPTCHA.", "reCAPTCHA Verification", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Get a new reCAPTCHA
            this.recaptcha.Reload();
            this.response.Text = "";
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Recaptcha.RecaptchaControl.StopAudio();
            base.OnFormClosed(e);
        }
    }
}

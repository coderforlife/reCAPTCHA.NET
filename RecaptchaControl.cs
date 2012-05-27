// This is a .NET desktop user control for displaying / playing reCAPTCHA information.
//    - Documentation and latest version
//          http://recaptcha.net/plugins/?????/
//    - Get a reCAPTCHA API Key
//          https://www.google.com/recaptcha/admin/create
//    - Discussion group
//          http://groups.google.com/group/recaptcha
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


// Make sure to add references to the assemblies:
//    System.Design


// Version 1.0:
//    Initial release


// Todo:
//    Does not support: languages/translations, includeContext (what does that do?)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Recaptcha
{
    /// <summary>
    /// This class encapsulates reCAPTCHA UI into a .NET user control.
    /// </summary>
    [Designer(typeof(RecaptchaControl.RecaptchaDesigner))]
    public class RecaptchaControl : UserControl
    {
        #region Private Constants
        // You probably don't need to change this
        private const string defaultServer = "http://www.google.com/recaptcha/api/";
        #endregion

        #region Error Messages
        // Possible error messages (while doing a verify)
        public const string unknown_error = "unknown";
        public const string invalid_public_key = "invalid-site-public-key";
        public const string invalid_private_key = "invalid-site-private-key";
        public const string invalid_request_cookie = "invalid-request-cookie";
        public const string incorrect_captcha_sol = "incorrect-captcha-sol";
        public const string verify_params_incorrect = "verify-params-incorrect";
        public const string invalid_referrer = "invalid-referrer";
        public const string recaptcha_not_reachable = "recaptcha-not-reachable";
        #endregion
        
        #region Public Properties
        [Category("Settings")]
        [Description("The public key from https://www.google.com/recaptcha/admin/create")]
        public string PublicKey
        {
            get { return publicKey; }
            set { publicKey = value; }
        }

        [Category("Settings")]
        [Description("The desired referer for the request, must be within the domain for your private key")]
        public string Referer
        {
            get { return referer; }
            set { referer = value; }
        }

        [Category("Settings")]
        [DefaultValue(null)] // (string)
        public string ExtraChallengeParams
        {
            get { return extraChallengeParams; }
            set { extraChallengeParams = value; }
        }

        [Category("Settings")]
        [DefaultValue(false)]
        public bool AudioBeta1208
        {
            get { return audio_beta_12_08; }
            set { audio_beta_12_08 = value; }
        }

        [Browsable(false)]
        public string Challenge { get { return this.data.ContainsKey("challenge") ? this.data["challenge"] : null; } }

        [Browsable(false)]
        public bool IsVisualChallenge { get { return this.isImage; } }

        [Browsable(false)]
        public bool IsAudioChallenge { get { return !this.isImage; } }
        #endregion

        public RecaptchaControl()
        {
            this.data = new Dictionary<string,string>();
            this.isImage = true;

            this.SuspendLayout();

            // Create the image
            this.image = new PictureBox();
            this.image.Location = new Point(0, 0);
            this.image.Size = new Size(300, 57);
            this.image.TabStop = false;
            this.image.Visible = true;
            this.Controls.Add(this.image);

            // Create the play again button
            this.play_again = new Button();
            this.play_again.Location = new Point(112, 17);
            this.play_again.Size = new Size(75, 23);
            this.play_again.Text = "Play Again";
            this.play_again.Visible = false;
            this.play_again.Click += new EventHandler(PlayAgainEvent);
            this.Controls.Add(this.play_again);

            this.Size = this.image.Size;
            this.ResumeLayout();
        }

        #region Public Methods
        /// <summary>
        /// Causes the audio to replay from the beginning if an audio challenge is currently active
        /// </summary>
        public void PlayAgain() { Play(this.GetPath()); }
        
        /// <summary>
        /// Delivers a new challenge of the current type (either audio of visual)
        /// </summary>
        public void Reload() { this.SetChallenge('r'); }

        /// <summary>
        /// Changes the challenge to an audio challenge, does nothing if it is already an audio challenge
        /// </summary>
        public void SwitchToAudioChallenge() { if (this.isImage) { isImage = false; this.SetChallenge('a'); } }

        /// <summary>
        /// Changes the challenge to a visual challenge, does nothing if it is already a visual challenge
        /// </summary>
        public void SwitchToVisualChallenge() { if (!this.isImage) { isImage = true; this.SetChallenge('v'); } }

        /// <summary>
        /// Switches the challenge type, to a visual challenge if the current is an audio challenge, and vice versa
        /// </summary>
        public void SwitchChallengeType() { this.isImage = !this.isImage; this.SetChallenge(this.isImage ? 'v' : 'a'); }
        
        /// <summary>
        /// Opens a browser window with the help page
        /// </summary>
        public void ShowHelp() { System.Diagnostics.Process.Start(this.GetHelpURL()); }

        /// <summary>
        /// Gets a the URL of the help page
        /// </summary>
        public string GetHelpURL() { return "http://www.google.com/recaptcha/help" + (this.data.ContainsKey("challenge") ? "?c=" + this.data["challenge"] : ""); }

        /// <summary>
        /// Stops all audio playback
        /// </summary>
        public static void StopAudio() { playing_state = PLAYING_NOTHING; if (player != null) { player.Stop(); } }
        
        #endregion

        #region Private Fields
        // The data that backs this reCAPTCHA, this is essentially RecaptchaState from the web-based version
        private Dictionary<string,string> data;

        // If we are currently showing a visual or audio challenge
        private bool isImage;

        // The public key and referer for the reCAPTCHA
        private string publicKey, referer;

        // Extra parameters to send to the server
        private string extraChallengeParams;
        private bool audio_beta_12_08;

        // The controls for the image and the audio
        private Button play_again;
        private PictureBox image;
        #endregion

        #region Private Methods
        // Gets the image / audio path
        private string GetPath() { return this.data["server"]+"image?c="+this.data["challenge"]; }

        // The event when the 'Play Again' button is clicked, simply calls PlayAgain()
        private void PlayAgainEvent(object sender, EventArgs e) { this.PlayAgain(); }
        #endregion

        // Sets the challenge, where type is one of '\0' (completely new), 'r' (reload), 'a' (change to audio), 'v' (change to video)
        private void SetChallenge(Char type)
        {
            if (!this.Visible || this.DesignMode) return;

            // Request the data
            string addr;
            if (type == 0) {
                addr = defaultServer+"challenge?k="+publicKey;
            } else {
                addr = data["server"]+"reload?c="+data["challenge"]+"&k="+data["site"]+"&reason="+type+"&type="+(isImage?"image":"audio")+"&lang=en";
                if (!this.isImage)
                    addr += audio_beta_12_08 ? "&audio_beta_12_08=1" : "&new_audio_default=1";
            }
            if (extraChallengeParams != null && extraChallengeParams.Length > 0) {
                addr += "&"+extraChallengeParams;
            }
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(addr);
            req.UserAgent = "reCAPTCHA/.NET";
            if (referer != null && referer.Length > 0)
                req.Referer = referer;

            // Get the response
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            StreamReader s = new StreamReader(res.GetResponseStream()); // res.ContentEncoding?
            Char[] buf = new Char[1024];
            StringBuilder text = new StringBuilder(1024);
            int c;
            while ((c = s.Read(buf, 0, 1024)) > 0) {
                text.Append(buf, 0, c);
            }
            string results = text.ToString();
            res.Close();
            s.Close();

            // Interpret the response
            if (type == 0) {
                int start = results.IndexOf('{'), end = results.IndexOf('}', start+1);
                string[] vars = results.Substring(start+1, end-start-1).Trim().Replace(" ", "").Replace("\n", "").Split(',');
                foreach (string varx in vars) {
                    string var = varx.Trim();
                    int colon = var.IndexOf(':');
                    string name = var.Substring(0, colon).ToLower().Trim(), val = var.Substring(colon+1).Trim();
                    if ((val[0] == '\'' && val[val.Length-1] == '\'') || (val[0] == '"' && val[val.Length-1] == '"')) {
                        val = val.Substring(1, val.Length-2);
                    }
                    this.data[name] = val;
                }
            } else {
                int start = results.IndexOf('\''), end = results.IndexOf('\'', start+1);
                this.data["challenge"] = results.Substring(start+1, end-start-1).Trim();
            }

            // Check the data
            if (!this.data.ContainsKey("challenge") || !this.data.ContainsKey("site")) { throw new Exception(recaptcha_not_reachable); }
            if (!this.data.ContainsKey("server")) { this.data["server"] = defaultServer; }

            // Load the display
            if (this.isImage) {
                StopAudio();
                this.image.ImageLocation = this.GetPath();
            } else {
                this.image.Image = null;
                Play(this.GetPath());
            }
            this.image.Visible = this.isImage;
            this.play_again.Visible = !this.isImage;
        }

        #region Audio Playback Methods
        // The states that the audio playback can be in
        private const int PLAYING_NOTHING = 0, PLAYING_INSTRUCTIONS = 1, PLAYING_LOOP = 2, PLAYING_FILE = 3;

        // The object for playing back audio. It is shared by all reCAPTCHAs so that two files never play at the same time
        private static MP3Player player;

        // The current state and file
        private static int playing_state = PLAYING_NOTHING;
        private static string playing_file = null;

        // A thread that downloads the audio file
        static Thread dlThread = null;

        private static void Play(string mp3)
        {
            playing_file = mp3;
            Play(global::reCAPTCHA.RecaptchaControl.instructions, "instructions", PLAYING_INSTRUCTIONS);
            if (dlThread != null && dlThread.ThreadState != ThreadState.Stopped)
                dlThread.Abort();
            dlThread = new Thread(new ThreadStart(DownloadMP3));
            dlThread.Name = "Download MP3";
            dlThread.Start();
        }
        private static void DownloadMP3()
        {
            string path = Path.Combine(Path.GetTempPath(), "recaptcha.mp3");
            FileStream f = null;
            Stream s = null;
            Byte[] b = null;
            try {
                f = new FileStream(path, FileMode.Create);
                s = WebRequest.Create(new Uri(playing_file)).GetResponse().GetResponseStream();
                b = new Byte[8192];
                int r;
                while ((r = s.Read(b, 0, b.Length)) > 0)
                    f.Write(b, 0, r);
                f.Close();
            } finally {
                if (f != null) f.Close();
                if (s != null) s.Close();
                //if (b != null) b.Dispose();
            }
        }
        // Switch to the next audio file when the current has ended
        private static void PlaybackEnded()
        {
            if (playing_state == PLAYING_FILE)
            {
                Play(global::reCAPTCHA.RecaptchaControl.play_again, "loop", PLAYING_LOOP);
            }
            else if (playing_state == PLAYING_INSTRUCTIONS || playing_state == PLAYING_LOOP)
            {
                Play(null, "recaptcha", PLAYING_FILE);
            }
        }
        private static void Play(Byte[] data, string name, int state)
        {
            playing_state = state;
            string path = Path.Combine(Path.GetTempPath(), name+".mp3");
            if (!File.Exists(path))
            {
                FileStream f = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
                f.Write(data, 0, data.Length);
                f.Close();
            }
            if (player == null)
            {
                player = new MP3Player();
                player.Ended += new MP3Player.Event(PlaybackEnded);
                player.Destroyed += new MP3Player.Event(CleanupAudio);
            }
            player.Play(path);
        }
        private static void CleanupAudio()
        {
            foreach (string x in new string[]{"instructions", "recaptcha", "loop"})
            {
                string path = Path.Combine(Path.GetTempPath(), x + ".mp3");
                if (File.Exists(path)) File.Delete(path);
            }
        }
        #endregion

        // Resets the data and gets a new challenge when it becomes visible
        protected override void OnVisibleChanged(EventArgs e)
        {
            if (this.Visible)
            {
                // Reset
                this.isImage = true;
                this.data.Clear();
                this.SetChallenge((Char)0);
            }
            else
            {
                // Stop the audio
                StopAudio();
            }
            base.OnVisibleChanged(e);
        }

        // Prevent resizing
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, this.image.Width, this.image.Height, specified);
        }

        // Create a custom designer so we can tell the designer that it can be moved but not resized and a custom picture
        public class RecaptchaDesigner : System.Windows.Forms.Design.ControlDesigner
        {
            public override System.Windows.Forms.Design.SelectionRules SelectionRules
            {
                get { return System.Windows.Forms.Design.SelectionRules.Visible | System.Windows.Forms.Design.SelectionRules.Moveable; }
            }
            protected override void OnPaintAdornments(PaintEventArgs pe)
            {
                Font f = Control.DefaultFont;
                Graphics g = pe.Graphics;
                g.FillRectangle(Brushes.White, g.ClipBounds);
                g.DrawString("reCAPTCHA.NET Control", new Font(f, FontStyle.Bold), Brushes.Black, 5, 8);
                g.DrawString("This area will display the challenge image or 'Play Again'\naudio button when run.", f, Brushes.Black, 5.0f, f.Height+9.0f);
                base.OnPaintAdornments(pe);
            }
        }
    }
}

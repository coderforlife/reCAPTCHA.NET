// This is a simple MP3 Player for the reCAPTCHA control.
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

// Version 1.0:
//    Initial release

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Recaptcha
{
    class MCIException : Exception {
        [DllImport("winmm.dll")]
        static extern int mciGetErrorString(int fdwError, StringBuilder lpszErrorText, uint cchErrorText);

        private string text;
        public int ErrorCode;
        public MCIException(int err) { ErrorCode = err; text = null; }
        public override string Message { get {
            if (text == null)
            {
                StringBuilder s = new StringBuilder(512);
                text = (mciGetErrorString(ErrorCode, s, 512)!=0) ? s.ToString() : "Unknown MCI error.";
                text += " ("+ErrorCode+")";
            }
            return text;
        } }
        public override string ToString() { return this.Message; }
    }

    class MP3Player : Form
    {
        // We are extending Form so that we can have a message loop
        // We cannot use a NativeWindow since that doesn't have 'Invoke' and 'InvokeRequired'
        private delegate void dSend(string cmd);

        public const int MM_MCINOTIFY           = 0x03B9;
        public const int MCI_NOTIFY_SUCCESSFUL  = 0x0001;
        public const int MCI_NOTIFY_SUPERSEDED  = 0x0002;
        public const int MCI_NOTIFY_ABORTED     = 0x0004;
        public const int MCI_NOTIFY_FAILURE     = 0x0008;
        public readonly IntPtr HWND_MESSAGE = new IntPtr(-3);

        [DllImport("winmm.dll")]
        static extern int mciSendString(string lpszCommand, StringBuilder lpszReturnString, int cchReturn, IntPtr hwndCallback);

        private bool playing;

        private void Send(string cmd)
        {
            if (this.InvokeRequired) { this.Invoke(new dSend(Send), cmd); } 
            else
            {
                int err = mciSendString(cmd, null, 0, cmd.EndsWith(" NOTIFY") ? this.Handle : IntPtr.Zero);
                if (err != 0)
                    throw new MCIException(err);
            }
        }

        protected override CreateParams CreateParams { get
        {
            CreateParams cp = new CreateParams();
            cp.ClassName = "STATIC";
            cp.Parent = HWND_MESSAGE;
            return cp;
        } }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == MM_MCINOTIFY)
            {
                //m.WParam is one of MCI_NOTIFY_ABORTED, MCI_NOTIFY_FAILURE, MCI_NOTIFY_SUCCESSFUL, MCI_NOTIFY_SUPERSEDED
                //m.LParam is the device id
                if (m.WParam.ToInt32() == MCI_NOTIFY_SUCCESSFUL && playing) // played till the end
                {
                    playing = false;
                    Send("CLOSE mp3media");
                    Ended();
                }
            } else { base.WndProc(ref m); }
        }
        protected override void Dispose(bool disposing)
        {
            Stop();
            Send("CLOSE ALL");
            base.Dispose(disposing);
            Destroyed();
        }

        public delegate void Event();
        public event Event Ended;
        public event Event Destroyed;

        public MP3Player() { this.CreateHandle(); }
        public bool Playing { get { return playing; } }
        public void Play(string file)
        {
            if (playing) Stop();
            Send("OPEN \""+file+"\" TYPE mpegvideo ALIAS mp3media");
            Send("PLAY mp3media NOTIFY");
            playing = true;
        }
        public void Stop()
        {
            if (playing)
            {
                Send("STOP mp3media");
                Send("CLOSE mp3media");
                playing = false;
            }
        }
    }
}

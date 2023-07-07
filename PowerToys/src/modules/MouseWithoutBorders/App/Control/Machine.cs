// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Windows.Forms;

// <summary>
//     User control, used in the Matrix form.
// </summary>
// <history>
//     2008 created by Truong Do (ductdo).
//     2009-... modified by Truong Do (TruongDo).
//     2023- Included in PowerToys.
// </history>
using MouseWithoutBorders.Class;
using MouseWithoutBorders.Properties;

namespace MouseWithoutBorders
{
    internal partial class Machine : UserControl
    {
        // private int ip;
        // private Point mouseDownPos;
        private SocketStatus statusClient;

        private SocketStatus statusServer;
        private bool localhost;

        internal Machine()
        {
            InitializeComponent();
            Visible = false;
            MachineEnabled = false;
        }

        internal string MachineName
        {
            get => textBoxName.Text;
            set => textBoxName.Text = value;
        }

        internal bool MachineEnabled
        {
            get => checkBoxEnabled.Checked;
            set
            {
                checkBoxEnabled.Checked = value;
                Editable = value;
                pictureBoxLogo.Image = value ? Resources.MachineEnabled : (System.Drawing.Image)Resources.MachineDisabled;
                OnEnabledChanged(EventArgs.Empty); // Borrow this event since we do not use it for any other purpose:) (we can create one but l...:))
            }
        }

        internal bool Editable
        {
            set => textBoxName.Enabled = value;

            // get { return textBoxName.Enabled;  }
        }

        internal bool CheckAble
        {
            set
            {
                if (!value)
                {
                    checkBoxEnabled.Checked = true;
                    Editable = false;
                }

                checkBoxEnabled.Enabled = value;
            }
        }

        internal bool LocalHost
        {
            // get { return localhost; }
            set
            {
                localhost = value;
                if (localhost)
                {
                    labelStatusClient.Text = "此电脑";
                    labelStatusServer.Text = "...";
                    CheckAble = false;
                }
                else
                {
                    labelStatusClient.Text = "...";
                    labelStatusServer.Text = "...";
                    CheckAble = true;
                }
            }
        }

        private void PictureBoxLogo_MouseDown(object sender, MouseEventArgs e)
        {
            // mouseDownPos = e.Location;
            OnMouseDown(e);
        }

        /*
        internal Point MouseDownPos
        {
            get { return mouseDownPos; }
        }
        */

        private void CheckBoxEnabled_CheckedChanged(object sender, EventArgs e)
        {
            MachineEnabled = checkBoxEnabled.Checked;
        }

        private static string StatusString(SocketStatus status)
        {
            string rv = string.Empty;

            switch (status)
            {
                case SocketStatus.Resolving:
                    rv = "正在查找地址";
                    break;

                case SocketStatus.Connected:
                    rv = "已连接";
                    break;

                case SocketStatus.Connecting:
                    rv = "正在连接";
                    break;

                case SocketStatus.Error:
                    rv = "出错";
                    break;

                case SocketStatus.ForceClosed:
                    rv = "已关闭";
                    break;

                case SocketStatus.Handshaking:
                    rv = "正在握手";
                    break;

                case SocketStatus.SendError:
                    rv = "发送错误";
                    break;

                case SocketStatus.InvalidKey:
                    rv = "密码错误";
                    break;

                case SocketStatus.Timeout:
                    rv = "连接超时";
                    break;

                case SocketStatus.NA:
                    rv = "...";
                    break;

                default:
                    break;
            }

            return rv;
        }

        internal SocketStatus StatusClient
        {
            get => statusClient;

            set
            {
                statusClient = value;
                if (statusClient is SocketStatus.Connected or
                    SocketStatus.Handshaking)
                {
                    Editable = false;
                }

                labelStatusClient.Text = StatusString(statusClient) + " -->";
            }
        }

        internal SocketStatus StatusServer
        {
            get => statusServer;

            set
            {
                statusServer = value;
                if (statusServer is SocketStatus.Connected or
                    SocketStatus.Handshaking)
                {
                    Editable = false;
                }

                labelStatusServer.Text = StatusString(statusServer) + " <--";
            }
        }

        private void PictureBoxLogo_Paint(object sender, PaintEventArgs e)
        {
            // e.Graphics.DrawString("(Draggable)", this.Font, Brushes.WhiteSmoke, 20, 15);
        }
    }
}

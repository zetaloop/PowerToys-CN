// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Windows.Forms;

using MouseWithoutBorders.Class;
using MouseWithoutBorders.Form.Settings;

namespace MouseWithoutBorders
{
    public partial class SettingsFormPage : UserControl
    {
        public event EventHandler<PageEventArgs> NextPage;

        protected bool BackButtonVisible
        {
            get => BackButton.Visible;
            set => BackButton.Visible = value;
        }

        public SettingsFormPage()
        {
            InitializeComponent();
        }

        public virtual void OnPageClosing()
        {
        }

        protected void SendNextPage(SettingsFormPage page)
        {
            NextPage?.Invoke(this, new PageEventArgs(page));
        }

        protected virtual SettingsFormPage CreateBackPage()
        {
            return null;
        }

        protected string GetSecureKey()
        {
            return Common.MyKey;
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            SendNextPage(CreateBackPage());
        }

        private void ButtonSkip_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                "强烈推荐您先完成初始配置！确定需要跳过这些步骤吗？",
                Application.ProductName,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                Setting.Values.FirstRun = false;
                Common.CloseSetupForm();
                Common.ShowMachineMatrix();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Placeholder : Form
    {
        private Activation act;
        private LicenceManagement lic;

        public Placeholder()
        {
            InitializeComponent();
        }

        private void Placeholder_Load(object sender, EventArgs e)
        {
            act = new Activation();
            lic = new LicenceManagement();

            var hasLicence = lic.HasLicence();

            if (hasLicence)
                Unlock();
            else
                Lock();
        }

        private void btnActivate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtAuthentication.Text)) //generate new authentication code
            {
                txtAuthentication.Text = act.GetAuthenticationCode();
            }
            else //authentication code is generated => validate activation code
            {
                if (string.IsNullOrEmpty(txtActivationCode.Text))
                {
                    MessageBox.Show("Activation code is needed. Please contact Rosslyn Analytics with your authentication code to get a valid activation code.", "Activation Code Needed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtActivationCode.Focus();
                }
                else
                {
                    var valid = act.ValidateActivationCode(txtActivationCode.Text);

                    if (valid)
                    {
                        //Successfully activate the software - everything is good now
                        lic.Activate();
                        Unlock();
                    }
                    else
                    {
                        MessageBox.Show("Invalid activation code.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Extraction process is starting now.", "Extraction succeeds");
        }

        private void Unlock()
        {
            btnExtract.Enabled = true;
            btnExtract.BackColor = Color.AliceBlue;
            txtActivationCode.Enabled = false;
            btnActivate.Enabled = false;
            btnDeactivate.Enabled = true;
        }

        private void Lock()
        {
            btnExtract.Enabled = false;
            btnExtract.BackColor = Color.LightCoral;
            txtActivationCode.Enabled = true;
            btnActivate.Enabled = true;
            btnDeactivate.Enabled = false;
        }

        private void btnDeactivate_Click(object sender, EventArgs e)
        {
            lic.Deactivate();
            Lock();
        }
    }

    public class Encryption
    {
        private const string publicKey = @"<RSAKeyValue>
<Modulus>oO5PiP+fWyfWUDwQv+RvQzl4iZeyhw7N6SDErav2MBepJMf5uHf5z9FuEG0GcrPF3L2whuCL/f5nB50dxQZET7LYFcuhKzTm9Dade9H7Qn42+6aX1kZ31TkkRx5+3Eu6QRxCM8drD3ow92xonmE6O4omUrLFEF4DCxyqtM3H/0U=</Modulus>
<Exponent>AQAB</Exponent>
</RSAKeyValue>";

        public string Encrypt(string plainText)
        {
            using (var cipher = new RSACryptoServiceProvider())
            {
                cipher.FromXmlString(publicKey);
                byte[] data = Encoding.UTF8.GetBytes(plainText);
                byte[] cipherText = cipher.Encrypt(data, true);
                return Convert.ToBase64String(cipherText);
            }
        }
    }

    public class Activation
    {
        private readonly string activationCode;

        public Activation()
        {
            activationCode = Guid.NewGuid().ToString();
        }

        public string GetAuthenticationCode()
        {
            string mac = GetMacAddress();
            var raw = string.Format($"{activationCode}:{mac}");
            var publicAuthenticationCode = new Encryption().Encrypt(raw);
            return publicAuthenticationCode;
        }

        public bool ValidateActivationCode(string activationCode)
        {
            return this.activationCode == activationCode;
        }

        private string GetMacAddress()
        {
            //MAC address of the first NIC card - whatever it is and whether it is up
            return NetworkInterface.GetAllNetworkInterfaces().First().GetPhysicalAddress().ToString();
        }
    }

    public class LicenceManagement
    {
        private const string parentFolder = "Software";
        private const string regFolder = "RosslynAnalytics";
        private const string regKey = "OfflineExtract";

        public bool HasLicence()
        {
            //make sure the hash of the mac address is in the registry
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(parentFolder, false);
            if (key.GetSubKeyNames().Contains(regFolder))
            {
                key = key.OpenSubKey(regFolder);
                var regVal = (string)key.GetValue(regKey);
                var mac = GetMacAddress();
                var secret = new Encryption().Encrypt(mac);

                return regVal == secret;
            }
            return false;
        }

        public void Activate()
        {
            Deactivate(); //delete the existing (if any) registry values

            //create the hash
            var mac = GetMacAddress();
            var secret = new Encryption().Encrypt(mac);

            //save the hash into the registry
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(parentFolder, true);
            key.CreateSubKey(regFolder);
            key = key.OpenSubKey(regFolder, true);
            key.SetValue(regKey, secret, Microsoft.Win32.RegistryValueKind.String);

            key.Dispose();
        }

        public void Deactivate()
        {
            //remove the entry in the registry
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(parentFolder, true);
            key.DeleteSubKeyTree(regFolder, false);
        }

        private string GetMacAddress()
        {
            //MAC address of the first NIC card - whatever it is and whether it is up
            return NetworkInterface.GetAllNetworkInterfaces().First().GetPhysicalAddress().ToString();
        }
    }
}

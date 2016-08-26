using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello world");

            AsymmetricAlgoService asymmService = new AsymmetricAlgoService();

            for (int i = 0; i < 10; i++)
            {
                string cipherText = asymmService.GetCipherText("Hello");
                Console.WriteLine("Encrypted: " + cipherText);
                string original = asymmService.DecryptCipherText(cipherText);
                Console.WriteLine("Original: " + original);
            }

            Console.ReadLine();
        }
    }

    public class AsymmetricAlgoService
    {
        private const string publicKey = @"<RSAKeyValue>
<Modulus>oO5PiP+fWyfWUDwQv+RvQzl4iZeyhw7N6SDErav2MBepJMf5uHf5z9FuEG0GcrPF3L2whuCL/f5nB50dxQZET7LYFcuhKzTm9Dade9H7Qn42+6aX1kZ31TkkRx5+3Eu6QRxCM8drD3ow92xonmE6O4omUrLFEF4DCxyqtM3H/0U=</Modulus>
<Exponent>AQAB</Exponent>
</RSAKeyValue>";

        private const string privateKey = @"<RSAKeyValue>
	<Modulus>oO5PiP+fWyfWUDwQv+RvQzl4iZeyhw7N6SDErav2MBepJMf5uHf5z9FuEG0GcrPF3L2whuCL/f5nB50dxQZET7LYFcuhKzTm9Dade9H7Qn42+6aX1kZ31TkkRx5+3Eu6QRxCM8drD3ow92xonmE6O4omUrLFEF4DCxyqtM3H/0U=</Modulus>
	<Exponent>AQAB</Exponent>
	<P>vXj322EAm0xcebmZf/7wgvmtsMwXWRpXuff5QFNx1vG1XUIvFiHhI8t83usvZKdDGc+YzIrUVy/Gc46y6mXs6w==</P>
	<Q>2W/VdXEtXg2p9hQMLWeyIaMqLws1dXvu3RN0/Dz5Ey3CVZmQCHMWTD7cmpJL/wRYNrdIkoAHvcN7MQVgmr/4jw==</Q>
	<DP>LzPe08mTxBy/ARhK9IdH1elr6xq9SlZ0uoDbmLnxJ3JqE4S0hFgGZcuBHWwMD5BX+Csuzu5bPilJ0GohiqG/5w==</DP>
	<DQ>ewyu4CeMZQ2WgYwW2Bs205Ji7PyK5FGee73nFlfrHM9oisi8mguHMt7gORlRqJ/szAotJ7sMpndZ4AQLB4hcQQ==</DQ>
	<InverseQ>JHiBBWpx3kQI0mV7+st0Kqg4ZsvRuRifsYn5+wLVZKQnDPSNj6HyuU61IwAZkuEpkClLw+OEnv0CH8bVQ/8ZPA==</InverseQ>
	<D>QBmsJG1tE+nyO1MxeL2Mc8JYeLWrro//BHTA4kw0a0OBY90jYo6nOle492H1x4pDrYEA5zhZinnin+29BdVRL6uAy+XevI8Tu0INVsaLTIBoHQf1eMv/U8jNDNYxG4g9FyycWqtFW53vc8pmTjGgtcE9+O3YpSrU1vK2UNeVeaE=</D>
</RSAKeyValue>";

        private RSACryptoServiceProvider CreateCipherForEncryption()
        {
            RSACryptoServiceProvider cipher = new RSACryptoServiceProvider();
            cipher.FromXmlString(publicKey);
            return cipher;
        }

        public void ProgrammaticRsaKeys()
        {
            RSACryptoServiceProvider myRSA = new RSACryptoServiceProvider();
            RSAParameters publicKey = myRSA.ExportParameters(false);
            string xml = myRSA.ToXmlString(true);
        }

        public string GetCipherText(string plainText)
        {
            RSACryptoServiceProvider cipher = CreateCipherForEncryption();
            byte[] data = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherText = cipher.Encrypt(data, false);
            return Convert.ToBase64String(cipherText);
        }

        private RSACryptoServiceProvider CreateCipherForDecryption()
        {
            RSACryptoServiceProvider cipher = new RSACryptoServiceProvider();
            cipher.FromXmlString(privateKey);
            return cipher;
        }

        public string DecryptCipherText(string cipherText)
        {
            RSACryptoServiceProvider cipher = CreateCipherForDecryption();
            byte[] original = cipher.Decrypt(Convert.FromBase64String(cipherText), false);
            return Encoding.UTF8.GetString(original);
        }
    }
}

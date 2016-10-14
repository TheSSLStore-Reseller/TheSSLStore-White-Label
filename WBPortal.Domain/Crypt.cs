using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore
{
    [NotMapped]
    public class CryptorEngine
    {

        /// <summary>
        /// Return Key block from key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static byte[] GetKey()
        {
            return new byte[] { 125, 180, 94, 41, 193, 38, 216, 251, 0, 31, 166, 182, 71, 95, 179, 160, 10, 29, 43, 6, 17, 202, 20, 223, 28, 53, 65, 212, 150, 100, 70, 152 };
        }

        private static byte[] GetIV()
        {
            return new byte[] { 166, 133, 210, 29, 40, 117, 221, 245, 75, 160, 132, 244, 142, 35, 33, 21 };
        }


        /// <summary>
        /// Uses Rijndael Managed Class to Encrypt, Resulting data is transportable over HTTP
        /// </summary>
        /// <param name="key"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static string Encrypt(string Data, bool useKey)
        {
            if (Data == null)
                return null;
            
            using (RijndaelManaged myRijndael = new RijndaelManaged())
            {
                UnicodeEncoding textConverter = new UnicodeEncoding();
                byte[] encrypted;
                byte[] toEncrypt;
                byte[] key = GetKey();
                byte[] IV = GetIV();


                //  myRijndael.Padding = PaddingMode.None;

                //Get an encryptor.
                ICryptoTransform encryptor = myRijndael.CreateEncryptor(key, IV);

                //Encrypt the data.
                MemoryStream msEncrypt = new MemoryStream();
                CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);

                //Convert the data to a byte array.
                toEncrypt = textConverter.GetBytes(Data);

                //Write all data to the crypto stream and flush it.
                csEncrypt.Write(toEncrypt, 0, toEncrypt.Length);
                csEncrypt.FlushFinalBlock();

                //Get encrypted array of bytes.
                encrypted = msEncrypt.ToArray();
                msEncrypt.Dispose();

                return Convert.ToBase64String(encrypted);
            }
        }



        public static string Decrypt(string Data, bool usekey)
        {
            if (Data == null)
                return null;

            using (RijndaelManaged myRijndael = new RijndaelManaged())
            {
                UnicodeEncoding textConverter = new UnicodeEncoding();
                byte[] key = GetKey();
                byte[] IV = GetIV();
                byte[] fromEncrypt;
                byte[] encrypted = Convert.FromBase64String(Data.Replace(" ", "+"));

                myRijndael.Padding = PaddingMode.PKCS7;
                //Get a decryptor that uses the same key and IV as the encryptor.
                ICryptoTransform decryptor = myRijndael.CreateDecryptor(key, IV);

                //Now decrypt the previously encrypted message using the decryptor
                // obtained in the above step.
                MemoryStream msDecrypt = new MemoryStream(encrypted);
                CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);

                fromEncrypt = new byte[encrypted.Length];

                //Read the data out of the crypto stream.
                csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);

                //Convert the byte array back into a string.
                return textConverter.GetString(fromEncrypt);
            }
        }
    }

    [NotMapped]
    public class CryptorEngine_1
    {
        public static readonly string SaltKey = "1$5^4$8$";
        /// <summary>
        /// Encrypt a string using dual encryption method. Return a encrypted cipher Text
        /// </summary>
        /// <param name="toEncrypt">string to be encrypted</param>
        /// <param name="useHashing">use hashing? send to for extra secirity</param>
        /// <returns></returns>
        public static string Encrypt(string toEncrypt, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);


            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(SaltKey));
                hashmd5.Clear();
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(SaltKey);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        /// <summary>
        /// DeCrypt a string using dual encryption method. Return a DeCrypted clear string
        /// </summary>
        /// <param name="cipherString">encrypted string</param>
        /// <param name="useHashing">Did you use hashing to encrypt this data? pass true is yes</param>
        /// <returns></returns>
        public static string Decrypt(string cipherString, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = Convert.FromBase64String(cipherString.Replace(" ", "+"));



            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(SaltKey));
                hashmd5.Clear();
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(SaltKey);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            tdes.Clear();
            return UTF8Encoding.UTF8.GetString(resultArray);
        }
    }
}


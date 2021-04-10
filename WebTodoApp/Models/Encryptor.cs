﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WebTodoApp.Models {
    public class Encryptor {

        public string encrypt(string plainText) {
            byte[] src = Encoding.Unicode.GetBytes(plainText);

            byte[] key = getKey();
            byte[] iv = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

            using (var aesManaged = new AesManaged())
            using (var encryptor = aesManaged.CreateEncryptor(key, iv))
            using (var outStream = new MemoryStream()) {
                using (var cryptoStream = new CryptoStream(outStream, encryptor, CryptoStreamMode.Write)) {
                    cryptoStream.Write(src, 0, src.Length);
                }

                byte[] result = outStream.ToArray();
                return Convert.ToBase64String(result);
            }
        }

        public string decrypt(string encryptText) {
            byte[] key = getKey();
            byte[] iv = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };


            byte[] src = Convert.FromBase64String(encryptText);

            using (var aesManaged = new AesManaged())
            using (var decryptor = aesManaged.CreateDecryptor(key, iv))
            using (var inStream = new MemoryStream(src,false))
            using (var outStream = new MemoryStream()) {
                using (var cryptoStream = new CryptoStream(inStream, decryptor, CryptoStreamMode.Read)) {
                    byte[] buffer = new byte[16];
                    int len = 0;

                    while ((len = cryptoStream.Read(buffer, 0, 16)) > 0 ) {
                        outStream.Write(buffer, 0, len);
                    }

                }

                byte[] result = outStream.ToArray();
                return Encoding.Unicode.GetString(result);
            }

        }

        private string getHash(string text) {
            var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
            return string.Concat(hash.Select(b => $"{b:x2}"));
        }

        private byte[] getKey() {
            var r = new Rfc2898DeriveBytes(
                getHash(Environment.MachineName),
                new byte[16],
                1000 
            );
            return r.GetBytes(32);
        }
    }
}

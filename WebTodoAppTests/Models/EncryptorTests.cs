﻿namespace WebTodoApp.Models.Tests
{
    using System.Security.Cryptography;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EncryptorTests
    {
        [TestMethod]
        public void EncryptTest()
        {
            var encryptor = new Encryptor();

            string encrypted = encryptor.Encrypt("hello world");

            Assert.IsNotNull(encrypted, "内部値に関わらずとりあえず null ではないか。");
            Assert.IsTrue(encrypted.Length > 0, "暗号化した文字列が入力されているか。");
            Assert.IsFalse(encrypted.Contains("hello world"), "平文が暗号化文字列内に存在しない確認する。");
        }

        [TestMethod]
        public void DecryptTest()
        {
            var encryptor = new Encryptor();

            string encrypted = encryptor.Encrypt("hello world");
            string decrypted = encryptor.Decrypt(encrypted);
            Assert.AreEqual(decrypted, "hello world", "暗号化、復号文字列を比較。");

            byte[] wrongKey = new byte[32];
            encryptor.Key = wrongKey;

            CryptographicException ex = null;

            try
            {
                encryptor.Decrypt(encrypted);
            }
            catch (CryptographicException e)
            {
                ex = e;
            }

            Assert.IsNotNull(ex, "間違った暗号鍵を入力して復号した場合、例外がスローされるか");
        }
    }
}
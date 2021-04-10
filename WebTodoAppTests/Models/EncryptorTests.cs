using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebTodoApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace WebTodoApp.Models.Tests {
    [TestClass()]
    public class EncryptorTests {
        [TestMethod()]
        public void encryptTest() {
            var encryptor = new Encryptor();

            string encrypted = encryptor.encrypt("hello world");

            Assert.IsNotNull(encrypted, "内部値に関わらずとりあえず null ではないか。");
            Assert.IsTrue(encrypted.Length > 0, "暗号化した文字列が入力されているか。");
            Assert.IsFalse(encrypted.Contains("hello world"), "平文が暗号化文字列内に存在しない確認する。");

            string decrypted = encryptor.decrypt(encrypted);
            Assert.AreEqual(decrypted, "hello world", "暗号化、復号文字列を比較。");
        }
    }
}
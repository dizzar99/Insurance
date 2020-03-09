﻿using System;
using System.IO;
using System.Security.Cryptography;

namespace Auth.Common.Implementation
{
    public sealed class AesProvider : IDisposable
    {
        private bool isDisposed = false;
        private readonly AesCryptoServiceProvider aes;

        public AesProvider(byte[] key)
        {
            this.aes = new AesCryptoServiceProvider
            {
                KeySize = key.Length * 8,
                Key = key,
                IV = this.GenerateIV(),
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };
        }
        public byte[] Encrypt(byte[] plainByteText)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(AesProvider));
            }

            return this.EncryptBytes(plainByteText);
        }
        public byte[] Decrypt(byte[] cipherByteText)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(AesProvider));
            }

            return this.DecryptBytes(cipherByteText);
        }

        ~AesProvider()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.aes.Dispose();
            }

            this.isDisposed = true;
        }

        private byte[] GenerateIV()
        {
            const int size = 16;
            byte[] iv = new byte[size];
            for (int i = 0; i < size; i++)
            {
                iv[i] = 0;
            }

            return iv;
        }

        private byte[] EncryptBytes(byte[] plainByteText)
        {
            byte[] encrypted;

            ICryptoTransform encryptor = this.aes.CreateEncryptor(this.aes.Key, this.aes.IV);
            encrypted = encryptor.TransformFinalBlock(plainByteText, 0, plainByteText.Length);
            return encrypted;

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                csEncrypt.Write(plainByteText, 0, plainByteText.Length);
                encrypted = msEncrypt.ToArray();
            }

            return encrypted;
        }

        private byte[] DecryptBytes(byte[] cipherByteText)
        {
            byte[] decrypted = new byte[cipherByteText.Length];

            ICryptoTransform decryptor = this.aes.CreateDecryptor(this.aes.Key, this.aes.IV);
            decrypted = decryptor.TransformFinalBlock(cipherByteText, 0, cipherByteText.Length);
            return decrypted;
            using MemoryStream msDecrypt = new MemoryStream(cipherByteText);
            using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);

            csDecrypt.Read(decrypted, 0, cipherByteText.Length);
            return decrypted;
        }
    }
}

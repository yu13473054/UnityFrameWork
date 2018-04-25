using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class DecipherUtility
{
    private const string Key = "wodemimahenchang"; //128位加密：16字节

    private static ICryptoTransform encryptor;
    private static ICryptoTransform decryptor;

    static void Init()
    {
        RijndaelManaged rm = new RijndaelManaged
        {
            Key = Encoding.UTF8.GetBytes(Key),
            Mode = CipherMode.ECB,
            Padding = PaddingMode.PKCS7
        };
        encryptor = rm.CreateEncryptor();
        decryptor = rm.CreateDecryptor();
    }

    /// <summary>
    /// 加密
    /// </summary>
    public static string Encrypt(string input)
    {
        if (string.IsNullOrEmpty(input)) return null;
        byte[] toEncryptArray = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(EncryptBytes(toEncryptArray));
    }

    /// <summary>
    /// 解密
    /// </summary>
    public static string Decrypt(string input)
    {
        if (string.IsNullOrEmpty(input)) return null;
        byte[] bytes = Convert.FromBase64String(input);
        return Encoding.UTF8.GetString(DecryptBytes(bytes));
    }

    public static byte[] DecryptBytes(byte[] input)
    {
        if (input == null) return null;
        byte[] toEncryptArray = input;

        if (decryptor == null)
        {
            Init();
        }

        byte[] resultArray = decryptor.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

        return resultArray;
    }
    public static byte[] EncryptBytes(byte[] input)
    {
        if (input == null) return null;

        byte[] toEncryptArray = input;

        if (encryptor == null)
        {
            Init();
        }
        byte[] resultArray = encryptor.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

        return resultArray;

    }
}

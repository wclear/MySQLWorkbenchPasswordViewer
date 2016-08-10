using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ViewWorkbenchPasswords
{
    // Based on: http://www.obviex.com/samples/dpapi.aspx
    class DPAPILite
    {
        // Prompt structure to be used for required parameters.
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CRYPTPROTECT_PROMPTSTRUCT
        {
            public int cbSize;
            public int dwPromptFlags;
            public IntPtr hwndApp;
            public string szPrompt;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct DATA_BLOB
        {
            public int cbData;
            public IntPtr pbData;
        }

        // Wrapper for DPAPI CryptUnprotectData function.
        [DllImport("crypt32.dll",
                    SetLastError = true,
                    CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern
            bool CryptUnprotectData(ref DATA_BLOB pCipherText,
                                    ref string pszDescription,
                                    ref DATA_BLOB pEntropy,
                                        IntPtr pReserved,
                                    ref CRYPTPROTECT_PROMPTSTRUCT pPrompt,
                                        int dwFlags,
                                    ref DATA_BLOB pPlainText);

        // Initializes empty prompt structure.
        private static void InitPrompt(ref CRYPTPROTECT_PROMPTSTRUCT ps)
        {
            ps.cbSize = Marshal.SizeOf(typeof(CRYPTPROTECT_PROMPTSTRUCT));
            ps.dwPromptFlags = 0;
            ps.hwndApp = ((IntPtr)((int)(0)));
            ps.szPrompt = null;
        }

        // Initializes a BLOB structure from a byte array.
        private static void InitBLOB(byte[] data, ref DATA_BLOB blob)
        {
            // Use empty array for null parameter.
            if (data == null)
                data = new byte[0];

            // Allocate memory for the BLOB data.
            blob.pbData = Marshal.AllocHGlobal(data.Length);

            // Make sure that memory allocation was successful.
            if (blob.pbData == IntPtr.Zero)
                throw new Exception(
                    "Unable to allocate data buffer for BLOB structure.");

            // Specify number of bytes in the BLOB.
            blob.cbData = data.Length;

            // Copy data from original source to the BLOB structure.
            Marshal.Copy(data, 0, blob.pbData, data.Length);
        }

        internal static string Decrypt(string filepath)
        {
            DATA_BLOB plainTextBlob = new DATA_BLOB();
            DATA_BLOB cipherTextBlob = new DATA_BLOB();
            DATA_BLOB entropyBlob = new DATA_BLOB();
            CRYPTPROTECT_PROMPTSTRUCT prompt = new CRYPTPROTECT_PROMPTSTRUCT();
            string description = string.Empty;
            byte[] encryptedBytes;
            
            try
            {
                encryptedBytes = File.ReadAllBytes(filepath);
                InitBLOB(encryptedBytes, ref cipherTextBlob);
                InitBLOB(Encoding.UTF8.GetBytes(String.Empty), ref entropyBlob);
                int flags = 0x1; // CRYPTPROTECT_UI_FORBIDDEN
                bool success = CryptUnprotectData(ref cipherTextBlob, ref description, ref entropyBlob, IntPtr.Zero, ref prompt, flags, ref plainTextBlob);
                if (!success)
                {
                    int code = Marshal.GetLastWin32Error();
                    throw new Exception("Unable to decrypt file.", new Win32Exception(code));
                }
                else
                {
                    byte[] textBytes = new byte[plainTextBlob.cbData];
                    Marshal.Copy(plainTextBlob.pbData, textBytes, 0, plainTextBlob.cbData);
                    var result = Encoding.UTF8.GetString(textBytes).Replace((char)2, ' ').Replace((char)3, ' ');
                    int[] controlChars = new int[] { 0, 2, 3 };
                    foreach (var c in controlChars)
                    {
                        result = result.Replace((char)c, ' ');
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                return "An error occurred while trying to decrypt the data.";
            }
            finally
            {
                if (plainTextBlob.pbData != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(plainTextBlob.pbData);
                }

                if (cipherTextBlob.pbData != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(cipherTextBlob.pbData);
                }

                if (entropyBlob.pbData != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(entropyBlob.pbData);
                }
            }
        }
    }
}
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace IPT.Toolbox.Core.Services;

public interface IDataProtector
{
    byte[] Protect(byte[] clearData);
    byte[] Unprotect(byte[] protectedData);
}

public sealed class WindowsDpapiProtector : IDataProtector
{
    private const int CryptprotectUiForbidden = 0x1;

    public byte[] Protect(byte[] clearData) => Transform(clearData, protect: true);
    public byte[] Unprotect(byte[] protectedData) => Transform(protectedData, protect: false);

    private static byte[] Transform(byte[] input, bool protect)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Le coffre de licences nécessite Windows.");

        var inputBlob = new DataBlob();
        var outputBlob = new DataBlob();
        try
        {
            inputBlob.Data = Marshal.AllocHGlobal(input.Length);
            inputBlob.Length = input.Length;
            Marshal.Copy(input, 0, inputBlob.Data, input.Length);

            var success = protect
                ? CryptProtectData(ref inputBlob, "IPT Toolbox Sartori - Licences", IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, CryptprotectUiForbidden, out outputBlob)
                : CryptUnprotectData(ref inputBlob, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, CryptprotectUiForbidden, out outputBlob);
            if (!success)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Windows n'a pas pu ouvrir le coffre de licences.");

            var output = new byte[outputBlob.Length];
            Marshal.Copy(outputBlob.Data, output, 0, output.Length);
            return output;
        }
        finally
        {
            if (inputBlob.Data != IntPtr.Zero)
            {
                ZeroMemory(inputBlob.Data, inputBlob.Length);
                Marshal.FreeHGlobal(inputBlob.Data);
            }
            if (outputBlob.Data != IntPtr.Zero)
            {
                ZeroMemory(outputBlob.Data, outputBlob.Length);
                LocalFree(outputBlob.Data);
            }
        }
    }

    private static void ZeroMemory(IntPtr pointer, int length)
    {
        for (var i = 0; i < length; i++)
            Marshal.WriteByte(pointer, i, 0);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DataBlob
    {
        public int Length;
        public IntPtr Data;
    }

    [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CryptProtectData(ref DataBlob dataIn, string description, IntPtr optionalEntropy,
        IntPtr reserved, IntPtr prompt, int flags, out DataBlob dataOut);

    [DllImport("crypt32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CryptUnprotectData(ref DataBlob dataIn, IntPtr description, IntPtr optionalEntropy,
        IntPtr reserved, IntPtr prompt, int flags, out DataBlob dataOut);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr LocalFree(IntPtr memory);
}

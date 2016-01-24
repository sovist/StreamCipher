namespace UsbHidDevice
{
    public interface IHash
    {
        byte[] Compute(int hashSizeInBytes, params byte[][] arr);
    }
}
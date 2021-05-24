using System;
using System.Diagnostics;

using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Fpe
{
public class FpeFf3_1Engine
    : FpeEngine
{
    public FpeFf3_1Engine(): this(new AesEngine())
    {
    }

    public FpeFf3_1Engine(IBlockCipher baseCipher): base(baseCipher)
    {
        if (IsOverrideSet(SP80038G.FPE_DISABLED))
        {
            throw new InvalidOperationException("FPE disabled");
        }
    }

    public override void Init(bool forEncryption, ICipherParameters parameters)
    {
        this.forEncryption = forEncryption;

        this.fpeParameters = (FpeParameters)parameters;

        baseCipher.Init(!fpeParameters.UseInverseFunction, new KeyParameter(Arrays.Reverse(fpeParameters.Key.GetKey())));

        if (fpeParameters.GetTweak().Length != 7)
        {
            throw new ArgumentException("tweak should be 56 bits");
        }
    }

    protected override int encryptBlock(byte[] inBuf, int inOff, int length, byte[] outBuf, int outOff)
    {
        byte[] enc;

        if (fpeParameters.Radix > 256)
        {
            enc = toByteArray(SP80038G.EncryptFF3_1w(baseCipher, fpeParameters.Radix, fpeParameters.GetTweak(), toShortArray(inBuf), inOff, length / 2));
        }
        else
        {
            enc = SP80038G.EncryptFF3_1(baseCipher, fpeParameters.Radix, fpeParameters.GetTweak(), inBuf, inOff, length);
        }

        Array.Copy(enc, 0, outBuf, outOff, length);

        return length;
    }

    protected override int decryptBlock(byte[] inBuf, int inOff, int length, byte[] outBuf, int outOff)
    {
        byte[] dec;

        if (fpeParameters.Radix > 256)
        {
            dec = toByteArray(SP80038G.DecryptFF3_1w(baseCipher, fpeParameters.Radix, fpeParameters.GetTweak(), toShortArray(inBuf), inOff, length / 2));
        }
        else
        {
            dec = SP80038G.DecryptFF3_1(baseCipher, fpeParameters.Radix, fpeParameters.GetTweak(), inBuf, inOff, length);
        }

        Array.Copy(dec, 0, outBuf, outOff, length);

        return length;
    }
}
}
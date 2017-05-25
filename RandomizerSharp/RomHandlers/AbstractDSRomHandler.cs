using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RandomizerSharp.NDS;
using RandomizerSharp.PokemonModel;

namespace RandomizerSharp.RomHandlers
{
    public abstract class AbstractDsRomHandler : AbstractRomHandler, IDisposable
    {
        protected NdsRom BaseRom { get; }

        protected AbstractDsRomHandler(Stream stream) => BaseRom = new NdsRom(stream);
        protected AbstractDsRomHandler(string filename) => BaseRom = new NdsRom(filename);

        protected byte[] Get3Byte(int amount)
        {
            var ret = new byte[3];
            ret[0] = unchecked((byte) (amount & 0xFF));
            ret[1] = unchecked((byte) ((amount >> 8) & 0xFF));
            ret[2] = unchecked((byte) ((amount >> 16) & 0xFF));
            return ret;
        }

        protected static string GetRomCodeFromFile(string filename)
        {
            var fis = new FileStream(filename, FileMode.Open, FileAccess.Read);
            fis.Seek(0x0C, SeekOrigin.Current);
            var sig = FileFunctions.ReadFullyIntoBuffer(fis, 4);
            fis.Close();
            var ndsCode = Encoding.ASCII.GetString(sig, 0, sig.Length);
            return ndsCode;
        }

        protected int ReadRelativePointer(IList<byte> data, int offset) => PpTxtHandler.ReadInt(data, offset) + offset + 4;

        protected void WriteRelativePointer(IList<byte> data, int offset, int pointer)
        {
            var relPointer = pointer - (offset + 4);
            PpTxtHandler.WriteInt(data, offset, relPointer);
        }

        protected void WriteFile(string location, byte[] data, int length, int offset = 0)
        {
            if (offset != 0 || length != data.Length)
                data = data.Slice(length, offset).ToArray();

            BaseRom.WriteFile(location, data);
        }

        protected void ReadByteIntoFlags(IList<byte> data, IList<bool> flags, int offsetIntoFlags, int offsetIntoData)
        {
            var thisByte = data[offsetIntoData] & 0xFF;
            for (var i = 0; i < 8 && i + offsetIntoFlags < flags.Count; i++)
                flags[offsetIntoFlags + i] = ((thisByte >> i) & 0x01) == 0x01;
        }

        protected byte GetByteFromFlags(IList<bool> flags, int offsetIntoFlags)
        {
            var thisByte = 0;
            for (var i = 0; i < 8 && i + offsetIntoFlags < flags.Count; i++)
                thisByte |= (flags[offsetIntoFlags + i] ? 1 : 0) << i;
            return (byte) thisByte;
        }

        protected int TypeTmPaletteNumber(Typing t)
        {
            if (t == null)
                return 411;

            switch (t.InnerEnumValue)
            {
                case Typing.InnerEnum.Fighting:
                    return 398;
                case Typing.InnerEnum.Dragon:
                    return 399;
                case Typing.InnerEnum.Water:
                    return 400;
                case Typing.InnerEnum.Psychic:
                    return 401;
                case Typing.InnerEnum.Normal:
                    return 402;
                case Typing.InnerEnum.Poison:
                    return 403;
                case Typing.InnerEnum.Ice:
                    return 404;
                case Typing.InnerEnum.Grass:
                    return 405;
                case Typing.InnerEnum.Fire:
                    return 406;
                case Typing.InnerEnum.Dark:
                    return 407;
                case Typing.InnerEnum.Steel:
                    return 408;
                case Typing.InnerEnum.Electric:
                    return 409;
                case Typing.InnerEnum.Ground:
                    return 410;
                case Typing.InnerEnum.Rock:
                    return 412;
                case Typing.InnerEnum.Flying:
                    return 413;
                case Typing.InnerEnum.Bug:
                    return 610;
                case Typing.InnerEnum.Ghost:
                    return 411;
                default:
                    return 411;
            }
        }

        public void Dispose()
        {
            BaseRom?.Dispose();
        }
    }
}
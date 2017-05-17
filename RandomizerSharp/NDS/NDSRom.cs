using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace RandomizerSharp.NDS
{
    public class NdsRom
    {
        private const int Arm9Align = 0x1FF, Arm7Align = 0x1FF;
        private const int FntAlign = 0x1FF, FatAlign = 0x1FF;

        private const int BannerAlign = 0x1FF, FileAlign = 0x1FF;
        private bool _arm9Compressed;
        private byte[] _arm9Footer;
        private bool _arm9Open, _arm9Changed, _arm9HasFooter;
        private byte[] _arm9Ramstored;
        private int _arm9Szmode, _arm9Szoffset;
        private Ndsy9Entry[] _arm9Overlays;
        private IDictionary<int, Ndsy9Entry> _arm9OverlaysByFileId;
        private byte[] _fat;
        private IDictionary<string, NdsFile> _files;
        private IDictionary<int, NdsFile> _filesById;
        private string _romCode;
        private readonly string _romFilename;
        private bool _romOpen;
        
        public NdsRom(string filename)
        {
            _romFilename = filename;
            BaseRom = new FileStream(filename, FileMode.Open, FileAccess.Read);
            _romOpen = true;
            var rawFilename = Path.GetFileName(filename);

            if (rawFilename == null)
                throw new NullReferenceException();

            var dataFolder = "tmp_" + rawFilename.Substring(0, rawFilename.LastIndexOf('.'));
            dataFolder = Regex.Replace(dataFolder, "[^A-Za-z0-9_]+", "");

            Directory.CreateDirectory(dataFolder);

            WritingEnabled = true;
            TmpFolder = dataFolder + Path.PathSeparator;

            ReadFileSystem();
            _arm9Open = false;
            _arm9Changed = false;
            _arm9Ramstored = null;
        }

        public virtual string Code => _romCode;

        public string TmpFolder { get; }

        public FileStream BaseRom { get; private set; }

        public bool WritingEnabled { get; }
        
        public virtual void ReopenRom()
        {
            if (!_romOpen)
            {
                BaseRom = new FileStream(_romFilename, FileMode.Open, FileAccess.Read);
                _romOpen = true;
            }
        }
        
        public virtual void CloseRom()
        {
            if (_romOpen && BaseRom != null)
            {
                BaseRom.Close();
                BaseRom = null;
                _romOpen = false;
            }
        }
        
        private void ReadFileSystem()
        {
            BaseRom.Seek(0x0C, SeekOrigin.Begin);
            var sig = new byte[4];
            BaseRom.Read(sig, 0, sig.Length);
            _romCode = Encoding.GetEncoding("US-ASCII").GetString(sig, 0, sig.Length);
            BaseRom.Seek(0x40, SeekOrigin.Begin);
            var fntOffset = ReadFromFile(BaseRom, 4);
            ReadFromFile(BaseRom, 4);
            var fatOffset = ReadFromFile(BaseRom, 4);
            var fatSize = ReadFromFile(BaseRom, 4);
            BaseRom.Seek(fatOffset, SeekOrigin.Begin);
            _fat = new byte[fatSize];
            BaseRom.Read(_fat, 0, _fat.Length);
            IDictionary<int, string> directoryPaths = new Dictionary<int, string>();
            directoryPaths[0xF000] = "";
            var dircount = ReadFromFile(BaseRom, fntOffset + 0x6, 2);
            _files = new Dictionary<string, NdsFile>();
            _filesById = new Dictionary<int, NdsFile>();
            BaseRom.Seek(fntOffset, SeekOrigin.Begin);
            var subTableOffsets = new int[dircount];
            var firstFileIDs = new int[dircount];
            var parentDirIDs = new int[dircount];
            for (var i = 0; i < dircount && i < 0x1000; i++)
            {
                subTableOffsets[i] = ReadFromFile(BaseRom, 4) + fntOffset;
                firstFileIDs[i] = ReadFromFile(BaseRom, 2);
                parentDirIDs[i] = ReadFromFile(BaseRom, 2);
            }
            var directoryNames = new string[dircount];
            IDictionary<int, string> filenames = new SortedDictionary<int, string>();
            IDictionary<int, int> fileDirectories = new Dictionary<int, int>();
            for (var i = 0; i < dircount && i < 0x1000; i++)
                FirstPassDirectory(i, subTableOffsets[i], firstFileIDs[i], directoryNames, filenames, fileDirectories);
            for (var i = 1; i < dircount && i < 0x1000; i++)
            {
                var dirname = directoryNames[i];
                if (!ReferenceEquals(dirname, null))
                {
                    var fullDirName = "";
                    var curDir = i;
                    while (!ReferenceEquals(dirname, null) && dirname.Length > 0)
                    {
                        if (fullDirName.Length > 0)
                            fullDirName = "/" + fullDirName;
                        fullDirName = dirname + fullDirName;
                        var parentDir = parentDirIDs[curDir];
                        if (parentDir >= 0xF001 && parentDir <= 0xFFFF)
                        {
                            curDir = parentDir - 0xF000;
                            dirname = directoryNames[curDir];
                        }
                        else
                        {
                            break;
                        }
                    }
                    directoryPaths[i + 0xF000] = fullDirName;
                }
                else
                {
                    directoryPaths[i + 0xF000] = "";
                }
            }
            foreach (var fileId in filenames.Keys)
            {
                var filename = filenames[fileId];
                var directory = fileDirectories[fileId];
                var dirPath = directoryPaths[directory + 0xF000];
                var fullFilename = filename;
                if (dirPath.Length > 0)
                    fullFilename = dirPath + "/" + filename;
                var nf = new NdsFile(this);
                var start = ReadFromByteArr(_fat, fileId * 8, 4);
                var end = ReadFromByteArr(_fat, fileId * 8 + 4, 4);
                nf.Offset = start;
                nf.Size = end - start;
                nf.FullPath = fullFilename;
                nf.FileId = fileId;
                _files[fullFilename] = nf;
                _filesById[fileId] = nf;
            }
            var arm9OvlTableOffset = ReadFromFile(BaseRom, 0x50, 4);
            var arm9OvlTableSize = ReadFromFile(BaseRom, 0x54, 4);
            var arm9OvlCount = arm9OvlTableSize / 32;
            var y9Table = new byte[arm9OvlTableSize];
            _arm9Overlays = new Ndsy9Entry[arm9OvlCount];
            _arm9OverlaysByFileId = new Dictionary<int, Ndsy9Entry>();
            BaseRom.Seek(arm9OvlTableOffset, SeekOrigin.Begin);
            BaseRom.Read(y9Table, 0, y9Table.Length);
            for (var i = 0; i < arm9OvlCount; i++)
            {
                var overlay = new Ndsy9Entry(this);
                var fileId = ReadFromByteArr(y9Table, i * 32 + 24, 4);
                var start = ReadFromByteArr(_fat, fileId * 8, 4);
                var end = ReadFromByteArr(_fat, fileId * 8 + 4, 4);
                overlay.Offset = start;
                overlay.Size = end - start;
                overlay.OriginalSize = end - start;
                overlay.FileId = fileId;
                overlay.OverlayId = i;
                overlay.RamAddress = ReadFromByteArr(y9Table, i * 32 + 4, 4);
                overlay.RamSize = ReadFromByteArr(y9Table, i * 32 + 8, 4);
                overlay.BssSize = ReadFromByteArr(y9Table, i * 32 + 12, 4);
                overlay.StaticStart = ReadFromByteArr(y9Table, i * 32 + 16, 4);
                overlay.StaticEnd = ReadFromByteArr(y9Table, i * 32 + 20, 4);
                overlay.CompressedSize = ReadFromByteArr(y9Table, i * 32 + 28, 3);
                overlay.CompressFlag = y9Table[i * 32 + 31] & 0xFF;
                _arm9Overlays[i] = overlay;
                _arm9OverlaysByFileId[fileId] = overlay;
            }
        }
        
        public virtual void SaveTo(string filename)
        {
            ReopenRom();
            var fNew = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            var headersize = ReadFromFile(BaseRom, 0x84, 4);
            BaseRom.Seek(0, SeekOrigin.Begin);
            Copy(BaseRom, fNew, headersize);
            var arm9Offset = (int) (fNew.Position + Arm9Align) & ~Arm9Align;
            var oldArm9Offset = ReadFromFile(BaseRom, 0x20, 4);
            var arm9Size = ReadFromFile(BaseRom, 0x2C, 4);
            if (_arm9Open && _arm9Changed)
            {
                var newArm9 = GetArm9();
                if (_arm9Compressed)
                {
                    newArm9 = BlzCoder.Encode(newArm9, true, "arm9.bin");
                    if (_arm9Szoffset > 0)
                    {
                        var newValue = _arm9Szmode == 1 ? newArm9.Length : newArm9.Length + 0x4000;
                        WriteToByteArr(newArm9, _arm9Szoffset, 3, newValue);
                    }
                }
                arm9Size = newArm9.Length;
                fNew.Seek(arm9Offset, SeekOrigin.Begin);
                fNew.Write(newArm9, 0, newArm9.Length);
                if (_arm9HasFooter)
                    fNew.Write(newArm9, 0, newArm9.Length);
            }
            else
            {
                BaseRom.Seek(oldArm9Offset, SeekOrigin.Begin);
                fNew.Seek(arm9Offset, SeekOrigin.Begin);
                Copy(BaseRom, fNew, arm9Size + 12);
            }
            var arm9OvlOffset = (int) fNew.Position;
            var arm9OvlSize = _arm9Overlays.Length * 32;
            var arm7Offset = (arm9OvlOffset + arm9OvlSize + Arm7Align) & ~Arm7Align;
            var oldArm7Offset = ReadFromFile(BaseRom, 0x30, 4);
            var arm7Size = ReadFromFile(BaseRom, 0x3C, 4);
            BaseRom.Seek(oldArm7Offset, SeekOrigin.Begin);
            fNew.Seek(arm7Offset, SeekOrigin.Begin);
            Copy(BaseRom, fNew, arm7Size);
            var arm7OvlOffset = (int) fNew.Position;
            var oldArm7OvlOffset = ReadFromFile(BaseRom, 0x58, 4);
            var arm7OvlSize = ReadFromFile(BaseRom, 0x5C, 4);
            BaseRom.Seek(oldArm7OvlOffset, SeekOrigin.Begin);
            fNew.Seek(arm7OvlOffset, SeekOrigin.Begin);
            Copy(BaseRom, fNew, arm7OvlSize);
            var bannerOffset = (int) (fNew.Position + BannerAlign) & ~BannerAlign;
            var oldBannerOffset = ReadFromFile(BaseRom, 0x68, 4);
            var bannerSize = 0x840;
            BaseRom.Seek(oldBannerOffset, SeekOrigin.Begin);
            fNew.Seek(bannerOffset, SeekOrigin.Begin);
            Copy(BaseRom, fNew, bannerSize);
            var fntOffset = (int) (fNew.Position + FntAlign) & ~FntAlign;
            var oldFntOffset = ReadFromFile(BaseRom, 0x40, 4);
            var fntSize = ReadFromFile(BaseRom, 0x44, 4);
            BaseRom.Seek(oldFntOffset, SeekOrigin.Begin);
            fNew.Seek(fntOffset, SeekOrigin.Begin);
            Copy(BaseRom, fNew, fntSize);
            var fatOffset = (int) (fNew.Position + FatAlign) & ~FatAlign;
            var fatSize = _fat.Length;
            var newfat = new byte[_fat.Length];
            var y9Table = new byte[_arm9Overlays.Length * 32];
            var baseOffset = fatOffset + fatSize;
            var filecount = _fat.Length / 8;
            for (var fid = 0; fid < filecount; fid++)
            {
                var offsetOfFile = (baseOffset + FileAlign) & ~FileAlign;
                var fileLen = 0;
                var copiedCustom = false;
                if (_filesById.ContainsKey(fid))
                {
                    var customContents = _filesById[fid].LoadContents();

                    fNew.Seek(offsetOfFile, SeekOrigin.Begin);
                    fNew.Write(customContents, 0, customContents.Length);
                    copiedCustom = true;
                    fileLen = customContents.Length;
                }
                if (_arm9OverlaysByFileId.ContainsKey(fid))
                {
                    var entry = _arm9OverlaysByFileId[fid];
                    var overlayId = entry.OverlayId;
                    var customContents = entry.OverrideContents;

                    fNew.Seek(offsetOfFile, SeekOrigin.Begin);
                    fNew.Write(customContents, 0, customContents.Length);
                    copiedCustom = true;
                    fileLen = customContents.Length;

                    WriteToByteArr(y9Table, overlayId * 32, 4, overlayId);
                    WriteToByteArr(y9Table, overlayId * 32 + 4, 4, entry.RamAddress);
                    WriteToByteArr(y9Table, overlayId * 32 + 8, 4, entry.RamSize);
                    WriteToByteArr(y9Table, overlayId * 32 + 12, 4, entry.BssSize);
                    WriteToByteArr(y9Table, overlayId * 32 + 16, 4, entry.StaticStart);
                    WriteToByteArr(y9Table, overlayId * 32 + 20, 4, entry.StaticEnd);
                    WriteToByteArr(y9Table, overlayId * 32 + 24, 4, fid);
                    WriteToByteArr(y9Table, overlayId * 32 + 28, 3, entry.CompressedSize);
                    WriteToByteArr(y9Table, overlayId * 32 + 31, 1, entry.CompressFlag);
                }
                if (!copiedCustom)
                {
                    var fileStarts = ReadFromByteArr(_fat, fid * 8, 4);
                    var fileEnds = ReadFromByteArr(_fat, fid * 8 + 4, 4);
                    fileLen = fileEnds - fileStarts;
                    BaseRom.Seek(fileStarts, SeekOrigin.Begin);
                    fNew.Seek(offsetOfFile, SeekOrigin.Begin);
                    Copy(BaseRom, fNew, fileLen);
                }
                WriteToByteArr(newfat, fid * 8, 4, offsetOfFile);
                WriteToByteArr(newfat, fid * 8 + 4, 4, offsetOfFile + fileLen);
                baseOffset = offsetOfFile + fileLen;
            }
            fNew.Seek(fatOffset, SeekOrigin.Begin);
            fNew.Write(newfat, 0, newfat.Length);
            fNew.Seek(arm9OvlOffset, SeekOrigin.Begin);
            fNew.Write(y9Table, 0, y9Table.Length);
            var newfilesize = baseOffset;
            newfilesize = (newfilesize + 3) & ~3;
            var applicationEndOffset = newfilesize;
            if (newfilesize != baseOffset)
            {
                fNew.Seek(newfilesize - 1, SeekOrigin.Begin);
                fNew.WriteByte(0);
            }
            newfilesize |= newfilesize >> 16;
            newfilesize |= newfilesize >> 8;
            newfilesize |= newfilesize >> 4;
            newfilesize |= newfilesize >> 2;
            newfilesize |= newfilesize >> 1;
            newfilesize++;
            if (newfilesize <= 128 * 1024)
                newfilesize = 128 * 1024;
            var devcap = -18;
            var x = newfilesize;
            while (x != 0)
            {
                x >>= 1;
                devcap++;
            }
            var devicecap = devcap < 0 ? 0 : devcap;
            WriteToFile(fNew, 0x20, 4, arm9Offset);
            WriteToFile(fNew, 0x2C, 4, arm9Size);
            WriteToFile(fNew, 0x30, 4, arm7Offset);
            WriteToFile(fNew, 0x3C, 4, arm7Size);
            WriteToFile(fNew, 0x40, 4, fntOffset);
            WriteToFile(fNew, 0x48, 4, fatOffset);
            WriteToFile(fNew, 0x50, 4, arm9OvlOffset);
            WriteToFile(fNew, 0x58, 4, arm7OvlOffset);
            WriteToFile(fNew, 0x68, 4, bannerOffset);
            WriteToFile(fNew, 0x80, 4, applicationEndOffset);
            WriteToFile(fNew, 0x14, 1, devicecap);
            fNew.Seek(0, SeekOrigin.Begin);
            var headerForCrc = new byte[0x15E];
            fNew.Read(headerForCrc, 0, headerForCrc.Length);
            var crc = Crc16.Calculate(headerForCrc, 0, 0x15E);
            WriteToFile(fNew, 0x15E, 2, crc & 0xFFFF);
            fNew.Close();
            CloseRom();
        }
        
        private void Copy(FileStream from, FileStream to, int bytes)
        {
            var sizeofCopybuf = Math.Min(256 * 1024, bytes);
            var copybuf = new byte[sizeofCopybuf];
            while (bytes > 0)
            {
                var size2 = bytes >= sizeofCopybuf ? sizeofCopybuf : bytes;
                var read = from.Read(copybuf, 0, size2);
                to.Write(copybuf, 0, read);
                bytes -= read;
            }
        }
        
        public virtual byte[] GetFile(string filename)
        {
            if (_files.ContainsKey(filename))
                return _files[filename].LoadContents();

            return null;
        }
        
        public virtual byte[] GetOverlay(int number)
        {
            if (number >= 0 && number < _arm9Overlays.Length)
                return _arm9Overlays[number].GetContents();
            return null;
        }

        public virtual int GetOverlayAddress(int number)
        {
            if (number >= 0 && number < _arm9Overlays.Length)
                return _arm9Overlays[number].RamAddress;
            return -1;
        }
        
        public virtual byte[] GetArm9()
        {
            if (!_arm9Open)
            {
                _arm9Open = true;
                ReopenRom();
                var arm9Offset = ReadFromFile(BaseRom, 0x20, 4);
                var arm9Size = ReadFromFile(BaseRom, 0x2C, 4);
                var arm9 = new byte[arm9Size];
                BaseRom.Seek(arm9Offset, SeekOrigin.Begin);
                BaseRom.Read(arm9, 0, arm9.Length);
                var nitrocode = ReadFromFile(BaseRom, 4);

                if (nitrocode == unchecked((int)0xDEC00621))
                {
                    _arm9Footer = new byte[12];
                    WriteToByteArr(_arm9Footer, 0, 4, unchecked((int) 0xDEC00621));
                    BaseRom.Read(_arm9Footer, 4, 8);
                    _arm9HasFooter = true;
                }
                else
                {
                    _arm9HasFooter = false;
                }

                while (ReadFromByteArr(arm9, arm9.Length - 12, 4) == unchecked((int)0xDEC00621) ||
                       ReadFromByteArr(arm9, arm9.Length - 12, 4) == 0 &&
                       ReadFromByteArr(arm9, arm9.Length - 8, 4) == 0 &&
                       ReadFromByteArr(arm9, arm9.Length - 4, 4) == 0)
                {
                    if (!_arm9HasFooter)
                    {
                        _arm9HasFooter = true;
                        _arm9Footer = new byte[0];
                    }
                    var newfooter = new byte[_arm9Footer.Length + 12];
                    Array.Copy(arm9, arm9.Length - 12, newfooter, 0, 12);
                    Array.Copy(_arm9Footer, 0, newfooter, 12, _arm9Footer.Length);
                    _arm9Footer = newfooter;
                    var newarm9 = new byte[arm9.Length - 12];
                    Array.Copy(arm9, 0, newarm9, 0, arm9.Length - 12);
                    arm9 = newarm9;
                }
                _arm9Compressed = false;
                _arm9Szoffset = 0;
                if (arm9[arm9.Length - 5] >= 0x08 && arm9[arm9.Length - 5] <= 0x0B)
                {
                    var compSize = ReadFromByteArr(arm9, arm9.Length - 8, 3);
                    if (compSize > arm9.Length * 9 / 10 && compSize < arm9.Length * 11 / 10)
                    {
                        _arm9Compressed = true;
                        var compLength = new byte[3];
                        WriteToByteArr(compLength, 0, 3, arm9.Length);
                        var foundOffsets = RomFunctions.Search(arm9, compLength);
                        if (foundOffsets.Count == 1)
                        {
                            _arm9Szmode = 1;
                            _arm9Szoffset = foundOffsets[0];
                        }
                        else
                        {
                            var compLength2 = new byte[3];
                            WriteToByteArr(compLength2, 0, 3, arm9.Length + 0x4000);
                            var foundOffsets2 = RomFunctions.Search(arm9, compLength2);
                            if (foundOffsets2.Count == 1)
                            {
                                _arm9Szmode = 2;
                                _arm9Szoffset = foundOffsets2[0];
                            }
                        }
                    }
                }
                if (_arm9Compressed)
                    arm9 = BlzCoder.Decode(arm9, "arm9.bin");

                if (WritingEnabled)
                {
                    var arm9File = TmpFolder + "arm9.bin";
                    var fos =
                        new FileStream(arm9File, FileMode.Create, FileAccess.Write);
                    fos.Write(arm9, 0, arm9.Length);
                    fos.Close();
                    _arm9Ramstored = null;
                    return arm9;
                }
                _arm9Ramstored = arm9;
                var newcopy = new byte[arm9.Length];
                Array.Copy(arm9, 0, newcopy, 0, arm9.Length);
                return newcopy;
            }
            if (WritingEnabled)
            {
                var file = FileFunctions.ReadFileFullyIntoBuffer(TmpFolder + "arm9.bin");
                return file;
            }
            {
                var newcopy = new byte[_arm9Ramstored.Length];
                Array.Copy(_arm9Ramstored, 0, newcopy, 0, _arm9Ramstored.Length);
                return newcopy;
            }
        }

        public void WriteFile(string filename, byte[] data)
        {
            if (_files.ContainsKey(filename))
                _files[filename].WriteOverride(data);
        }

        public void WriteOverlay(int number, byte[] data)
        {
            if (number >= 0 && number <= _arm9Overlays.Length)
                _arm9Overlays[number].WriteOverride(data);
        }

        public void WriteArm9(byte[] arm9)
        {
            if (!_arm9Open)
                GetArm9();

            _arm9Changed = true;
            if (WritingEnabled)

            {
                var fos = new FileStream(TmpFolder + "arm9.bin", FileMode.Create,
                    FileAccess.Write);
                fos.Write(arm9, 0, arm9.Length);
                fos.Close();
            }
            else

            {
                if (_arm9Ramstored.Length == arm9.Length)
                {
                    Array.Copy(arm9, 0, _arm9Ramstored, 0, arm9.Length);
                }
                else
                {
                    _arm9Ramstored = null;
                    _arm9Ramstored = new byte[arm9.Length];
                    Array.Copy(arm9, 0, _arm9Ramstored, 0, arm9.Length);
                }
            }
        }

        private void FirstPassDirectory(int dir, int subTableOffset, int firstFileId, string[] directoryNames,
            IDictionary<int, string> filenames, IDictionary<int, int> fileDirectories)
        {
            BaseRom.Seek(subTableOffset, SeekOrigin.Begin);
            while (true)

            {
                var control = BaseRom.ReadByte();
                if (control == 0x00)
                    break;
                var namelen = control & 0x7F;
                var rawname = new byte[namelen];
                BaseRom.Read(rawname, 0, rawname.Length);
                var name = Encoding.GetEncoding("US-ASCII").GetString(rawname, 0, rawname.Length);
                if ((control & 0x80) > 0x00)
                {
                    var subDirectoryId = ReadFromFile(BaseRom, 2);
                    directoryNames[subDirectoryId - 0xF000] = name;
                }
                else
                {
                    var fileId = firstFileId++;
                    filenames[fileId] = name;
                    fileDirectories[fileId] = dir;
                }
            }
        }

        public int ReadFromByteArr(byte[] data, int offset, int size)
        {
            var result = 0;
            for (var i = 0; i < size; i++)
                result |= (data[i + offset] & 0xFF) << (i * 8);
            return result;
        }

        public void WriteToByteArr(byte[] data, int offset, int size, int value)
        {
            for (var i = 0; i < size; i++)
                data[offset + i] = unchecked((byte) ((value >> (i * 8)) & 0xFF));
        }

        public int ReadFromFile(FileStream file, int size)
        {
            return ReadFromFile(file, -1, size);
        }

        public int ReadFromFile(FileStream file, int offset, int size)
        {
            var
                buf = new byte[size];
            if (offset >= 0)
                file.Seek(offset);
            file.ReadFully(buf);
            var result = 0;
            for (var i = 0; i < size; i++)
                result |= (buf[i] & 0xFF) << (i * 8);
            return result;
        }

        public void WriteToFile(FileStream file, int size, int value)
        {
            WriteToFile(file, -1, size, value);
        }

        public void WriteToFile(FileStream file, int offset, int size, int value)
        {
            var
                buf = new byte[size];
            for (var i = 0; i < size; i++)
                buf[i] = unchecked((byte) ((value >> (i * 8)) & 0xFF));
            if (offset >= 0)
                file.Seek(offset);
            file.Write(buf, 0, buf.Length);
        }
    }
}
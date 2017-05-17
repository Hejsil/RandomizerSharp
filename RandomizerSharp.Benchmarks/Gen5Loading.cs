using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using RandomizerSharp.Benchmarks.Properties;
using RandomizerSharp.NDS;
using RandomizerSharp.RomHandlers;

namespace RandomizerSharp.Benchmarks
{
    public class Gen5Loading
    {
        [Benchmark]
        public NdsRom NdsRomConstructor() => new NdsRom(Resources.RomPath);

        [Benchmark]
        public bool Gen5RomHandlerLoad() => new Gen5RomHandler(new Random()).LoadRom(Resources.RomPath);
    }
}

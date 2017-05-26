using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using RandomizerSharp.PokemonModel;
using RandomizerSharp.RomHandlers;
using RandomizerSharp.Tests.Properties;

namespace RandomizerSharp.Tests
{
    [TestFixture]
    public class RomHandlerTest
    {
        private static readonly string[] RomPaths =
        {
            Resources.Gen5Rom
        };

        private static readonly Func<Stream, AbstractRomHandler>[] RomHandlerCreators =
        {
            stream => new Gen5RomHandler(stream)
        };

        private static void TestOnAll(Action<AbstractRomHandler> edit, Action<AbstractRomHandler, AbstractRomHandler> varify, [CallerMemberName] string id = "")
        {
            foreach (var (creator, filePath) in RomHandlerCreators.Zip(RomPaths, (func, s) => (func, s)))
            {
                using (var file = File.OpenRead(filePath))
                {
                    var handler = creator(file);
                    edit(handler);

                    using (var newFile = File.Create(id, 10_000, FileOptions.DeleteOnClose))
                    {
                        handler.SaveRom(newFile);
                        var newHandler = creator(newFile);
                        varify(handler, newHandler);
                    }
                }
            }
        }

        [Test]
        public void TestStarters()
        {
            TestOnAll(
                handler =>
                {
                    foreach (var starter in handler.Starters)
                    {
                        starter.Pokemon = handler.AllPokemons[0];
                        starter.HeldItem = 0;
                    }
                },
                (handler, newHandler) =>
                {
                    foreach (var (oldS, newS) in handler.Starters.Zip(newHandler.Starters, (oldStarer, newStarter) => (oldStarer, newStarter)))
                    {
                        Assert.AreEqual(oldS.Pokemon.Id, newS.Pokemon.Id);
                        Assert.AreEqual(oldS.HeldItem, newS.HeldItem);
                    }
                }
            );
        }

        [Test]
        public void TestAllPokemons()
        {
            TestOnAll(
                handler =>
                {
                    foreach (var starter in handler.AllPokemons)
                    {
                        starter.EvolutionsFrom.Clear();
                        starter.EvolutionsTo.Clear();
                        starter.MovesLearnt.Clear();
                        starter.Ability1 = 0;
                        starter.Ability2 = 0;
                        starter.Ability3 = 0;
                        starter.Hp = 0;
                        starter.Attack = 0;
                        starter.Defense = 0;
                        starter.Spatk = 0;
                        starter.Spdef = 0;
                        starter.Speed = 0;
                        starter.CatchRate = 0;
                        starter.CommonHeldItem = 0;
                        starter.RareHeldItem = 0;
                        starter.DarkGrassHeldItem = 0;
                        starter.GrowthExpCurve = ExpCurve.Slow;
                        starter.Name = "";
                        starter.PrimaryType = Typing.Bug;
                        starter.SecondaryType = Typing.Bug;
                        starter.TMHMCompatibility.Populate(false);
                        starter.MoveTutorCompatibility.Populate(false);

                    }
                },
                (handler, newHandler) =>
                {
                    foreach (var (oldP, newP) in handler.AllPokemons.Zip(newHandler.AllPokemons))
                    {
                        Assert.AreEqual(oldP.EvolutionsFrom.Count, newP.EvolutionsTo.Count);
                        Assert.AreEqual(oldP.EvolutionsTo.Count, newP.EvolutionsTo.Count);
                        Assert.AreEqual(oldP.MovesLearnt.Count, newP.MovesLearnt.Count);
                        Assert.AreEqual(oldP.Ability1, newP.Ability1);
                        Assert.AreEqual(oldP.Ability2, newP.Ability2);
                        Assert.AreEqual(oldP.Ability3, newP.Ability3);
                        Assert.AreEqual(oldP.Hp, newP.Hp);
                        Assert.AreEqual(oldP.Attack, newP.Attack);
                        Assert.AreEqual(oldP.Defense, newP.Defense);
                        Assert.AreEqual(oldP.Spatk, newP.Spatk);
                        Assert.AreEqual(oldP.Spatk, newP.Spatk);
                        Assert.AreEqual(oldP.Spdef, newP.Spdef);
                        Assert.AreEqual(oldP.Special, newP.Special);
                        Assert.AreEqual(oldP.Speed, newP.Speed);
                        Assert.AreEqual(oldP.CatchRate, newP.CatchRate);
                        Assert.AreEqual(oldP.CommonHeldItem, newP.CommonHeldItem);
                        Assert.AreEqual(oldP.DarkGrassHeldItem, newP.DarkGrassHeldItem);
                        Assert.AreEqual(oldP.GrowthExpCurve, newP.GrowthExpCurve);
                        Assert.AreEqual(oldP.Name, newP.Name);
                        Assert.AreEqual(oldP.PrimaryType, newP.PrimaryType);
                        Assert.AreEqual(oldP.SecondaryType, newP.SecondaryType);
                        Assert.AreEqual(oldP.RareHeldItem, newP.RareHeldItem);
                        Assert.True(oldP.TMHMCompatibility.SequenceEqual(newP.TMHMCompatibility));
                        Assert.True(oldP.MoveTutorCompatibility.SequenceEqual(newP.MoveTutorCompatibility));
                    }
                }
            );
        }

        [Test]
        public void TestStaticPokemon()
        {
            TestOnAll(
                handler =>
                {
                    handler.StaticPokemon.Populate(handler.AllPokemons[0]);
                },
                (handler, newHandler) =>
                {
                    foreach (var (oldS, newS) in handler.StaticPokemon.Zip(newHandler.StaticPokemon))
                    {
                        Assert.AreEqual(oldS.Id, newS.Id);
                    }
                }
            );
        }

        [Test]
        public void TestAbilityNames()
        {
            TestOnAll(
                handler =>
                {
                    handler.AbilityNames.Populate("");
                },
                (handler, newHandler) =>
                {
                    foreach (var (oldN, newN) in handler.AbilityNames.Zip(newHandler.AbilityNames))
                    {
                        Assert.AreEqual(oldN, newN);
                    }
                }
            );
        }

        [Test]
        public void TestRequiredFiledTMs()
        {
            Assert.Fail();
        }

        [Test]
        public void TestAllMoves()
        {
            TestOnAll(
                handler =>
                {
                    foreach (var move in handler.AllMoves)
                    {
                        move.Category = MoveCategory.Physical;
                        move.Hitratio = 0;
                        move.Name = "";
                        move.Power = 0;
                        move.Pp = 0;
                        move.Type = Typing.Bug;
                    }
                },
                (handler, newHandler) =>
                {
                    foreach (var (oldM, newM) in handler.AllMoves.Zip(newHandler.AllMoves))
                    {
                        Assert.AreEqual(oldM.Category, newM.Category);
                        Assert.AreEqual(oldM.Hitratio, newM.Hitratio);
                        Assert.AreEqual(oldM.Name, newM.Name);
                        Assert.AreEqual(oldM.Power, newM.Power);
                        Assert.AreEqual(oldM.Pp, newM.Pp);
                        Assert.AreEqual(oldM.Type, newM.Type);
                    }
                }
            );
        }

        [Test]
        public void TestFieldMoves()
        {
            Assert.Fail();
        }

        [Test]
        public void TestEarlyRequiredHmMoves()
        {
            Assert.Fail();
        }

        [Test]
        public void TestMoveTutorMoves()
        {
            Assert.Fail();
        }

        [Test]
        public void TestCurrentFieldTMs()
        {
            Assert.Fail();
        }

        [Test]
        public void TestFieldItems()
        {
            Assert.Fail();
        }

        [Test]
        public void TestHmMoves()
        {
            Assert.Fail();
        }

        [Test]
        public void TestTmMoves()
        {
            Assert.Fail();
        }

        [Test]
        public void TestAllowedItems()
        {
            Assert.Fail();
        }

        [Test]
        public void TestNonBadItems()
        {
            Assert.Fail();
        }

        [Test]
        public void TestItemNames()
        {
            Assert.Fail();
        }

        [Test]
        public void TestRegularFieldItems()
        {
            Assert.Fail();
        }

        [Test]
        public void TestCanChangeTrainerText()
        {
            Assert.Fail();
        }

        [Test]
        public void TestFixedTrainerClassNamesLength()
        {
            Assert.Fail();
        }

        [Test]
        public void TestDoublesTrainerClasses()
        {
            Assert.Fail();
        }

        [Test]
        public void TestTrainerClassNames()
        {
            Assert.Fail();
        }

        [Test]
        public void TestTrainerNames()
        {
            Assert.Fail();
        }

        [Test]
        public void TestTrainers()
        {
            Assert.Fail();
        }

        [Test]
        public void TestMaxSumOfTrainerNameLengths()
        {
            Assert.Fail();
        }

        [Test]
        public void TestMaxTrainerClassNameLength()
        {
            Assert.Fail();
        }

        [Test]
        public void TestMaxTrainerNameLength()
        {
            Assert.Fail();
        }

        [Test]
        public void TestTrainerNameMode()
        {
            Assert.Fail();
        }

        [Test]
        public void TestGame()
        {
            Assert.Fail();
        }

        [Test]
        public void TestLoadedFilename()
        {
            Assert.Fail();
        }

        [Test]
        public void TestEncounters()
        {
            Assert.Fail();
        }

        [Test]
        public void TestIngameTrades()
        {
            Assert.Fail();
        }
    }
}
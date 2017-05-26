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
                    foreach (var (oldS, newS) in handler.Starters.Zip(newHandler.Starters))
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
        public void TestMoveTutorMoves()
        {
            TestOnAll(
                handler =>
                {
                    handler.MoveTutorMoves.Populate(0);
                },
                (handler, newHandler) =>
                {
                    foreach (var (oldM, newM) in handler.MoveTutorMoves.Zip(newHandler.MoveTutorMoves))
                    {
                        Assert.AreEqual(oldM, newM);
                    }
                }
            );
        }
        
        [Test]
        public void TestFieldItems()
        {
            TestOnAll(
                handler =>
                {
                    handler.FieldItems.Populate(0);
                },
                (handler, newHandler) =>
                {
                    foreach (var (oldItem, newItem) in handler.FieldItems.Zip(newHandler.FieldItems))
                    {
                        Assert.AreEqual(oldItem, newItem);
                    }
                }
            );
        }

        [Test]
        public void TestHmMoves()
        {
            TestOnAll(
                handler =>
                {
                    handler.HmMoves.Populate(0);
                },
                (handler, newHandler) =>
                {
                    foreach (var (oldHm, newHm) in handler.HmMoves.Zip(newHandler.HmMoves))
                    {
                        Assert.AreEqual(oldHm, newHm);
                    }
                }
            );
        }

        [Test]
        public void TestTmMoves()
        {
            TestOnAll(
                handler =>
                {
                    handler.TmMoves.Populate(0);
                },
                (handler, newHandler) =>
                {
                    foreach (var (oldTm, newTm) in handler.TmMoves.Zip(newHandler.TmMoves))
                    {
                        Assert.AreEqual(oldTm, newTm);
                    }
                }
            );
        }

        [Test]
        public void TestItemNames()
        {
            TestOnAll(
                handler =>
                {
                    handler.ItemNames.Populate("");
                },
                (handler, newHandler) =>
                {
                    foreach (var (oldName, newName) in handler.ItemNames.Zip(newHandler.ItemNames))
                    {
                        Assert.AreEqual(oldName, newName);
                    }
                }
            );
        }

        [Test]
        public void TestDoublesTrainerClasses()
        {
            TestOnAll(
                handler =>
                {
                    handler.DoublesTrainerClasses.Populate(0);
                },
                (handler, newHandler) =>
                {
                    foreach (var (oldClass, newClass) in handler.DoublesTrainerClasses.Zip(newHandler.DoublesTrainerClasses))
                    {
                        Assert.AreEqual(oldClass, newClass);
                    }
                }
            );
        }

        [Test]
        public void TestTrainerClassNames()
        {
            TestOnAll(
                handler =>
                {
                    handler.TrainerClassNames.Populate("");
                },
                (handler, newHandler) =>
                {
                    foreach (var (oldClass, newClass) in handler.TrainerClassNames.Zip(newHandler.TrainerClassNames))
                    {
                        Assert.AreEqual(oldClass, newClass);
                    }
                }
            );
        }

        [Test]
        public void TestTrainerNames()
        {
            TestOnAll(
                handler =>
                {
                    handler.TrainerNames.Populate("");
                },
                (handler, newHandler) =>
                {
                    foreach (var (oldName, newName) in handler.TrainerNames.Zip(newHandler.TrainerNames))
                    {
                        Assert.AreEqual(oldName, newName);
                    }
                }
            );
        }

        [Test]
        public void TestTrainers()
        {
            TestOnAll(
                handler =>
                {
                    // TODO: Not done
                    foreach (var trainer in handler.Trainers)
                    {
                        foreach (var pokemon in trainer.Pokemon)
                        {
                            pokemon.Pokemon = handler.AllPokemons[0];
                            pokemon.Ability = 0;
                            pokemon.AiLevel = 0;
                            pokemon.HeldItem = 0;
                            pokemon.Level = 0;
                            pokemon.Move1 = 0;
                            pokemon.Move2 = 0;
                            pokemon.Move3 = 0;
                            pokemon.Move4 = 0;
                            pokemon.ResetMoves = false;
                        }
                    }
                },
                (handler, newHandler) =>
                {
                    foreach (var (oldName, newName) in handler.TrainerNames.Zip(newHandler.TrainerNames))
                    {
                        Assert.AreEqual(oldName, newName);
                    }
                }
            );
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
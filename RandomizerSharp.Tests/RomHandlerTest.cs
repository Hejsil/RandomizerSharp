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
        private static readonly Random Random = new Random(0);

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
            foreach (var (creator, filePath) in RomHandlerCreators.Zip(RomPaths))
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
                        starter.Pokemon = handler.AllPokemons[Random.Next(handler.AllPokemons.Length)];

                        // TODO: Not all games support starter helditems
                        //starter.HeldItem = _random.Next(256);
                    }
                },
                (handler, newHandler) =>
                {
                    foreach (var (oldS, newS) in handler.Starters.Zip(newHandler.Starters))
                    {
                        Assert.AreEqual(oldS.Pokemon.Id, newS.Pokemon.Id);

                        // TODO: Not all games support starter helditems
                        //Assert.AreEqual(oldS.HeldItem, newS.HeldItem);
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
                    foreach (var pokemon in handler.AllPokemons)
                    {
                        pokemon.EvolutionsFrom.Clear();
                        pokemon.EvolutionsTo.Clear();
                        pokemon.MovesLearnt.Clear();
                        pokemon.Ability1 = Random.Next(256);
                        pokemon.Ability2 = Random.Next(256);
                        pokemon.Ability3 = Random.Next(256);
                        pokemon.Hp = Random.Next(256);
                        pokemon.Attack = Random.Next(256);
                        pokemon.Defense = Random.Next(256);
                        pokemon.Spatk = Random.Next(256);
                        pokemon.Spdef = Random.Next(256);
                        pokemon.Speed = Random.Next(256);
                        pokemon.CatchRate = Random.Next(256);
                        pokemon.CommonHeldItem = Random.Next(256);
                        pokemon.RareHeldItem = Random.Next(256);
                        pokemon.DarkGrassHeldItem = Random.Next(256);

                        var curvers = (ExpCurve[]) Enum.GetValues(typeof(ExpCurve));
                        pokemon.GrowthExpCurve = curvers[Random.Next(curvers.Length)];
                        
                        // TODO: Random name
                        pokemon.Name = "";
                        pokemon.TMHMCompatibility.Populate(() => Convert.ToBoolean(Random.Next(2)));
                        pokemon.MoveTutorCompatibility.Populate(() => Convert.ToBoolean(Random.Next(2)));

                        // TODO: Some types are not in all games
                        // starter.PrimaryType = Typing.Values()[_random.Next(Typing.Values().Count)];
                        // starter.SecondaryType = Typing.Values()[_random.Next(Typing.Values().Count)];

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
                    handler.StaticPokemon.Populate(() => handler.AllPokemons[Random.Next(handler.AllPokemons.Length)]);
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
                    // TODO: Random ability name
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
                        var categories = (MoveCategory[]) Enum.GetValues(typeof(MoveCategory));
                        move.Category = categories[Random.Next(categories.Length)];

                        // TODO: Should be a byte long, but doubles are weird
                        //move.Hitratio = _random.Next(256); 

                        // TODO: Random name
                        move.Name = "";
                        move.Power = Random.Next(256);
                        move.Pp = Random.Next(256);
                        
                        // TODO: Some types are not in all games
                        // move.Type = Typing.Values()[_random.Next(Typing.Values().Count)];
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
                    handler.MoveTutorMoves.Populate(() => Random.Next(256));
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
                    handler.FieldItems.Populate(() => Random.Next(256));
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
                    handler.HmMoves.Populate(() => Random.Next(256));
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
                    handler.TmMoves.Populate(() => Random.Next(256));
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
                    // TODO: Random name
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
        public void TestTrainerClassNames()
        {
            TestOnAll(
                handler =>
                {
                    // TODO: Random name
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
                    // TODO: Random name
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
                    foreach (var trainer in handler.Trainers)
                    {
                        foreach (var pokemon in trainer.Pokemon)
                        {
                            pokemon.Pokemon = handler.AllPokemons[Random.Next(handler.AllPokemons.Length)];


                            // TODO: Not all trainer pokemons can have held items
                            // pokemon.HeldItem = _random.Next(256);
                            // pokemon.Move1 = _random.Next(256);
                            // pokemon.Move2 = _random.Next(256);
                            // pokemon.Move3 = _random.Next(256);
                            // pokemon.Move4 = _random.Next(256);
                            // pokemon.Ability = _random.Next(256);

                            // pokemon.AiLevel = _random.Next(256);
                            // pokemon.Level = _random.Next(256);
                        }

                        // trainer.Poketype = 100;
                        // trainer.Trainerclass = 100;
                    }
                },
                (handler, newHandler) =>
                {
                    foreach (var (oldTrainer, newTrainer) in handler.Trainers.Zip(newHandler.Trainers))
                    {
                        Assert.AreEqual(oldTrainer.Pokemon.Length, newTrainer.Pokemon.Length);

                        // TODO: We can't really test PokeType and class, as BW2 has edge cases we need to figure out
                        //Assert.AreEqual(oldTrainer.Poketype, newTrainer.Poketype);
                        //Assert.AreEqual(oldTrainer.Trainerclass, newTrainer.Trainerclass);

                        // TODO: Only certain trainers can have most of their pokemons fields changed
                        foreach (var (oldPoke, newPoke) in oldTrainer.Pokemon.Zip(newTrainer.Pokemon))
                        {
                            // TODO: Same goes for AiLevel and Level
                            //Assert.AreEqual(oldPoke.AiLevel, newPoke.AiLevel);
                            //Assert.AreEqual(oldPoke.Level, newPoke.Level);

                            // TODO: Not all trainer pokemons can have held items
                            // Assert.AreEqual(oldPoke.HeldItem, newPoke.HeldItem);
                            // Assert.AreEqual(oldPoke.Move1, newPoke.Move1);
                            // Assert.AreEqual(oldPoke.Move2, newPoke.Move2);
                            // Assert.AreEqual(oldPoke.Move3, newPoke.Move3);
                            // Assert.AreEqual(oldPoke.Move4, newPoke.Move4);
                            // Assert.AreEqual(oldPoke.Ability, newPoke.Ability);

                            Assert.AreEqual(oldPoke.Pokemon.Id, newPoke.Pokemon.Id);
                        }
                    }
                }
            );
        }

        [Test]
        public void TestEncounters()
        {
            TestOnAll(
                handler =>
                {
                    foreach (var encSet in handler.Encounters)
                    {
                        encSet.Rate = 100;

                        foreach (var encounter in encSet.Encounters)
                        {
                            encounter.Level = Random.Next(256);
                            encounter.MaxLevel = Random.Next(256);
                            encounter.Pokemon1 = handler.AllPokemons[Random.Next(handler.AllPokemons.Length)];
                        }
                    }
                },
                (handler, newHandler) =>
                {
                    foreach (var (oldEncSet, newEncSet) in handler.Encounters.Zip(newHandler.Encounters))
                    {
                        Assert.AreEqual(oldEncSet.Rate, newEncSet.Rate);

                        foreach (var (oldEnc, newEnc) in oldEncSet.Encounters.Zip(newEncSet.Encounters))
                        {
                            Assert.AreEqual(oldEnc.Level, newEnc.Level);
                            Assert.AreEqual(oldEnc.MaxLevel, newEnc.MaxLevel);
                            Assert.AreEqual(oldEnc.Pokemon1.Id, newEnc.Pokemon1.Id);
                        }
                    }
                }
            );
        }
    }
}
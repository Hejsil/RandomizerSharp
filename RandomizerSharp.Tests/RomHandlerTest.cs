using NUnit.Framework;
using System;
using System.Linq;
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

        private static readonly Func<string, AbstractRomHandler>[] RomHandlerCreators =
        {
            filename => new Gen5RomHandler(filename)
        };

        private static void TestOnAll(Action<AbstractRomHandler> edit, Action<AbstractRomHandler, AbstractRomHandler> varify)
        {
            foreach (var (creator, filePath) in RomHandlerCreators.Zip(RomPaths, (func, s) => (func, s)))
            {
                var newPath = Guid.NewGuid().ToString("N");  
                var handler = creator(filePath);
                edit(handler);
                handler.SaveRom(newPath);
                var newHandler = creator(newPath);
                varify(handler, newHandler);
            }
        }

        [Test]
        public void TestChangeStarters()
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
                    foreach (var (oldStarter, newStarter) in handler.Starters.Zip(newHandler.Starters, (oldStarer, newStarter) => (oldStarer, newStarter)))
                    {
                        Assert.AreEqual(oldStarter.Pokemon.Id, newStarter.Pokemon.Id);
                        Assert.AreEqual(oldStarter.HeldItem, newStarter.HeldItem);
                    }
                }
            );
        }

        [Test]
        public void TestChangeAllPokemons()
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
                        starter.Special = 0;
                        starter.Speed = 0;
                        starter.CatchRate = 0;
                        starter.CommonHeldItem = 0;
                        starter.DarkGrassHeldItem = 0;
                        starter.ExpYield = 0;
                        starter.FrontSpritePointer = 0;
                        starter.GenderRatio = 0;
                        starter.GrowthExpCurve = 0;
                        starter.GuaranteedHeldItem = 0;
                        starter.Name = "";
                        starter.PicDimensions = 0;
                        starter.PrimaryType = Typing.Bug;
                        starter.SecondaryType = Typing.Bug;
                        starter.RareHeldItem = 0;
                        starter.TMHMCompatibility.Populate(false);
                        starter.MoveTutorCompatibility.Populate(false);

                    }
                },
                (handler, newHandler) =>
                {
                    foreach (var (oldP, newP) in handler.AllPokemons.Zip(newHandler.AllPokemons, (oldP, newP) => (oldP, newP)))
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
                        Assert.AreEqual(oldP.ExpYield, newP.ExpYield);
                        Assert.AreEqual(oldP.FrontSpritePointer, newP.FrontSpritePointer);
                        Assert.AreEqual(oldP.GenderRatio, newP.GenderRatio);
                        Assert.AreEqual(oldP.GrowthExpCurve, newP.GrowthExpCurve);
                        Assert.AreEqual(oldP.GuaranteedHeldItem, newP.GuaranteedHeldItem);
                        Assert.AreEqual(oldP.Name, newP.Name);
                        Assert.AreEqual(oldP.PicDimensions, newP.PicDimensions);
                        Assert.AreEqual(oldP.PrimaryType, newP.PrimaryType);
                        Assert.AreEqual(oldP.SecondaryType, newP.SecondaryType);
                        Assert.AreEqual(oldP.RareHeldItem, newP.RareHeldItem);
                        Assert.True(oldP.TMHMCompatibility.SequenceEqual(newP.TMHMCompatibility));
                        Assert.True(oldP.MoveTutorCompatibility.SequenceEqual(newP.MoveTutorCompatibility));
                    }
                }
            );
        }
    }
}
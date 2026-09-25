using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using MergeStudio.Gameplay;

namespace MergeStudio.Tests
{
    public sealed class TripleMatchGameTests
    {
        private static readonly string[] Items = Enumerable.Range(0, 40).Select(index => "food-" + index).ToArray();

        [Test]
        public void EveryLevelBuildsCompleteTriplesAndIncreasingComplexity()
        {
            int previousCount = 0;
            for (int level = 1; level <= 100; level++)
            {
                var game = new TripleMatchGame(level, Items);
                TripleMatchSnapshot snapshot = game.Snapshot();
                Assert.AreEqual(0, snapshot.TotalTiles % 3, "Level " + level);
                Assert.GreaterOrEqual(snapshot.TotalTiles, previousCount, "Level " + level);
                Assert.That(snapshot.Tiles.GroupBy(tile => tile.ItemId).All(group => group.Count() % 3 == 0));
                previousCount = snapshot.TotalTiles;
            }
        }

        [Test]
        public void SelectingThreeIdenticalFoodsClearsTheTrayAndScores()
        {
            var game = new TripleMatchGame(1, Items);
            var triple = game.Snapshot().Tiles.GroupBy(tile => tile.ItemId).First().Take(3).ToArray();
            foreach (TripleTileSnapshot tile in triple) Assert.IsTrue(game.Select(tile.Index));
            Assert.IsEmpty(game.Tray);
            Assert.Greater(game.Score, 0);
            Assert.AreEqual(3, game.Snapshot().ClearedTiles);
        }

        [Test]
        public void SevenUnmatchedSelectionsLoseTheLevel()
        {
            var game = new TripleMatchGame(1, Items);
            var selected = new List<TripleTileSnapshot>();
            foreach (var group in game.Snapshot().Tiles.GroupBy(tile => tile.ItemId))
            {
                selected.AddRange(group.Take(2));
                if (selected.Count >= 7) break;
            }
            foreach (TripleTileSnapshot tile in selected.Take(7)) game.Select(tile.Index);
            Assert.AreEqual(TripleMatchState.Lost, game.State);
            Assert.AreEqual(7, game.Tray.Count);
        }

        [Test]
        public void UndoRestoresThePreviousBoardAndTray()
        {
            var game = new TripleMatchGame(12, Items);
            int index = game.Snapshot().Tiles[0].Index;
            Assert.IsTrue(game.Select(index));
            Assert.AreEqual(1, game.Tray.Count);
            Assert.IsTrue(game.Undo());
            Assert.IsEmpty(game.Tray);
            Assert.IsTrue(game.Snapshot().Tiles[index].Active);
            Assert.AreEqual(2, game.UndoRemaining);
        }

        [Test]
        public void TimedLevelsFailWhenTheClockExpires()
        {
            var game = new TripleMatchGame(10, Items);
            Assert.Greater(game.Level.Seconds, 0);
            Assert.IsTrue(game.Tick(game.Level.Seconds + 1));
            Assert.AreEqual(TripleMatchState.Lost, game.State);
        }
    }
}

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
        public void UnmatchedFoodDoesNotAdvanceCompletion()
        {
            var game = new TripleMatchGame(1, Items);
            game.Select(0);
            Assert.AreEqual(0, game.Snapshot().ClearedTiles);
        }

        [Test]
        public void PauseFreezesTimerAndRejectsMovesAndBoosters()
        {
            var game = new TripleMatchGame(10, Items);
            int seconds = game.Snapshot().SecondsRemaining;
            game.SetPaused(true);
            Assert.IsFalse(game.Tick(500));
            Assert.IsFalse(game.Select(0));
            Assert.IsFalse(game.Hint());
            Assert.IsFalse(game.ShuffleActive());
            Assert.AreEqual(seconds, game.Snapshot().SecondsRemaining);
            Assert.AreEqual(3, game.HintRemaining);
            game.SetPaused(false);
            Assert.IsTrue(game.Select(0));
            Assert.IsTrue(game.Tick(1));
        }

        [Test]
        public void FinishedGamesCannotOpenAnUnresumablePause()
        {
            var lost = new TripleMatchGame(10, Items);
            lost.Tick(lost.Level.Seconds + 1);
            lost.SetPaused(true);
            Assert.IsFalse(lost.Snapshot().Paused);
            var won = new TripleMatchGame(1, Items);
            foreach (var group in won.Snapshot().Tiles.GroupBy(tile => tile.ItemId))
                foreach (var tile in group) won.Select(tile.Index);
            won.SetPaused(true);
            Assert.IsFalse(won.Snapshot().Paused);
        }

        [Test]
        public void CappedUndoHistoryKeepsTheMostRecentTurn()
        {
            var game = new TripleMatchGame(30, Items);
            var sequence = game.Snapshot().Tiles.GroupBy(tile => tile.ItemId).SelectMany(group => group).Take(15).ToArray();
            foreach (var tile in sequence) Assert.IsTrue(game.Select(tile.Index));
            Assert.IsTrue(game.Undo());
            Assert.IsTrue(game.Snapshot().Tiles[sequence[14].Index].Active);
            Assert.IsFalse(game.Snapshot().Tiles[sequence[13].Index].Active);
        }

        [Test]
        public void AllLevelsCanBeClearedByCompletingTriples()
        {
            for (int level = 1; level <= 100; level++)
            {
                var game = new TripleMatchGame(level, Items);
                foreach (var group in game.Snapshot().Tiles.GroupBy(tile => tile.ItemId))
                    foreach (var tile in group) Assert.IsTrue(game.Select(tile.Index));
                Assert.AreEqual(TripleMatchState.Won, game.State);
                Assert.AreEqual(game.Snapshot().TotalTiles, game.Snapshot().ClearedTiles);
                Assert.IsEmpty(game.Tray);
            }
        }

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

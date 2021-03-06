﻿using System;
using System.Linq;
using NUnit.Framework;
using OrigoDB.Core.Storage;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class SnapshotTests
    {
        [Test]
        public void Snapshots_are_numbered_correctly()
        {
            var config = EngineConfiguration.Create().ForImmutability().ForIsolatedTest();
            var engine = Engine.Create<ImmutableModel>(config);

            engine.Execute(new AppendNumberCommand(42));
            engine.Execute(new AppendNumberCommand(42));
            engine.Execute(new AppendNumberCommand(42));
            engine.Execute(new AppendNumberCommand(42));
            engine.CreateSnapshot();

            var store = config.CreateStore();


            Assert.AreEqual(0, store.Snapshots.First().LastEntryId);
            Assert.AreEqual(4, store.Snapshots.Skip(1).First().LastEntryId);
            Assert.AreEqual(2, store.Snapshots.Count());

        }

        [Test]
        public void Entry_id_is_extracted_from_snapshot_filename()
        {
            var dt = DateTime.Now;
            Snapshot ss = FileSnapshot.FromFileInfo("000467000.snapshot", dt);
            Assert.AreEqual(dt,ss.Created);
            Assert.AreEqual(467000, ss.LastEntryId);
        }
    }
}
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
	internal class CommandJournal : ICommandJournal
	{
        enum JournalState
        {
            Closed,
            Open
        }

        private IJournalWriter _writer;
		private IPersistentStorage _storage;
        private JournalState _state;
        private EngineConfiguration _config;


		public CommandJournal(EngineConfiguration config, IPersistentStorage storage)
		{
            _config = config;
			_storage = storage;
		}


        public void Rollover()
        {
            if (_state == JournalState.Closed) throw new InvalidOperationException("Cant advance journal when journal is closed");
            Log.Write("Journal file rollover");
            _writer.Dispose();

            Stream stream = _storage.CreateJournalWriterStream(JournalWriterCreateOptions.NextFragment);
            _writer = _config.CreateJournalWriter(stream);
        }

        public IEnumerable<JournalEntry<Command>> GetEntriesFrom(JournalFragmentInfo fragment)
        {
            JournalState preState = _state;
            SetState(JournalState.Closed);

            foreach (var journalEntry in _storage.GetJournalEntries(fragment))
            {
                yield return journalEntry;
            }

            SetState(preState);			
        }

		public IEnumerable<JournalEntry<Command>> GetAllEntries()
		{
            return GetEntriesFrom(JournalFragmentInfo.Initial);
		}


		public void Open()
		{

            if (_state == JournalState.Open)
            {
                throw new InvalidOperationException("Can't open command journal, already open");
            }
            
            Stream stream = _storage.CreateJournalWriterStream(JournalWriterCreateOptions.Append);
            _writer = _config.CreateJournalWriter(stream);
            _state = JournalState.Open;
		}

		public void Append(Command command)
		{
            if (_state != JournalState.Open)
            {
                throw new InvalidOperationException("Can't append to journal when closed");
            }

			var entry = new JournalEntry<Command>(command);
			_writer.Write(entry);
            if (_writer.Length >= _config.JournalFragmentSizeInBytes)
            {
                Rollover();
            }
		}

		public void Close()
		{

            if (_state == JournalState.Open)
            {
                _writer.Close();
                _writer.Dispose();
                _state = JournalState.Closed;
            }
		}


        private void SetState(JournalState state)
        {
            if (_state != state)
            {
                if (state == JournalState.Open) Open();
                else Close();
            }
        }
    }
}

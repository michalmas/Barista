using Barista.Foundation.EventSourcing.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Barista.Foundation.DataAccess;

namespace Barista.Foundation.EventSourcing.Persistence
{
    public class DataMapperPersistenceEngine : IPersistStreams
    {
        private static readonly DateTime EpochTime = new DateTime(1970, 1, 1);
        private readonly IDataMapper _dataMapper;
        private readonly IRepository<CommitRecord> _commitRecords;
        private readonly ISerialize _serializer;
        private bool _disposed;
        private int _initialized;
        private readonly IStreamIdHasher _streamIdHasher;

        public DataMapperPersistenceEngine(
            IDataMapper dataMapper,
            ISerialize serializer)
            : this(dataMapper, serializer, new Sha1StreamIdHasher())
        {}

        public DataMapperPersistenceEngine(
            IDataMapper dataMapper,
            ISerialize serializer,
            IStreamIdHasher streamIdHasher)
        {
            if (dataMapper == null)
            {
                throw new ArgumentNullException("dataMapper");
            }

            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            if (streamIdHasher == null)
            {
                throw new ArgumentNullException("streamIdHasher");
            }

            _dataMapper = dataMapper;
            _commitRecords = _dataMapper.GetRepository<CommitRecord>();
            _serializer = serializer;
            _streamIdHasher = new StreamIdHasherValidator(streamIdHasher);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Initialize()
        {
            if (Interlocked.Increment(ref _initialized) > 1)
            {
                return;
            }

            //ExecuteCommand(statement => statement.ExecuteWithoutExceptions(_dialect.InitializeStorage));
        }

        public virtual IEnumerable<ICommit> GetFrom(string streamId, int minRevision, int maxRevision)
        {
            streamId = _streamIdHasher.GetHash(streamId);
            
            var foundCommitRecords =
                _commitRecords.Where(
                    x =>
                        x.StreamId == streamId && x.StreamRevision >= minRevision &&
                        (x.StreamRevision - x.Items) < maxRevision && x.CommitSequence > 0)
                .ToArray();

            return foundCommitRecords.ToCommits(_serializer);
        }

        public virtual IEnumerable<ICommit> GetFrom(DateTime start)
        {
            start = start.AddTicks(-(start.Ticks%TimeSpan.TicksPerSecond)); // Rounds down to the nearest second.
            start = start < EpochTime ? EpochTime : start;

            var foundCommitRecords = _commitRecords
                .Where(x => x.CommitStamp > start)
                .OrderBy(x => x.CommitStamp)
                .ThenBy(x => x.StreamId)
                .ThenBy(x => x.CommitSequence)
                .ToArray();

            return foundCommitRecords.ToCommits(_serializer);
        }

        /*public ICheckpoint GetCheckpoint(string checkpointToken)
        {
            return string.IsNullOrWhiteSpace(checkpointToken) ? null : LongCheckpoint.Parse(checkpointToken);
        }*/

        public virtual IEnumerable<ICommit> GetFromTo(DateTime start, DateTime end)
        {
            start = start.AddTicks(-(start.Ticks%TimeSpan.TicksPerSecond)); // Rounds down to the nearest second.
            start = start < EpochTime ? EpochTime : start;
            end = end < EpochTime ? EpochTime : end;

            var foundCommitRecords = _commitRecords
                .Where(x => x.CommitStamp >= start && x.CommitStamp <= end)
                .OrderBy(x => x.CommitStamp)
                .ThenBy(x => x.StreamId)
                .ThenBy(x => x.CommitSequence)
                .ToArray();

            return foundCommitRecords.ToCommits(_serializer);
        }

        public virtual ICommit Commit(CommitAttempt attempt)
        {
            ICommit commit;
            /*try
            {*/
                commit = PersistCommit(attempt);
            /*}
            catch (Exception e)
            {
                if (!(e is UniqueKeyViolationException))
                {
                    throw;
                }

                if (DetectDuplicate(attempt))
                {
                    throw new DuplicateCommitException(e.Message, e);
                }

                Logger.Info(Messages.ConcurrentWriteDetected);
                throw new ConcurrencyException(e.Message, e);
            }*/
            return commit;
        }

        public virtual IEnumerable<ICommit> GetUndispatchedCommits()
        {
            return _commitRecords
                .Where(x => !x.IsDispatched)
                .OrderBy(x => x.CheckpointNumber)
                .ToArray()
                .ToCommits(_serializer);
        }

        public virtual void MarkCommitAsDispatched(ICommit commit)
        {
            string streamId = _streamIdHasher.GetHash(commit.StreamId);

            var commitRecord =
                _commitRecords.FirstOrDefault(
                    x => x.StreamId == streamId && x.CommitSequence == commit.CommitSequence);

            if (commitRecord != null)
            {
                commitRecord.IsDispatched = true;
                
                _dataMapper.SubmitChanges();
            }
        }

        /*public virtual IEnumerable<IStreamHead> GetStreamsToSnapshot(string bucketId, int maxThreshold)
        {
            Logger.Debug(Messages.GettingStreamsToSnapshot);
            return ExecuteQuery(query =>
                {
                    string statement = _dialect.GetStreamsRequiringSnapshots;
                    query.AddParameter(_dialect.BucketId, bucketId, DbType.AnsiString);
                    query.AddParameter(_dialect.Threshold, maxThreshold);
                    return
                        query.ExecutePagedQuery(statement,
                            (q, s) => q.SetParameter(_dialect.StreamId, _dialect.CoalesceParameterValue(s.StreamId()), DbType.AnsiString))
                            .Select(x => x.GetStreamToSnapshot());
                });
        }*/

        /*public virtual ISnapshot GetSnapshot(string bucketId, string streamId, int maxRevision)
        {
            Logger.Debug(Messages.GettingRevision, streamId, maxRevision);
            var streamIdHash = _streamIdHasher.GetHash(streamId);
            return ExecuteQuery(query =>
                {
                    string statement = _dialect.GetSnapshot;
                    query.AddParameter(_dialect.BucketId, bucketId, DbType.AnsiString);
                    query.AddParameter(_dialect.StreamId, streamIdHash, DbType.AnsiString);
                    query.AddParameter(_dialect.StreamRevision, maxRevision);
                    return query.ExecuteWithQuery(statement).Select(x => x.GetSnapshot(_serializer, streamId));
                }).FirstOrDefault();
        }

        public virtual bool AddSnapshot(ISnapshot snapshot)
        {
            Logger.Debug(Messages.AddingSnapshot, snapshot.StreamId, snapshot.StreamRevision);
            string streamId = _streamIdHasher.GetHash(snapshot.StreamId);
            return ExecuteCommand((connection, cmd) =>
                {
                    cmd.AddParameter(_dialect.BucketId, snapshot.BucketId, DbType.AnsiString);
                    cmd.AddParameter(_dialect.StreamId, streamId, DbType.AnsiString);
                    cmd.AddParameter(_dialect.StreamRevision, snapshot.StreamRevision);
                    _dialect.AddPayloadParamater(_connectionFactory, connection, cmd, _serializer.Serialize(snapshot.Payload));
                    return cmd.ExecuteWithoutExceptions(_dialect.AppendSnapshotToCommit);
                }) > 0;
        }*/

        /*public virtual void Purge()
        {
            Logger.Warn(Messages.PurgingStorage);
            ExecuteCommand(cmd => cmd.ExecuteNonQuery(_dialect.PurgeStorage));
        }

        public void Purge(string bucketId)
        {
            Logger.Warn(Messages.PurgingBucket, bucketId);
            ExecuteCommand(cmd =>
                {
                    cmd.AddParameter(_dialect.BucketId, bucketId, DbType.AnsiString);
                    return cmd.ExecuteNonQuery(_dialect.PurgeBucket);
                });
        }

        public void Drop()
        {
            Logger.Warn(Messages.DroppingTables);
            ExecuteCommand(cmd => cmd.ExecuteNonQuery(_dialect.Drop));
        }

        public void DeleteStream(string bucketId, string streamId)
        {
            Logger.Warn(Messages.DeletingStream, streamId, bucketId);
            streamId = _streamIdHasher.GetHash(streamId);
            ExecuteCommand(cmd =>
                {
                    cmd.AddParameter(_dialect.BucketId, bucketId, DbType.AnsiString);
                    cmd.AddParameter(_dialect.StreamId, streamId, DbType.AnsiString);
                    return cmd.ExecuteNonQuery(_dialect.DeleteStream);
                });
        }

        public IEnumerable<ICommit> GetFrom(string checkpointToken)
        {
            LongCheckpoint checkpoint = LongCheckpoint.Parse(checkpointToken);
            Logger.Debug(Messages.GettingAllCommitsFromCheckpoint, checkpointToken);
            return ExecuteQuery(query =>
            {
                string statement = _dialect.GetCommitsFromCheckpoint;
                query.AddParameter(_dialect.CheckpointNumber, checkpoint.LongValue);
                return query.ExecutePagedQuery(statement, (q, r) => { })
                    .Select(x => x.GetCommit(_serializer, _dialect));
            });
        }*/

        public bool IsDisposed
        {
            get { return _disposed; }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _disposed)
            {
                return;
            }

            _disposed = true;
        }

        private ICommit PersistCommit(CommitAttempt attempt)
        {
            /*Logger.Debug(Messages.AttemptingToCommit, attempt.Events.Count, attempt.StreamId, attempt.CommitSequence, attempt.BucketId);
            string streamId = _streamIdHasher.GetHash(attempt.StreamId);
            return ExecuteCommand((connection, cmd) =>
            {
                cmd.AddParameter(_dialect.BucketId, attempt.BucketId, DbType.AnsiString);
                cmd.AddParameter(_dialect.StreamId, streamId, DbType.AnsiString);
                cmd.AddParameter(_dialect.StreamIdOriginal, attempt.StreamId);
                cmd.AddParameter(_dialect.StreamRevision, attempt.StreamRevision);
                cmd.AddParameter(_dialect.Items, attempt.Events.Count);
                cmd.AddParameter(_dialect.CommitId, attempt.CommitId);
                cmd.AddParameter(_dialect.CommitSequence, attempt.CommitSequence);
                cmd.AddParameter(_dialect.CommitStamp, attempt.CommitStamp);
                cmd.AddParameter(_dialect.Headers, _serializer.Serialize(attempt.Headers));
                _dialect.AddPayloadParamater(_connectionFactory, connection, cmd, _serializer.Serialize(attempt.Events.ToList()));
                
                var checkpointNumber = cmd.ExecuteScalar(_dialect.PersistCommit).ToLong();
                return new Commit(
                    attempt.BucketId,
                    attempt.StreamId,
                    attempt.StreamRevision,
                    attempt.CommitId,
                    attempt.CommitSequence,
                    attempt.CommitStamp,
                    checkpointNumber.ToString(CultureInfo.InvariantCulture),
                    attempt.Headers,
                    attempt.Events);
            });*/

            string streamId = _streamIdHasher.GetHash(attempt.StreamId);

            var commit = new CommitRecord
            {
                StreamId = streamId,
                StreamIdOriginal = attempt.StreamId,
                StreamRevision = attempt.StreamRevision,
                Items = attempt.Events.Count,
                CommitId = attempt.CommitId,
                CommitSequence = attempt.CommitSequence,
                CommitStamp = attempt.CommitStamp,
                Headers = _serializer.Serialize(attempt.Headers),
                Payload = _serializer.Serialize(attempt.Events.ToList())
            };

            _dataMapper.Add(commit);
            _dataMapper.SubmitChanges();

            return new Commit(
                    attempt.StreamId,
                    attempt.StreamRevision,
                    attempt.CommitId,
                    attempt.CommitSequence,
                    attempt.CommitStamp,
                    commit.CheckpointNumber.ToString(),
                    attempt.Headers,
                    attempt.Events);
        }

        private bool DetectDuplicate(CommitAttempt attempt)
        {
            string streamId = _streamIdHasher.GetHash(attempt.StreamId);

            return
                _commitRecords.Where(
                    x =>
                        x.StreamId == streamId && x.CommitSequence == attempt.CommitSequence &&
                        x.CommitId == attempt.CommitId).Select(x => true).FirstOrDefault();
        }

        /*protected virtual IEnumerable<T> ExecuteQuery<T>(Func<IDbStatement, IEnumerable<T>> query)
        {
            ThrowWhenDisposed();

            TransactionScope scope = OpenQueryScope();
            IDbConnection connection = null;
            IDbTransaction transaction = null;
            IDbStatement statement = null;

            try
            {
                connection = _connectionFactory.Open();
                transaction = _dialect.OpenTransaction(connection);
                statement = _dialect.BuildStatement(scope, connection, transaction);
                statement.PageSize = _pageSize;

                Logger.Verbose(Messages.ExecutingQuery);
                return query(statement);
            }
            catch (Exception e)
            {
                if (statement != null)
                {
                    statement.Dispose();
                }
                if (transaction != null)
                {
                    transaction.Dispose();
                }
                if (connection != null)
                {
                    connection.Dispose();
                }
                if (scope != null)
                {
                    scope.Dispose();
                }

                Logger.Debug(Messages.StorageThrewException, e.GetType());
                if (e is StorageUnavailableException)
                {
                    throw;
                }

                throw new StorageException(e.Message, e);
            }
        }

        protected virtual TransactionScope OpenQueryScope()
        {
            return OpenCommandScope() ?? new TransactionScope(TransactionScopeOption.Suppress);
        }

        private void ThrowWhenDisposed()
        {
            if (!_disposed)
            {
                return;
            }

            Logger.Warn(Messages.AlreadyDisposed);
            throw new ObjectDisposedException(Messages.AlreadyDisposed);
        }

        private T ExecuteCommand<T>(Func<IDbStatement, T> command)
        {
            return ExecuteCommand((_, statement) => command(statement));
        }

        protected virtual T ExecuteCommand<T>(Func<IDbConnection, IDbStatement, T> command)
        {
            ThrowWhenDisposed();

            using (TransactionScope scope = OpenCommandScope())
            using (IDbConnection connection = _connectionFactory.Open())
            using (IDbTransaction transaction = _dialect.OpenTransaction(connection))
            using (IDbStatement statement = _dialect.BuildStatement(scope, connection, transaction))
            {
                try
                {
                    Logger.Verbose(Messages.ExecutingCommand);
                    T rowsAffected = command(connection, statement);
                    Logger.Verbose(Messages.CommandExecuted, rowsAffected);

                    if (transaction != null)
                    {
                        transaction.Commit();
                    }

                    if (scope != null)
                    {
                        scope.Complete();
                    }

                    return rowsAffected;
                }
                catch (Exception e)
                {
                    Logger.Debug(Messages.StorageThrewException, e.GetType());
                    if (!RecoverableException(e))
                    {
                        throw new StorageException(e.Message, e);
                    }

                    Logger.Info(Messages.RecoverableExceptionCompletesScope);
                    if (scope != null)
                    {
                        scope.Complete();
                    }

                    throw;
                }
            }
        }

        protected virtual TransactionScope OpenCommandScope()
        {
            return new TransactionScope(_scopeOption);
        }

        private static bool RecoverableException(Exception e)
        {
            return e is UniqueKeyViolationException || e is StorageUnavailableException;
        }*/

        private class StreamIdHasherValidator : IStreamIdHasher
        {
            private readonly IStreamIdHasher _streamIdHasher;
            private const int MaxStreamIdHashLength = 40;

            public StreamIdHasherValidator(IStreamIdHasher streamIdHasher)
            {
                if (streamIdHasher == null)
                {
                    throw new ArgumentNullException("streamIdHasher");
                }
                _streamIdHasher = streamIdHasher;
            }
            public string GetHash(string streamId)
            {
                if (string.IsNullOrWhiteSpace(streamId))
                {
                    throw new ArgumentException("StreamIdIsNullEmptyOrWhiteSpace");
                }
                string streamIdHash = _streamIdHasher.GetHash(streamId);
                if (string.IsNullOrWhiteSpace(streamIdHash))
                {
                    throw new InvalidOperationException("StreamIdHashIsNullEmptyOrWhiteSpace");
                }
                if (streamIdHash.Length > MaxStreamIdHashLength)
                {
                    throw new InvalidOperationException("StreamIdHashTooLong.FormatWith(streamId, streamIdHash, streamIdHash.Length, MaxStreamIdHashLength)");
                }
                return streamIdHash;
            }
        }

        /*private readonly IDataMapper _dataMapper;
        private readonly ISerialize _serializer;
        private readonly IStreamIdHasher _streamIdHasher;

        public DataMapperPersistenceEngine(IDataMapper dataMapper, ISerialize serializer, IStreamIdHasher streamIdHasher)
        {
            _dataMapper = dataMapper;
            _serializer = serializer;
            _streamIdHasher = streamIdHasher;
        }

        public bool IsDisposed
        {
            get { throw new NotImplementedException(); }
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ICommit> GetFrom(DateTime start)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ICommit> GetFrom(string checkpointToken = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ICommit> GetFromTo(DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ICommit> GetUndispatchedCommits()
        {
            throw new NotImplementedException();
        }

        public void MarkCommitAsDispatched(ICommit commit)
        {
            throw new NotImplementedException();
        }

        public void Purge()
        {
            throw new NotImplementedException();
        }

        public void Drop()
        {
            throw new NotImplementedException();
        }

        public void DeleteStream(string streamId)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ICommit> GetFrom(string streamId, int minRevision, int maxRevision)
        {
            streamId = _streamIdHasher.GetHash(streamId);

            var commitRecords = _dataMapper.GetRepository<CommitRecord>();

            var foundCommitRecords =
                commitRecords.Where(
                    x =>
                        x.StreamId == streamId && x.StreamRevision > minRevision &&
                        (x.StreamRevision - x.Items) < maxRevision && x.CommitSequence > 0)
                .ToArray();

            return foundCommitRecords.ToCommits(_serializer);
        }

        public ICommit Commit(CommitAttempt attempt)
        {
            ICommit commit;
            /*try
            {*/
                //commit = PersistCommit(attempt);
            /*}
            catch (Exception e)
            {
                if (!(e is UniqueKeyViolationException))
                {
                    throw;
                }

                if (DetectDuplicate(attempt))
                {
                    throw new DuplicateCommitException(e.Message, e);
                }

                throw new ConcurrencyException(e.Message, e);
            }*/
            /*return commit;
        }

        private ICommit PersistCommit(CommitAttempt attempt)
        {
            string streamId = _streamIdHasher.GetHash(attempt.StreamId);

            var commit = new CommitRecord
            {
                StreamId = streamId,
                StreamIdOriginal = attempt.StreamId,
                StreamRevision = attempt.StreamRevision,
                Items = attempt.Events.Count,
                CommitId = attempt.CommitId,
                CommitSequence = attempt.CommitSequence,
                CommitStamp = attempt.CommitStamp,
                Headers = _serializer.Serialize(attempt.Headers),
                Payload = _serializer.Serialize(attempt.Events.ToList())
            };

            _dataMapper.Add(commit);
            _dataMapper.SubmitChanges();

            return new Commit(
                    attempt.StreamId,
                    attempt.StreamRevision,
                    attempt.CommitId,
                    attempt.CommitSequence,
                    attempt.CommitStamp,
                    commit.CheckpointNumber.ToString(),
                    attempt.Headers,
                    attempt.Events);

            /*string streamId = _streamIdHasher.GetHash(attempt.StreamId);
            return ExecuteCommand((connection, cmd) =>
            {
                cmd.AddParameter(_dialect.BucketId, attempt.BucketId, DbType.AnsiString);
                cmd.AddParameter(_dialect.StreamId, streamId, DbType.AnsiString);
                cmd.AddParameter(_dialect.StreamIdOriginal, attempt.StreamId);
                cmd.AddParameter(_dialect.StreamRevision, attempt.StreamRevision);
                cmd.AddParameter(_dialect.Items, attempt.Events.Count);
                cmd.AddParameter(_dialect.CommitId, attempt.CommitId);
                cmd.AddParameter(_dialect.CommitSequence, attempt.CommitSequence);
                cmd.AddParameter(_dialect.CommitStamp, attempt.CommitStamp);
                cmd.AddParameter(_dialect.Headers, _serializer.Serialize(attempt.Headers));
                _dialect.AddPayloadParamater(_connectionFactory, connection, cmd, _serializer.Serialize(attempt.Events.ToList()));
                OnPersistCommit(cmd, attempt);
                var checkpointNumber = cmd.ExecuteScalar(_dialect.PersistCommit).ToLong();
                return new Commit(
                    attempt.BucketId,
                    attempt.StreamId,
                    attempt.StreamRevision,
                    attempt.CommitId,
                    attempt.CommitSequence,
                    attempt.CommitStamp,
                    checkpointNumber.ToString(CultureInfo.InvariantCulture),
                    attempt.Headers,
                    attempt.Events);
            });*/
        /*}*/
    }
}

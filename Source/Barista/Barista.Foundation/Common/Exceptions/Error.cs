using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Barista.Foundation.Common.Exceptions
{
    [Serializable]
    public class Error
    {
        public static Error Internal = new Error("InternalError");
        public static Error AttemptToRetrieveRecordWithEmptyKey = new Error("AttemptToRetrieveRecordWithEmptyKey");
        public static Error RecordNotFound = new Error("RecordNotFound");
        public static Error RecordIsChangedByAnotherUser = new Error("RecordIsChangedByAnotherUser");
        public static Error RecordIsDeletedByAnotherUser = new Error("RecordIsDeletedByAnotherUser");
        public static Error AspNetRequestValidationFailed = new Error("AspNetRequestValidationFailed");
        public static Error CertificateReportProfileNotFound = new Error("CertificateReportProfileNotFound");
        public static Error DataExceedsMaximumLength = new Error("DataExceedsMaximumLength");
        public static Error RecordWithDuplicateUniqueKeyExists = new Error("RecordWithDuplicateUniqueKeyExists");
        public static Error RecordIsUsedByAnotherRecord = new Error("RecordIsUsedByAnotherRecord");
        public static Error ConnectionToDatabaseFailed = new Error("ConnectionToDatabaseFailed");
        public static Error DatabaseTableNotFound = new Error("DatabaseTableNotFound");
        public static Error MasterNotAvailable = new Error("MasterNotAvailable");
        public static Error CriticalSynchronizationError = new Error("CriticalSynchronizationError");
        public static Error UnkownSlave = new Error("UnkownSlave");
        public static Error ProductVersionsMismatch = new Error("ProductVersionsMismatch");
        public static Error SlaveHasSameDeploymentCodeAsMaster = new Error("SlaveHasSameDeploymentCodeAsMaster");
        public static Error CoordinatesOutOfBounds = new Error("CoordinatesOutOfBounds");
        public static Error InvalidCredentials = new Error("InvalidCredentials");
        public static Error ClientNotSynchronizedForTooLong = new Error("ClientNotSynchronizedForTooLong");
        public static Error SynchronizationShouldBeConfigured = new Error("SynchronizationShouldBeConfigured");

        public string Code { get; set; }

        /*protected Error(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }*/

        protected Error(string code)
            /*: base(code)*/
        {
        }
    }
}

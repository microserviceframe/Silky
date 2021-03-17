using System.Diagnostics;
using System.Threading.Tasks;
using Lms.Core.DynamicProxy;
using Lms.Core.Extensions;
using Lms.Core.Utils;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Transport;
using Lms.Transaction.Participant;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lms.Transaction.Tcc.Executor
{
    public sealed class TccTransactionExecutor
    {
        private static TccTransactionExecutor executor = new();
        public ILogger<TccTransactionExecutor> Logger { get; set; }

        private TccTransactionExecutor()
        {
            Logger = NullLogger<TccTransactionExecutor>.Instance;
        }

        public static TccTransactionExecutor Executor => executor;


        public ITransaction PreTry(ILmsMethodInvocation invocation)
        {
            Logger.LogDebug("tcc transaction starter");
            var transaction = CreateTransaction();
            var participant = BuildParticipant(invocation,
                null,
                null,
                TransactionRole.Start,
                ParticipantType.Local,
                transaction.TransId);
            transaction.RegisterParticipant(participant);
            LmsTransactionHolder.Instance.Set(transaction);
            var context = new TransactionContext
            {
                Action = ActionStage.Trying,
                TransId = transaction.TransId,
                TransactionRole = TransactionRole.Start,
                TransType = TransactionType.Tcc
            };
            RpcContext.GetContext().SetTransactionContext(context);
            return transaction;
        }

        public IParticipant PreTryParticipant(TransactionContext context, ILmsMethodInvocation invocation)
        {
            Logger.LogDebug($"participant tcc transaction start..：{context}");
            var serviceEntry = invocation.ArgumentsDictionary["serviceEntry"] as ServiceEntry;
            Debug.Assert(serviceEntry != null);
            if (serviceEntry.IsTransactionServiceEntry())
            {
                var participantType = serviceEntry.IsLocal ? ParticipantType.Local : ParticipantType.Inline;
                IParticipant participant = BuildParticipant(invocation,
                    null,
                    context.ParticipantId,
                    TransactionRole.Participant,
                    participantType,
                    context.TransId);
                if (LmsTransactionHolder.Instance.CurrentTransaction != null)
                {
                    LmsTransactionHolder.Instance.CurrentTransaction.RegisterParticipant(participant);
                }
                else
                {
                    var transaction = CreateTransaction(context.TransId);
                    participant.Role = TransactionRole.Consumer;
                    transaction.RegisterParticipant(participant);
                    context.TransactionRole = TransactionRole.Consumer;
                    LmsTransactionHolder.Instance.Set(transaction);
                }

                return participant;
            }

            return null;
        }

        public void UpdateStartStatus(ITransaction transaction)
        {
            foreach (var participant in transaction.Participants)
            {
                participant.Status = transaction.Status;
            }
        }

        public async Task GlobalConfirm(ITransaction transaction)
        {
            foreach (var participant in transaction.Participants)
            {
                await participant.ParticipantConfirm();
                participant.Status = ActionStage.Confirming;
            }

            transaction.Status = ActionStage.Confirming;
        }

        public async Task GlobalCancel(ITransaction transaction)
        {
            foreach (var participant in transaction.Participants)
            {
                if (participant.Status == ActionStage.Trying)
                {
                    await participant.ParticipantCancel();
                    participant.Status = ActionStage.Canceling;
                }

                transaction.Status = ActionStage.Canceling;
            }
        }

        private IParticipant BuildParticipant(ILmsMethodInvocation invocation,
            string participantId,
            string participantRefId,
            TransactionRole transactionRole,
            ParticipantType participantType,
            string transId)
        {
            var participant = new LmsParticipant()
            {
                Role = transactionRole,
                TransId = transId,
                TransType = TransactionType.Tcc,
                ParticipantType = participantType,
                Invocation = invocation,
            };
            if (participantId.IsNullOrEmpty())
            {
                participant.ParticipantId = GuidGenerator.CreateGuidStrWithNoUnderline();
            }
            else
            {
                participant.ParticipantId = participantId;
            }

            if (!participantRefId.IsNullOrEmpty())
            {
                participant.ParticipantRefId = participantRefId;
            }

            return participant;
        }

        private ITransaction CreateTransaction()
        {
            var transaction = new LmsTransaction(GuidGenerator.CreateGuidStrWithNoUnderline());
            transaction.Status = ActionStage.PreTry;
            transaction.TransType = TransactionType.Tcc;
            return transaction;
        }

        private ITransaction CreateTransaction(string transId)
        {
            var transaction = new LmsTransaction(transId);
            transaction.Status = ActionStage.PreTry;
            transaction.TransType = TransactionType.Tcc;
            return transaction;
        }


        public async Task<object> ConsumerParticipantExecute(TransactionContext context,
            ILmsMethodInvocation invocation, TccMethodType tccMethodType)
        {
            var serviceEntry = invocation.ArgumentsDictionary["serviceEntry"] as ServiceEntry;
            Debug.Assert(serviceEntry != null);
            var serviceKey = invocation.ArgumentsDictionary["serviceKey"] as string;
            var parameters = invocation.ArgumentsDictionary["parameters"] as object[];
            PreTryParticipant(context, invocation);
            var excutorInfo = serviceEntry.GetTccExcutorInfo(serviceKey, tccMethodType);

            if (excutorInfo.Item2)
            {
                return await excutorInfo.Item1.ExecuteAsync(excutorInfo.Item3, parameters);
            }
            else
            {
                return excutorInfo.Item1.Execute(excutorInfo.Item3, parameters);
            }
        }
    }
}
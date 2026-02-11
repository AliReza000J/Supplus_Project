using _0_Framework.Application;
using AccountManagement.Domain.AccountAgg;
using SiteManagement.Application.Contracts.Ticket;
using SiteManagement.Domain.TicketAgg;

namespace SiteManagement.Application
{
    public class TicketApplication : ITicketApplication
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITicketRepository _ticketRepository;

        public TicketApplication(IAccountRepository accountRepository, ITicketRepository ticketRepository)
        {
            _accountRepository = accountRepository;
            _ticketRepository = ticketRepository;
        }

        public async Task<(string, OperationResult)> CreateTicket(CreateTicket command, CancellationToken cancellation)
        {
            var op = new OperationResult();

            if (command == null)
                return (string.Empty, op.Failed("دیتای مربوط به تیکت خالی است."));

            var acc = await _accountRepository.GetAsync(command.UserId, cancellation);
            if (acc == null)
                return (string.Empty ,op.Failed($"کاربر با آیدی {command.UserId} یافت نشد."));

            var trackingCode = GuidShortener.ToBase62(Guid.NewGuid());
            var ticket = new Ticket(command.UserId, command.Email, command.Subject, command.Message, trackingCode);

            _ticketRepository.Create(ticket);
            await _ticketRepository.SaveChangesAsync(cancellation);
            return (ticket.TrackingCode, op.Succeeded("تیکت شما با موفقیت ارسال شد."));
        }

        public async Task<ReplyTicket?> GetDetailsAsync(long id, CancellationToken cancellation)
        {
            return await _ticketRepository.GetTicketDetails(id, cancellation);
        }

        public async Task<OperationResult> ReplyTicket(ReplyTicket command, CancellationToken cancellation)
        {
            var op = new OperationResult();

            var acc = await _accountRepository.GetAsync(command.AdminId, cancellation);
            if (acc == null)
                return op.Failed($"ادمین با آیدی {command.AdminId} یافت نشد.");

            var ticket = await _ticketRepository.GetAsync(command.TicketId, cancellation);
            if (ticket == null || ticket.Id <= 0)
                return op.Failed(ApplicationMessages.RecordNotFound);

            ticket.Reply(command.ReplyMessage, command.AdminId);
            await _ticketRepository.SaveChangesAsync(cancellation);
            return op.Succeeded($"پاسخ شما برای تیکت #{ticket.Id} ثبت شد.");
        }

        public async Task<List<TicketViewModel>> Search(TicketSearchModel searchModel, CancellationToken cancellation)
        {
            return await _ticketRepository.Search(searchModel, cancellation);
        }
    }
}

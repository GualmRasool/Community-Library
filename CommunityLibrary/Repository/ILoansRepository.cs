using CommunityLibrary.Models.DTOs;
using CommunityLibrary.Models.Entities;
using CommunityLibrary.Models.Request;

namespace CommunityLibrary.Repository
{
    public interface ILoansRepository
    {
        public Task<LoanResponse> LoanbyID(Guid loanId, CancellationToken ct);

        public Task<LoansListResponse> allLoan(string status, int limit, int offset, CancellationToken ct);

        public Task<LoanResponse> addLoan(BorrowRequest request, Guid bookId);

        public Task<LoanResponse> updateLoan(ReturnRequest request, Guid loanID, CancellationToken ct);

    }
}

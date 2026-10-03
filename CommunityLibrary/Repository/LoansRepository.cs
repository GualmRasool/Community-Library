using CommunityLibrary.Api.Exceptions;
using CommunityLibrary.Models.DTOs;
using CommunityLibrary.Models.Entities;
using CommunityLibrary.Models.Request;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;

namespace CommunityLibrary.Repository
{
    
    public class LoansRepository : ILoansRepository
    {
        private readonly LibraryDbContext _db;
        private readonly TimeProvider _timeProvider;

        public LoansRepository(LibraryDbContext db, TimeProvider timeProvider)
        {
            _db = db;
            _timeProvider = timeProvider;
        }

        //public List<Loans> Loans { get; set; } = new List<Loans>()
        //{

        //};
        public async Task<LoanResponse> LoanbyID(Guid loanId, CancellationToken ct)
        {
            var loan = await _db.Loan.Include(l => l.Book).AsNoTracking().FirstOrDefaultAsync(l => l.Id == loanId, ct);


            if (loan is null)
            {
                throw new NotFoundApiException("BOOK_NOT_FOUND", $"No book found with id '{loanId}'.");
            }

            loan.RefreshComputedStatus(DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime));
            return new LoanResponse { Data = MapToDto(loan) };
        }
        public async Task<LoansListResponse> allLoan(string status, int limit, int offset, CancellationToken ct)
        {
            var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

            var loans = await _db.Loan
            .Include(l => l.Book)
            .AsNoTracking()
            .ToListAsync(ct);

            foreach (var loan in loans)
            {
                loan.RefreshComputedStatus(today);
            }

            var filtered = loans;

            if (status.ToLower() == "active")
            {
                filtered = loans.Where(l => l.Status == LoanStatus.Active).ToList();
            }
            else if (status.ToLower() == "overdue")
            {
                filtered = loans.Where(l => l.Status == LoanStatus.Overdue).ToList();
            }

            var ordered = filtered.OrderByDescending(l => l.BorrowedAt).ToList();
            var total = ordered.Count;
            var page = ordered.Skip(offset).Take(limit).ToList();

            return new LoansListResponse
            {
                Data = page.Select(MapToDto).ToList(),
                Pagination = new PaginationInfo { Limit = limit, Offset = offset, Total = total }
            };


        }
        public async Task<LoanResponse> addLoan(BorrowRequest request, Guid bookId)
        {
            var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == bookId);

            if (book is null)
            {
                throw new NotFoundApiException("BOOK_NOT_FOUND", $"No book found with id '{bookId}'. change it ");
            }

            if (book.Status != BookStatus.Available)
            {
                throw new ConflictApiException("BOOK_NOT_AVAILABLE", "The requested book is not available for borrowing.");
            }

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var loan = new Loans
            {
                Id = Guid.NewGuid(),
                BookId = book.Id,
                MemberId = request.MemberID,
                BorrowedAt = now,
                DueDate = DateOnly.FromDateTime(now.AddDays(30)),
                Status = LoanStatus.Active
            };

            book.Status = BookStatus.Borrowed;

            _db.Loan.Add(loan);
            await _db.SaveChangesAsync();

            loan.Book = book;
            return new LoanResponse { Data = MapToDto(loan) };
        }
        public async Task<LoanResponse> updateLoan(ReturnRequest request, Guid loanID, CancellationToken ct)
        {
            var loan = await _db.Loan.Include(l => l.Book).FirstOrDefaultAsync(l => l.Id == loanID, ct);
            if (loan is null)
            {
                throw new NotFoundApiException("LOAN_NOT_FOUND", $"No loan found with id '{loanID}'.");
            }

            if (loan.Status == LoanStatus.Returned)
            {
                throw new ConflictApiException("LOAN_ALREADY_RETURNED", "This loan has already been marked as returned.");
            }

            loan.Status = LoanStatus.Returned;
            loan.ReturnedAt = request.returnedAt.ToUniversalTime();
            loan.Notes = request.Notes;

            if (loan.Book is not null)
            {
                loan.Book.Status = BookStatus.Available;
            }

            await _db.SaveChangesAsync(ct);

            return new LoanResponse { Data = MapToDto(loan) };
        }
        private static LoansDTO MapToDto(Loans l) => new()
        {
            Id = l.Id,
            BookId = l.BookId,
            MemberId = l.MemberId,
            BorrowedAt = l.BorrowedAt,
            DueDate = l.DueDate,
            ReturnedAt = l.ReturnedAt,
            Status = l.Status,
            Book = l.Book is null ? null : BooksRepository.MapToDto(l.Book)
        };
    }
}

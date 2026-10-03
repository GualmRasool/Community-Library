using CommunityLibrary.Models.DTOs;
using CommunityLibrary.Models.Entities;
using CommunityLibrary.Models.Request;
using CommunityLibrary.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CommunityLibrary.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class LoansController : ControllerBase
    {
        private readonly ILoansRepository _loansrepository;
        private readonly IBooksRepository _booksrepository;

        public LoansController(ILoansRepository loansrepository, IBooksRepository booksrepository)
        {
            _loansrepository = loansrepository;
            _booksrepository = booksrepository;
        }

        [HttpPost("books/{bookId:guid}/Loan")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoanResponse>> BarrowBook([FromBody] BorrowRequest request, Guid bookId)
        {
            //var books = _booksrepository.getBooksbyID(bookId).FirstOrDefault();

            if (request == null)
            {
                return BadRequest("Invalid Data");
            }

            var result = await _loansrepository.addLoan(request, bookId);
            return CreatedAtAction(nameof(loanbyid), new { loanId = result.Data.Id }, result);
        }

        [HttpGet]
        [Route("Loans/{loanId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> loanbyid(Guid loanId, CancellationToken ct = default)
        {
            var loan = await _loansrepository.LoanbyID(loanId, ct);
            if (loan == null)
            {
                return NotFound($"Loan Not Found for {loanId}");
            }
            return Ok(loan);
        }

        [HttpGet]
        [Route("Loans")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> loan(
            [FromQuery] string status = "All", 
            [FromQuery]  int limit = 10, 
            [FromQuery]  int offset = 0,
            CancellationToken ct = default)
        {
            var validStatuses = new[] { "active", "overdue", "All" };
            if (!validStatuses.Contains(status))
            {
                return BadRequest("Invalid Status");
            }
            if (limit < 1 || limit > 10)
            {
                return BadRequest("Invalid limit");
            }
            if (offset < 0)
            {
                return BadRequest("Invalid offset");
            }
            var loan = await _loansrepository.allLoan(status, limit, offset, ct);
            return Ok(loan);
        }
    }
}
    
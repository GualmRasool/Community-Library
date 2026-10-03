using CommunityLibrary.Models.Entities;
using CommunityLibrary.Models.Request;
using CommunityLibrary.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using static System.Reflection.Metadata.BlobBuilder;

namespace CommunityLibrary.Controllers
{
    [Route("/loans")]
    [ApiController]
    public class ReturnsController : ControllerBase
    {
        private readonly ILoansRepository _loansRespository;
        private readonly IBooksRepository _booksRespository;

        public ReturnsController(ILoansRepository loansRepository, IBooksRepository booksRespository)
        {
            _loansRespository = loansRepository;
            _booksRespository = booksRespository;
        }

        [HttpPost]
        [Route("{loanId:guid}/return")]
        public async Task<ActionResult> returnLoans([FromBody] ReturnRequest request, Guid loanId, CancellationToken ct)
        {
            var loans = await _loansRespository.updateLoan(request, loanId, ct);

            return Ok(loans);
        }
    }
}

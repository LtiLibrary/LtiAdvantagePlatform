using System;
using System.Threading.Tasks;
using LtiAdvantage.AssignmentGradeServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AdvantagePlatform.Controllers
{
    public class LineItemsController : LineItemsControllerBase
    {
        public LineItemsController(ILogger<LineItemsControllerBase> logger) : base(logger)
        {
        }

        protected override Task<LineItemResult> OnCreateLineItemAsync(PostLineItemRequest request)
        {
            throw new NotImplementedException();
        }

        protected override Task<IActionResult> OnDeleteLineItemAsync(DeleteLineItemRequest request)
        {
            throw new NotImplementedException();
        }

        protected override Task<LineItemResult> OnGetLineItemAsync(GetLineItemRequest request)
        {
            var result = new LineItemResult(new LineItem());
            return Task.FromResult(result);
        }

        protected override Task<IActionResult> OnUpdateLineItemAsync(PutLineItemRequest request)
        {
            throw new NotImplementedException();
        }

        protected override Task<LineItemContainerResult> OnGetLineItemsAsync(GetLineItemsRequest request)
        {
            var lineitems = new LineItemContainer
            {
                new LineItem()
            };
            var result = new LineItemContainerResult(lineitems);
            return Task.FromResult(result);
        }
    }
}

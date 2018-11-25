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

        protected override async Task<LineItemResult> OnCreateLineItemAsync(PostLineItemRequest request)
        {
            var lineItem = new LineItem();
            lineItem.Id = lineItem.GetHashCode().ToString();
            return Created(lineItem);
        }

        protected override async Task<IActionResult> OnDeleteLineItemAsync(DeleteLineItemRequest request)
        {
            return Ok();
        }

        protected override async Task<LineItemResult> OnGetLineItemAsync(GetLineItemRequest request)
        {
            var lineItem = new LineItem();
            lineItem.Id = lineItem.GetHashCode().ToString();
            return Ok(lineItem);
        }

        protected override async Task<IActionResult> OnUpdateLineItemAsync(PutLineItemRequest request)
        {
            return Ok();
        }

        protected override async Task<LineItemContainerResult> OnGetLineItemsAsync(GetLineItemsRequest request)
        {
            var lineItem = new LineItem();
            lineItem.Id = lineItem.GetHashCode().ToString();
            var lineitems = new LineItemContainer
            {
                lineItem
            };
            return Ok(lineitems);
        }
    }
}

#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.Services
{
    public class PromotionOrchestratorService
    {
        private readonly PromotionEvaluationService _evaluationService;
        private readonly CampaignSelectionService _selectionService;

        public PromotionOrchestratorService(PromotionEvaluationService evaluationService, CampaignSelectionService selectionService)
        {
            _evaluationService = evaluationService;
            _selectionService = selectionService;
        }

        public PromotionResult OrchestratePromotionFlow(EligibilityContext context)
        {
            return _evaluationService.EvaluateEligibility(context);
        }
    }
}

#endif
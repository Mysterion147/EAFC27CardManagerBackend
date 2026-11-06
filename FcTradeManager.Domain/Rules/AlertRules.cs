namespace FcTradeManager.Domain.Rules;

public readonly record struct AlertEvaluation(
    bool FireBuy,
    bool FireSell,
    bool BuyTriggered,
    bool SellTriggered);

public static class AlertRules
{
    // RN06 + RN07
    // buy:  price <= buyTarget
    // sell: price >= sellTarget
    // "triggered" state is simply whether the condition holds now, so it re-arms when the price crosses back
    public static AlertEvaluation Evaluate(
        int price,
        int buyTarget,
        int sellTarget,
        bool buyAlreadyTriggered,
        bool sellAlreadyTriggered)
    {
        var buyHit = price <= buyTarget;
        var sellHit = price >= sellTarget;

        return new AlertEvaluation(
            FireBuy: buyHit && !buyAlreadyTriggered,
            FireSell: sellHit && !sellAlreadyTriggered,
            BuyTriggered: buyHit,
            SellTriggered: sellHit);
    }
}

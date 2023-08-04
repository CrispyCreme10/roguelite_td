namespace DragDrop {
    public class TargetDropdownResult {
        public bool ItemAccepted { get; private set; }
        public int RemainingAmount { get; private set; }

        public TargetDropdownResult(bool itemAccepted, int remainingAmount) {
            ItemAccepted = itemAccepted;
            RemainingAmount = remainingAmount;
        }

        public override string ToString() {
            return $"ItemAccepted: {ItemAccepted}, RemainingAmount: {RemainingAmount}";
        }
    }
}
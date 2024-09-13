namespace BlazesoftMachine.Model.Requests
{
    public class SpinResponse
    {
        public int[,] Matrix { get; set; }
        public decimal WinAmount { get; set; }
        public decimal PlayerBalance { get; set; }
    }
}

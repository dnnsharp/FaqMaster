namespace DnnSharp.FaqMaster.Core.DnnSf.ExpressionEvaluator.Tokens
{
    internal class MemberToken : OpToken
    {
        public string Name { get; set; }

        public MemberToken()
        {
            Value = ".";
        }
    }
}
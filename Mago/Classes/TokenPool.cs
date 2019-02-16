using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mago
{
    public class TokenPool
    {
        int _capacity;
        List<CancellationTokenSource> tokens;

        public TokenPool(int capacity)
        {
            _capacity = capacity;
            tokens = new List<CancellationTokenSource>(_capacity);
        }

        public CancellationToken getToken()
        {
            CancellationRequest();
            if (tokens.Count >= tokens.Capacity) tokens.RemoveAt(0);

            tokens.Add(new CancellationTokenSource());
                return tokens.Last().Token;
        }

        public void CancellationRequest()
        {
            if(tokens.Count > 0)
                tokens.Last().Cancel();
        }

        public CancellationToken LastToken()
        {
            
            return tokens[tokens.Count - 2].Token;
        }

    }
}

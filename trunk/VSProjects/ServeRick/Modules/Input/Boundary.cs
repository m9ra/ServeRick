using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Modules.Input
{
    public class Boundary
    {
        /// <summary>
        /// Prefix needed for boundary (except first one - that doesn't have preceding newline)
        /// </summary>
        private static readonly byte[] BoundaryPrefix = Encoding.ASCII.GetBytes("\r\n--");

        /// <summary>
        /// raw data of boundary string
        /// </summary>
        internal readonly byte[] DelimiterData;

        /// <summary>
        /// Current cursor position in boundary
        /// </summary>
        internal int Cursor { get; private set; }

        /// <summary>
        /// Start offset derived from local end offset and data length
        /// </summary>
        public int LocalStartOffset { get { return LocalEndOffset - DelimiterData.Length + 1; } }

        /// <summary>
        /// End offset according to accepted data, when boundary end has been found
        /// </summary>
        public int LocalEndOffset { get; private set; }

        /// <summary>
        /// Determine that boundary has been recognized in accepted data
        /// </summary>
        public bool IsComplete { get { return Cursor >= DelimiterData.Length; } }

        public Boundary(string boundary)
        {
            if (boundary.Length > 70)
                //limit according to specification
                boundary = boundary.Substring(0, 70);

            DelimiterData = new byte[Encoding.ASCII.GetByteCount(boundary) + BoundaryPrefix.Length];

            //fill delimiter data
            Encoding.ASCII.GetBytes(boundary, 0, boundary.Length, DelimiterData, BoundaryPrefix.Length);
            Array.Copy(BoundaryPrefix, DelimiterData, BoundaryPrefix.Length);

            //first boundary doesn't have prefixing newline
            Cursor = 2;
        }

        public void AcceptNext(byte[] data, int dataOffset, int dataLength)
        {
            if (IsComplete)
                //starting new boundary search
                Cursor = 0;

            for (int i = dataOffset; i < dataOffset + dataLength; ++i)
            {
                var current = data[i];
                var needed = DelimiterData[Cursor];

                if (current == needed)
                {
                    //delimiter data match
                    ++Cursor;
                }
                else
                {
                    //delimiter doesn't match given data at index i
                    Cursor = 0;
                }


                if (IsComplete)
                {
                    //we reached end offset
                    LocalEndOffset = i;
                    return;
                }
            }

            LocalEndOffset = int.MaxValue;
        }
    }
}

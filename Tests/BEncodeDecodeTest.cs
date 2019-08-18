using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BitTorrent.Test
{
    public class BEncodeDecodeTest : BEncodeBaseTest
    {
        public BEncodeDecodeTest()
        {
            DecodeInt_ReturnTrue();
            DecodeByteArray_ReturnTrue();
            DecodeDictionary_ReturnTrue();
        }

        private void DecodeInt_ReturnTrue()
        {
            var testcase = 2331;
            var bytes = GetBytes($"i{testcase}e");
            var resObj = BEncoding.DecodeNextObject(GetIterator(bytes));

            Debug.Assert(resObj is long);
            var intVal = (long)resObj;
            Debug.Assert(intVal == 2331);
        }

        private void DecodeByteArray_ReturnTrue()
        {
            var testcase = "5:hello";
            var bytes = GetBytes(testcase);
            var decodedResult = BEncoding.DecodeNextObject(GetIterator(bytes)) as byte[];
            var decodedStr = GetString(decodedResult);
            Debug.Assert(decodedStr == "hello");
        }

        private void DecodeDictionary_ReturnTrue()
        {
            var authorInfo = "i'am";
            var infoVal = 331;
            var testcase = $"d4:infoi{infoVal}e6:author{authorInfo.Length}:{authorInfo}";
            var bytes = GetBytes(testcase);
            var result = BEncoding.DecodeNextObject(GetIterator(bytes));

            Debug.Assert(result is IDictionary<string, object>);

            var dict = result as IDictionary<string, object>;

            Debug.Assert(dict.Keys.Contains("info"));
            Debug.Assert(dict.Keys.Contains("author"));
            Debug.Assert((long)dict["info"] == 331);

            var authorBytes = GetBytes(authorInfo);
            var resultAuthor = (byte[])dict["author"];

            Debug.Assert(resultAuthor.Length == authorBytes.Length);
            Debug.Assert(authorBytes.SequenceEqual(resultAuthor));
        }
    }
}
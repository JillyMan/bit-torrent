using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace BitTorrent
{
    public class BEncodeEncodeTest : BEncodeBaseTest
    {
        public BEncodeEncodeTest()
        {
            EncodeInt_ReturnTrue(0);
            EncodeInt_ReturnTrue(23);
            EncodeInt_ReturnTrue(55568885);

            EncodeString_ReturnTrue("");
            EncodeString_ReturnTrue("test_1");

            EncodeList_Return_True(new List<object>() 
            {
                "hello", 
                (long)4323,
                new List<object> 
                {
                    (long)31, 
                    "test-1"
                }
            }, "l5:helloi4323eli31e6:test-1ee");

            EncodeDictionary_ReturnTrue(new Dictionary<string, object>()
            {
                { "author", "i'am" },
                { "info", "test case for info" },
                { "list", new List<object>() 
                    { 
                        "test_1", 
                        "test_2", 
                        (long)231
                    }
                }
            }, "d6:author4:i'am4:info18:test case for info4:listl6:test_16:test_2i231eee");
        }       

        private void EncodeInt_ReturnTrue(long testcase)
        {
            var resultBytes = Encoding.UTF8.GetBytes($"i{testcase}e");
            var bytes = BEncoding.Encode(testcase);
            Debug.Assert(bytes.SequenceEqual(resultBytes));            
        }

        private void EncodeString_ReturnTrue(string testcase)
        {
            var testbytes = GetBytes($"{testcase.Length}:{testcase}");
            var bytes = BEncoding.Encode(testcase);
            Debug.Assert(bytes.SequenceEqual(testbytes));
        }

        private void EncodeList_Return_True(IList<object> list, string expected) 
        {           
            var returnResult = GetString(BEncoding.Encode(list));
            Debug.Assert(returnResult == expected);
        }

        private void EncodeDictionary_ReturnTrue(IDictionary<string, object> input, string expected)
        {
            var encoded = GetString(BEncoding.Encode(input));
            Debug.Assert(encoded == expected);
        }
    }
}
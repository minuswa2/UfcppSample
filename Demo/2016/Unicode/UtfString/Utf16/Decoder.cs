﻿using System;

namespace UtfString.Utf16
{
    public static class Decoder
    {
        public static int GetCount(ushort[] buffer)
        {
            var count = 0;
            foreach (var x in buffer)
                if ((x & 0b1111_1100_0000_0000) != 0b1101_1100_0000_0000)
                    count++;
            return count;
        }

        public static bool TryGetCount(ushort[] buffer, int index, out byte wordCount)
        {
            wordCount = 0;

            if (index >= buffer.Length) return false;
            uint x = buffer[index];
            if ((x & 0b1111_1100_0000_0000) == 0b1101_1000_0000_0000)
            {
                if (index + 1 >= buffer.Length) return false;
                wordCount = 2;
            }
            else
                wordCount = 1;

            return true;
        }

        public static CodePoint Decode(ushort[] buffer, Index index)
        {
            var i = index.index;
            uint x = buffer[i];

            if (index.wordCount == 2)
            {
                uint y = buffer[i + 1];

                var code = (x & 0b0011_1111_1111) + 0b0100_0000;
                code = (code << 10) | (y & 0b0011_1111_1111);
                return new CodePoint(code);
            }
            else
            {
                return new CodePoint(x);
            }
        }

        public static bool TryDecode(ushort[] buffer, ref int index, out CodePoint codePoint)
        {
            codePoint = default(CodePoint);

            if (index >= buffer.Length) return false;

            uint x = buffer[index++];

            if ((x & 0b1111_1100_0000_0000) == 0b1101_1000_0000_0000)
            {
                // サロゲート ペアの処理
                var code = (x & 0b0011_1111_1111) + 0b0100_0000;
                if (index >= buffer.Length) return false;
                x = buffer[index++];
                if ((x & 0b1111_1100_0000_0000) != 0b1101_1100_0000_0000) return false;
                code = (code << 10) | (x & 0b0011_1111_1111);

                codePoint = new CodePoint(code);
                return true;
            }
            else
            {
                // 利用頻度が高い文字はほぼこちら側に来る
                // バッファー内の値を素通し。
                codePoint = new CodePoint(x);
                return true;
            }
        }
    }
}

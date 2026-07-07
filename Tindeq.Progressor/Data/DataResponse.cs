using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Text;

namespace Tindeq.Progressor.Data
{
    public readonly struct DataResponse
    {
        public byte Tag { get; init; }
        public byte Length { get; init; }
        public ReadOnlyMemory<byte> Value { get; init; }

        public DataResponse(byte tag, byte length, ReadOnlyMemory<byte> value)
        {
            Tag = tag;
            Length = length;
            Value = value;
        }
    }
}

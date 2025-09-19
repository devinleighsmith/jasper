using System.Collections.Generic;
using System.IO;

namespace Scv.Api.Documents.Parsers;

public interface ICsvParser
{
    public IEnumerable<T> Parse<T>(MemoryStream csvStream) where T : class;
}
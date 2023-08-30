using Foundation.Common.Entities;
using Foundation.Common.Roles;

namespace Foundation.Services.UPx;

public class Disposal
{
    public required double Credits { get; set; }

    public required float Weight { get; set; }

    public required DisposalType DisposalType { get; set; }
}
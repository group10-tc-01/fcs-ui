using Fcs.UI.Application.Interfaces;

namespace Fcs.UI.Web.Infrastructure;

public sealed class WebConnectivityService : IConnectivityService
{
    public bool IsConnected { get; set; } = true;
    public event EventHandler<bool>? ConnectivityChanged;
}

// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Threading.Tasks;

namespace Softeq.NetKit.Components.EventBus.Abstract
{
    public interface IDynamicEventHandler
    {
        Task Handle(dynamic eventData);
    }
}

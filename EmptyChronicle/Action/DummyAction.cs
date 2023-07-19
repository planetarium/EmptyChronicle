using Bencodex.Types;
using Libplanet.Action;
using Libplanet.State;

namespace EmptyChronicle.Action;

public class DummyAction : IAction
{
    public void LoadPlainValue(IValue plainValue)
    {
    }

    public IAccountStateDelta Execute(IActionContext context)
    {
        return context.PreviousState;
    }

    public IValue PlainValue => Dictionary.Empty;
}
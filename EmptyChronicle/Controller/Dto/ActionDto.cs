using Bencodex;
using Bencodex.Types;
using Libplanet;

namespace EmptyChronicle.Controller.Dto;

public class ActionDto
{
    public string Raw { get; set; }
    public string TypeId { get; set; }
    public string Inspection { get; set; }

    public ActionDto(Dictionary action)
    {
        var codec = new Codec();
        Raw = ByteUtil.Hex(codec.Encode(action));
        TypeId = ((Dictionary)action).GetValue<Text>("type_id");
        Inspection = action.Inspect(true);
    }
}
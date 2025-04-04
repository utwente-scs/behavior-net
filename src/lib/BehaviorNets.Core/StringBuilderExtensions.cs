using System.Text;

namespace BehaviorNets
{
    internal static class StringBuilderExtensions
    {
        public static void AppendValue(this StringBuilder builder, object? value)
        {
            switch (value)
            {
                case null:
                    builder.Append("null");
                    break;

                case long x:
                    builder.Append("0x");
                    builder.Append(x.ToString("X"));
                    break;

                default:
                    builder.Append(value);
                    break;
            }
        }

    }
}

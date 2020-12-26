using Tizen.Applications;
using Uno.UI.Runtime.Skia;

namespace Uno.Validation.Skia.Tizen
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new TizenHost(() => new Uno.Validation.App(), args);
            host.Run();
        }
    }
}

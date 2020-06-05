using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenLibrary
{
    class Plugin : BasePlugin, IHasThumbImage
    {

        public override string Name => "Open Library";

        public override string Description => "Open Library";

        public override Guid Id => new Guid("D863DA9A-B832-4506-B85A-2C50357E89FB");

        public ImageFormat ThumbImageFormat => ImageFormat.Png;

        public Stream GetThumbImage()
        {
            var type = GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".thumb.png");
        }
    }
}

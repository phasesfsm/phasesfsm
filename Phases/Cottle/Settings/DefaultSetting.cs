using System;

namespace Cottle.Settings
{
	public sealed class DefaultSetting : ISetting
	{
        #region Properties / Instance

        public string BlockBegin => "{";

        public string BlockContinue => "|";

        public string BlockEnd => "}";

        public char Escape => '\\';

        public bool Optimize => true;

        public Trimmer Trimmer => (t) => t;

        #endregion

        #region Properties / Static

        public static DefaultSetting Instance => instance;

        #endregion

        #region Attributes

        private static readonly DefaultSetting instance = new DefaultSetting ();

		#endregion
	}
}

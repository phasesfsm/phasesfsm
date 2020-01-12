using System;

namespace Cottle.Settings
{
	public sealed class CustomSetting : ISetting
	{
		#region Properties

		public string BlockBegin { get; set; }

		public string BlockContinue { get; set; }

		public string BlockEnd { get; set; }

		public char Escape { get; set; }

		public bool Optimize { get; set; }

		public Trimmer Trimmer { get; set; }

		#endregion

		#region Attributes

		private string blockBegin = DefaultSetting.Instance.BlockBegin;

		private string blockContinue = DefaultSetting.Instance.BlockContinue;

		private string blockEnd = DefaultSetting.Instance.BlockEnd;

		private char escape = DefaultSetting.Instance.Escape;

		private bool optimize = DefaultSetting.Instance.Optimize;

		private Trimmer trimmer = DefaultSetting.Instance.Trimmer;

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Direct3DLib
{
	public class WndProcMessage
	{
		public const int MIN_MOUSE = 0x0200;
		public const int MAX_MOUSE = 0x02A3;
		public const int WM_MOUSELEAVE = 0x02A3;
		public const int WM_NCMOUSELEAVE = 0x02A2;
		public const int WM_MOUSEMOVE = 0x0200;
		public const int WM_LBUTTONDOWN = 0x0201;
		public const int WM_LBUTTONUP = 0x0202;
		public const int WM_LBUTTONDBLCLK = 0x0203;
		public const int WM_RBUTTONDOWN = 0x0204;
		public const int WM_RBUTTONUP = 0x0205;
		public const int WM_RBUTTONDBLCLK = 0x0206;
		public const int MIN_KEY = 0x0100;
		public const int MAX_KEY = 0x0105;
		public const int WM_KEYDOWN = 0x0100;
		public const int WM_KEYUP = 0x0101;
		public const int WM_CHAR = 0x0102;
		public const int WM_DEADCHAR = 0x0103;
		public const int WM_SYSKEYDOWN = 0x0104;
		public const int WM_SYSKEYUP = 0x0105;
	}
}

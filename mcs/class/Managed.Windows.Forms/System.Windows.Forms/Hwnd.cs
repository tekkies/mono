// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

// NOT COMPLETE

using System;
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	internal class Hwnd : IDisposable {
		#region Local Variables
		private static Hashtable	windows	= new Hashtable(100, 0.5f);
		private const int	menu_height = 14;			// FIXME - Read this value from somewhere
		private const int	caption_height = 0;			// FIXME - Read this value from somewhere
		private const int	tool_caption_height = 0;		// FIXME - Read this value from somewhere

		private GCHandle	gc_handle;
		internal IntPtr		client_window;
		internal IntPtr		whole_window;
		internal bool		has_menu;
		internal TitleStyle	title_style;
		internal BorderStyle	border_style;
		internal Border3DStyle	edge_style;
		internal int		x;
		internal int		y;
		internal int		width;
		internal int		height;
		internal Hwnd		parent;
		internal bool		visible;
		internal Rectangle	invalid;
		internal bool		expose_pending;
		internal bool		nc_expose_pending;
		internal Graphics		client_dc;
		#endregion	// Local Variables

		#region Constructors and destructors
		public Hwnd() {
			gc_handle = GCHandle.Alloc(this);

			x = 0;
			y = 0;
			width = 0;
			height = 0;
			visible = false;
			has_menu = false;
			border_style = BorderStyle.None;
			client_window = IntPtr.Zero;
			whole_window = IntPtr.Zero;
			parent = null;
			invalid = Rectangle.Empty;
			expose_pending = false;
			nc_expose_pending = false;
			edge_style = Border3DStyle.Raised;
		}

		public void Dispose() {
			gc_handle.Free();
		}
		#endregion

		#region	Static Methods
		public void SetObjectWindow(Hwnd obj, IntPtr window) {
			windows[window] = obj;
		}

		public static Hwnd ObjectFromWindow(IntPtr window) {
			return (Hwnd)windows[window];
		}

		public static Hwnd ObjectFromHandle(IntPtr handle) {
			return (Hwnd)(((GCHandle)handle).Target);
		}

		public static IntPtr HandleFromObject(Hwnd obj) {
			return (IntPtr)obj.gc_handle;
		}

		public static Hwnd GetObjectFromWindow(IntPtr window) {
			return (Hwnd)windows[window];
		}

		public static IntPtr GetHandleFromWindow(IntPtr window) {
			Hwnd	hwnd;

			hwnd = (Hwnd)windows[window];
			if (hwnd != null) {
				return (IntPtr)hwnd.gc_handle;
			} else {
				return IntPtr.Zero;
			}
		}

		public static Rectangle GetWindowRectangle(BorderStyle border_style, bool has_menu, TitleStyle title_style, Rectangle client_rect) {
			Rectangle	rect;

			rect = new Rectangle(client_rect.Location, client_rect.Size);

			if (has_menu) {
				rect.Y -= menu_height;
				rect.Height += menu_height;
			}

			if (border_style == BorderStyle.Fixed3D) {
				rect.X -= 2;
				rect.Y -= 2;
				rect.Width += 4;
				rect.Height += 4;
			} else if (border_style == BorderStyle.FixedSingle) {
				rect.X -= 1;
				rect.Y -= 1;
				rect.Width += 2;
				rect.Height += 2;
			}

			if (title_style == TitleStyle.Normal) {
				rect.Y -= caption_height;
				rect.Height += caption_height;
			} else if (title_style == TitleStyle.Tool) {
				rect.Y -= tool_caption_height;
				rect.Height += tool_caption_height;
			}

			return rect;
		}
		#endregion	// Static Methods

		#region Instance Properties
		public BorderStyle BorderStyle {
			get {
				return border_style;
			}

			set {
				border_style = value;
			}
		}

		public Graphics ClientDC {
			get {
				return client_dc;
			}

			set {
				client_dc = value;
			}
		}

		public Rectangle ClientRect {
			get {
				Rectangle rect;

				rect = new Rectangle(0, 0, width, height);

				if (has_menu) {
					rect.Y += menu_height;
					rect.Height -= menu_height;
				}

				if (border_style == BorderStyle.Fixed3D) {
					rect.X += 2;
					rect.Y += 2;
					rect.Width -= 4;
					rect.Height -= 4;
				} else if (border_style == BorderStyle.FixedSingle) {
					rect.X += 1;
					rect.Y += 1;
					rect.Width -= 2;
					rect.Height -= 2;
				}

				if (this.title_style == TitleStyle.Normal)  {
					rect.Y += caption_height;
					rect.Height -= caption_height;
				} else if (this.title_style == TitleStyle.Normal)  {
					rect.Y += tool_caption_height;
					rect.Height -= tool_caption_height;
				}

				return rect;
			}
		}

		public IntPtr ClientWindow {
			get {
				return client_window;
			}

			set {
				client_window = value;

				if (windows[client_window] == null) {
					windows[client_window] = this;
				}
			}
		}

		public Border3DStyle EdgeStyle {
			get {
				return edge_style;
			}

			set {
				edge_style = value;
			}
		}

		public bool ExposePending {
			get {
				return expose_pending;
			}

			set {
				expose_pending = value;
			}
		}

		public IntPtr Handle {
			get {
				return (IntPtr)gc_handle;
			}
		}

		public int Height {
			get {
				return height;
			}

			set {
				height = value;
			}
		}

		public bool HasMenu {
			get {
				return has_menu;
			}

			set {
				has_menu = value;
			}
		}

		public Rectangle Invalid {
			get {
				return invalid;
			}

			set {
				invalid = value;
			}
		}

		public bool NCExposePending {
			get {
				return nc_expose_pending;
			}

			set {
				nc_expose_pending = value;
			}
		}

		public Hwnd Parent {
			get {
				return parent;
			}

			set {
				parent = value;
			}
		}

		public TitleStyle TitleStyle {
			get {
				return title_style;
			}

			set {
				title_style = value;
			}
		}

		public IntPtr WholeWindow {
			get {
				return whole_window;
			}

			set {
				whole_window = value;

				if (windows[whole_window] == null) {
					windows[whole_window] = this;
				}
			}
		}

		public int Width {
			get {
				return width;
			}

			set {
				width = value;
			}
		}

		public bool Visible {
			get {
				return visible;
			}

			set {
				visible = value;
			}
		}

		public int X {
			get {
				return x;
			}

			set {
				x = value;
			}
		}

		public int Y {
			get {
				return y;
			}

			set {
				y = value;
			}
		}
		#endregion	// Instance properties

		#region Methods
		public void AddInvalidArea(int x, int y, int width, int height) {
			if (invalid == Rectangle.Empty) {
				invalid = new Rectangle (x, y, width, height);
				return;
			}
			invalid = Rectangle.Union (invalid, new Rectangle (x, y, width, height));
		}

		public void AddInvalidArea(Rectangle rect) {
			if (invalid == Rectangle.Empty) {
				invalid = rect;
				return;
			}
			invalid = Rectangle.Union (invalid, rect);
		}

		public void ClearInvalidArea() {
			invalid = Rectangle.Empty;
			expose_pending = false;
		}
		#endregion	// Methods
	}
}

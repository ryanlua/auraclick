using System.Runtime.InteropServices;
using System.Text;

namespace AuraClick.Helpers;

/// <summary>
/// A top-level window the auto clicker can target.
/// </summary>
public sealed class WindowEntry(nint handle, string title)
{
    /// <summary>
    /// Gets the window handle.
    /// </summary>
    public nint Handle { get; } = handle;

    /// <summary>
    /// Gets the window title.
    /// </summary>
    public string Title { get; } = title;

    /// <inheritdoc />
    public override string ToString() => Title;
}

/// <summary>
/// Helpers for listing top-level windows and bringing one to the foreground.
/// </summary>
public static class WindowHelper
{
    private const int SwRestore = 9;
    private const int DwmaCloaked = 14;

    private delegate bool EnumWindowsProc(nint hWnd, nint lParam);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc callback, nint lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(nint hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindow(nint hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(nint hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(nint hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(nint hWnd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(nint hWnd, out uint processId);

    [DllImport("user32.dll")]
    private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextLength(nint hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(nint hWnd, StringBuilder text, int maxCount);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(nint hWnd, int attribute, out int value, int size);

    /// <summary>
    /// Lists visible, titled top-level windows, excluding <paramref name="exclude" />.
    /// </summary>
    public static List<WindowEntry> GetOpenWindows(nint exclude)
    {
        List<WindowEntry> result = [];
        _ = EnumWindows((hWnd, _) =>
        {
            if (hWnd == exclude || !IsWindowVisible(hWnd))
            {
                return true;
            }

            // Skip hidden UWP shells that report as visible but are cloaked
            if (DwmGetWindowAttribute(hWnd, DwmaCloaked, out int cloaked, sizeof(int)) == 0 && cloaked != 0)
            {
                return true;
            }

            int length = GetWindowTextLength(hWnd);
            if (length <= 0)
            {
                return true;
            }

            StringBuilder sb = new(length + 1);
            _ = GetWindowText(hWnd, sb, sb.Capacity);
            string title = sb.ToString().Trim();
            if (title.Length > 0)
            {
                result.Add(new WindowEntry(hWnd, title));
            }

            return true;
        }, 0);

        result.Sort((a, b) => string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase));
        return result;
    }

    /// <summary>
    /// Returns whether the window still exists.
    /// </summary>
    public static bool Exists(nint hWnd) => hWnd != 0 && IsWindow(hWnd);

    /// <summary>
    /// Brings the window to the foreground and returns the previously active window.
    /// </summary>
    public static nint BringToFront(nint target)
    {
        nint previous = GetForegroundWindow();
        if (previous == target)
        {
            return previous;
        }

        if (IsIconic(target))
        {
            _ = ShowWindow(target, SwRestore);
        }

        ForceForeground(target, previous);
        return previous;
    }

    /// <summary>
    /// Returns focus to the window that was active before <see cref="BringToFront" />.
    /// </summary>
    public static void Restore(nint previous, nint target)
    {
        if (previous != 0 && previous != target && IsWindow(previous))
        {
            ForceForeground(previous, GetForegroundWindow());
        }
    }

    private static void ForceForeground(nint window, nint currentForeground)
    {
        uint foregroundThread = currentForeground != 0 ? GetWindowThreadProcessId(currentForeground, out _) : 0;
        uint thisThread = GetCurrentThreadId();
        bool attached = foregroundThread != 0 && foregroundThread != thisThread
            && AttachThreadInput(thisThread, foregroundThread, true);

        _ = SetForegroundWindow(window);

        if (attached)
        {
            _ = AttachThreadInput(thisThread, foregroundThread, false);
        }
    }
}
